using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace XSerializer
{
    public class XmlSerializerFactory
    {
        private readonly Dictionary<Type, Func<string, Type[], string, IXmlSerializer>> _getSerializerMap = new Dictionary<Type, Func<string, Type[], string, IXmlSerializer>>();
        private readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();

        public static readonly XmlSerializerFactory Instance = new XmlSerializerFactory();

        private XmlSerializerFactory()
        {
        }

        public IXmlSerializer GetSerializer(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            Func<string, Type[], string, IXmlSerializer> getSerializer;
            if (!_getSerializerMap.TryGetValue(type, out getSerializer))
            {
                var getSerializerMethod =
                    typeof(XmlSerializerFactory)
                        .GetMethod("GetSerializer", new[] { typeof(string), typeof(Type[]), typeof(string) })
                        .MakeGenericMethod(type);

                getSerializer =
                    (Func<string, Type[], string, IXmlSerializer>)Delegate.CreateDelegate(
                        typeof(Func<string, Type[], string, IXmlSerializer>), this, getSerializerMethod);
                _getSerializerMap[type] = getSerializer;
            }

            return getSerializer(defaultNamespace, extraTypes, rootElementName);
        }

        public IXmlSerializer<T> GetSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            IXmlSerializer<T> serializer;

            if (!TryGetCachedSerializer(defaultNamespace, extraTypes, rootElementName, out serializer))
            {
                var type = typeof(T);

                if (type == typeof(object) || type == typeof(ExpandoObject))
                {
                    serializer = DynamicSerializer.GetSerializer<T>(defaultNamespace, extraTypes, rootElementName);
                }
                else if (!TryGetDefaultSerializer(defaultNamespace, extraTypes, rootElementName, out serializer))
                {
                    if (type.IsEnum)
                    {
                        serializer = new EnumSerializer<T>(rootElementName);
                    }
                    else if (type.IsAssignableToNonGenericIDictionary() || type.IsAssignableToGenericIDictionary())
                    {
                        serializer = (IXmlSerializer<T>)DictionarySerializer.GetSerializer(type, defaultNamespace, extraTypes, rootElementName);
                    }
                    else if (type != typeof(string) && (type.IsAssignableToNonGenericIEnumerable() || type.IsAssignableToGenericIEnumerable()))
                    {
                        serializer = (IXmlSerializer<T>)ListSerializer.GetSerializer(type, defaultNamespace, extraTypes, rootElementName, null);
                    }
                    else
                    {
                        serializer = (IXmlSerializer<T>)CustomSerializer.GetSerializer(type, defaultNamespace, extraTypes, rootElementName);
                    }
                }

                CacheSerializer(defaultNamespace, extraTypes, rootElementName, serializer);
            }

            return serializer;
        }

        protected virtual bool TryGetCachedSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName, out IXmlSerializer<T> serializer)
        {
            IXmlSerializer serializerObject;
            if (_serializerCache.TryGetValue(CreateKey(typeof(T), defaultNamespace, extraTypes, rootElementName), out serializerObject))
            {
                serializer = (IXmlSerializer<T>)serializerObject;
                return true;
            }

            serializer = null;
            return false;
        }

        protected virtual bool TryGetDefaultSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName, out IXmlSerializer<T> serializer)
        {
            if (ShouldNotAttemptToUseDefaultSerializer(typeof(T), extraTypes))
            {
                serializer = null;
                return false;
            }

            serializer = DefaultSerializer.GetSerializer<T>(defaultNamespace, extraTypes, rootElementName);
            return serializer != null;
        }

        private bool ShouldNotAttemptToUseDefaultSerializer(Type type, ICollection<Type> extraTypes)
        {
            var allTypes = new[] { type }.Concat(extraTypes ?? new Type[0]).ToList();

            if (allTypes.Any(t => IsObjectLike(t)))
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
                .Any(p => ShouldNotAttemptToUseDefaultSerializer(p.PropertyType, new Type[0]));
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

        protected virtual void CacheSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName, IXmlSerializer<T> serializer)
        {
            _serializerCache[CreateKey(typeof(T), defaultNamespace, extraTypes, rootElementName)] = serializer;
        }

        internal int CreateKey(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            unchecked
            {
                var key = type.GetHashCode();

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(defaultNamespace) ? "" : defaultNamespace).GetHashCode();

                if (extraTypes != null)
                {
                    key = extraTypes
                        .Where(extraType => extraType != null)
                        .Distinct(EqualityComparer<Type>.Default)
                        .OrderBy(extraType => extraType.FullName)
                        .Aggregate(key, (current, extraType) => (current * 397) ^ extraType.GetHashCode());
                }

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(rootElementName) ? type.Name : rootElementName).GetHashCode();

                return key;
            }
        }
    }
}