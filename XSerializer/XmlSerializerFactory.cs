using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace XSerializer
{
    public class XmlSerializerFactory
    {
        private readonly Dictionary<Type, Func<IXmlSerializerOptions, IXmlSerializer>> _getSerializerMap = new Dictionary<Type, Func<IXmlSerializerOptions, IXmlSerializer>>();
        private readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();

        public static readonly XmlSerializerFactory Instance = new XmlSerializerFactory();

        private XmlSerializerFactory()
        {
        }

        public IXmlSerializer GetSerializer(Type type, IXmlSerializerOptions options)
        {
            Func<IXmlSerializerOptions, IXmlSerializer> getSerializer;
            if (!_getSerializerMap.TryGetValue(type, out getSerializer))
            {
                var getSerializerMethod =
                    typeof(XmlSerializerFactory)
                        .GetMethod("GetSerializer", new[] { typeof(IXmlSerializerOptions) })
                        .MakeGenericMethod(type);

                getSerializer =
                    (Func<IXmlSerializerOptions, IXmlSerializer>)Delegate.CreateDelegate(
                        typeof(Func<IXmlSerializerOptions, IXmlSerializer>), this, getSerializerMethod);
                _getSerializerMap[type] = getSerializer;
            }

            return getSerializer(options);
        }

        public IXmlSerializer<T> GetSerializer<T>(IXmlSerializerOptions options)
        {
            IXmlSerializer<T> serializer;

            if (!TryGetCachedSerializer(options, out serializer))
            {
                var type = typeof(T);

                if (type == typeof(object) || type == typeof(ExpandoObject))
                {
                    serializer = DynamicSerializer.GetSerializer<T>(options);
                }
                else if (!TryGetDefaultSerializer(options, out serializer))
                {
                    if (type.IsPrimitiveLike() || type.IsNullablePrimitiveLike())
                    {
                        serializer = new XmlElementSerializer<T>(options);
                    }
                    else if (type.IsAssignableToNonGenericIDictionary() || type.IsAssignableToGenericIDictionary())
                    {
                        serializer = (IXmlSerializer<T>)DictionarySerializer.GetSerializer(type, options);
                    }
                    else if (type.IsAssignableToNonGenericIEnumerable() || type.IsAssignableToGenericIEnumerable())
                    {
                        serializer = (IXmlSerializer<T>)ListSerializer.GetSerializer(type, options, null);
                    }
                    else
                    {
                        serializer = (IXmlSerializer<T>)CustomSerializer.GetSerializer(type, options);
                    }
                }

                CacheSerializer(options, serializer);
            }

            return serializer;
        }

        protected virtual bool TryGetCachedSerializer<T>(IXmlSerializerOptions options, out IXmlSerializer<T> serializer)
        {
            IXmlSerializer serializerObject;
            if (_serializerCache.TryGetValue(CreateKey(typeof(T), options), out serializerObject))
            {
                serializer = (IXmlSerializer<T>)serializerObject;
                return true;
            }

            serializer = null;
            return false;
        }

        protected virtual bool TryGetDefaultSerializer<T>(IXmlSerializerOptions options, out IXmlSerializer<T> serializer)
        {
            if (ShouldNotAttemptToUseDefaultSerializer(typeof(T), options))
            {
                serializer = null;
                return false;
            }

            serializer = DefaultSerializer.GetSerializer<T>(options);
            return serializer != null;
        }

        private bool ShouldNotAttemptToUseDefaultSerializer(Type type, IXmlSerializerOptions options)
        {
            if (options == null)
            {
                options = new XmlSerializationOptions();
            }

            if (options.RedactAttribute != null)
            {
                return true;
            }

            var allTypes = new[] { type }.Concat(options.ExtraTypes ?? new Type[0]).ToList();

            if (allTypes.Any(IsObjectLike))
            {
                return true;
            }

            if (allTypes.All(t => t.IsPrimitiveLike()))
            {
                return false;
            }

            return allTypes
                .SelectMany(t => t.GetProperties())
                .Where(p => p.IsSerializable())
                .Any(p => 
                    Attribute.IsDefined(p, typeof(RedactAttribute))
                    || ShouldNotAttemptToUseDefaultSerializer(p.PropertyType, null));
        }

        private bool IsObjectLike(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            if (type.IsInterface)
            {
                return true;
            }

            if (type == typeof(object))
            {
                return true;
            }

            if (type.IsAssignableToNonGenericIDictionary() || type.IsAssignableToGenericIDictionary())
            {
                return true;
            }

            if (type.IsAssignableToGenericIEnumerableOfTypeObject())
            {
                return true;
            }

            if (type.IsAssignableToNonGenericIEnumerable() && !type.IsAssignableToGenericIEnumerable())
            {
                return true;
            }

            if (type.IsAssignableToGenericIEnumerable())
            {
                if (type.GetGenericIEnumerableType().GetGenericArguments()
                    .Where(genericArgumentType => !genericArgumentType.IsPrimitiveLike())
                    .Any(genericArgumentType => ShouldNotAttemptToUseDefaultSerializer(genericArgumentType, null)))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void CacheSerializer<T>(IXmlSerializerOptions options, IXmlSerializer<T> serializer)
        {
            _serializerCache[CreateKey(typeof(T), options)] = serializer;
        }

        internal int CreateKey(Type type, IXmlSerializerOptions options)
        {
            unchecked
            {
                var key = type.GetHashCode();

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(options.DefaultNamespace) ? "" : options.DefaultNamespace).GetHashCode();

                if (options.ExtraTypes != null)
                {
                    key = options.ExtraTypes
                        .Where(extraType => extraType != null)
                        .Distinct(EqualityComparer<Type>.Default)
                        .OrderBy(extraType => extraType.FullName)
                        .Aggregate(key, (current, extraType) => (current * 397) ^ extraType.GetHashCode());
                }

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(options.RootElementName) ? type.Name : options.RootElementName).GetHashCode();

                if (options.RedactAttribute != null)
                {
                    key = (key * 397) ^ options.RedactAttribute.GetHashCode();
                }

                return key;
            }
        }
    }
}