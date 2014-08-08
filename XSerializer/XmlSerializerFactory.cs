using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace XSerializer
{
    internal class XmlSerializerFactory
    {
        private readonly ConcurrentDictionary<Type, Func<IXmlSerializerOptions, IXmlSerializerInternal>> _getSerializerMap = new ConcurrentDictionary<Type, Func<IXmlSerializerOptions, IXmlSerializerInternal>>();
        private readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

        public static readonly XmlSerializerFactory Instance = new XmlSerializerFactory();

        private XmlSerializerFactory()
        {
        }

        public IXmlSerializerInternal GetSerializer(Type type, IXmlSerializerOptions options)
        {
            var getSerializer = _getSerializerMap.GetOrAdd(
                type,
                t =>
                    {
                        var getSerializerMethod =
                            typeof(XmlSerializerFactory)
                                .GetMethod("GetSerializer", new[] { typeof(IXmlSerializerOptions) })
                                .MakeGenericMethod(type);

                        return (Func<IXmlSerializerOptions, IXmlSerializerInternal>)Delegate.CreateDelegate(typeof(Func<IXmlSerializerOptions, IXmlSerializerInternal>), this, getSerializerMethod);
                    });

            return getSerializer(options);
        }

        public IXmlSerializerInternal<T> GetSerializer<T>(IXmlSerializerOptions options)
        {
            return (IXmlSerializerInternal<T>)_serializerCache.GetOrAdd(
                CreateKey(typeof(T), options),
                _ =>
                {
                    var type = typeof(T);

                    if (type == typeof(object) || type == typeof(ExpandoObject))
                    {
                        return DynamicSerializer.GetSerializer<T>(options);
                    }

                    IXmlSerializerInternal<T> serializer;

                    if (!TryGetDefaultSerializer(options, out serializer))
                    {
                        if (type.IsPrimitiveLike() || type.IsNullablePrimitiveLike())
                        {
                            serializer = new XmlElementSerializer<T>(options);
                        }
                        else if (type.IsAssignableToNonGenericIDictionary() || type.IsAssignableToGenericIDictionary())
                        {
                            serializer = (IXmlSerializerInternal<T>)DictionarySerializer.GetSerializer(type, options);
                        }
                        else if (type.IsAssignableToNonGenericIEnumerable() || type.IsAssignableToGenericIEnumerable())
                        {
                            serializer = (IXmlSerializerInternal<T>)ListSerializer.GetSerializer(type, options, null);
                        }
                        else if (type == typeof(Enum))
                        {
                            serializer = new XmlElementSerializer<T>(options);
                        }
                        else if (type == typeof(Type))
                        {
                            serializer = new XmlElementSerializer<T>(options);
                        }
                        else
                        {
                            serializer = (IXmlSerializerInternal<T>)CustomSerializer.GetSerializer(type, options);
                        }
                    }

                    return serializer;
                });
        }

        protected virtual bool TryGetDefaultSerializer<T>(IXmlSerializerOptions options, out IXmlSerializerInternal<T> serializer)
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
            if (type == typeof(Type))
            {
                return true;
            }

            if (type == typeof(Enum))
            {
                return true;
            }

            if (type.IsArray)
            {
                return ShouldNotAttemptToUseDefaultSerializer(type.GetElementType(), options);
            }

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

            var constructorParameters = type.GetConstructors().SelectMany(c => c.GetParameters());

            return allTypes
                .SelectMany(t => t.GetProperties())
                .Where(p => p.IsSerializable(constructorParameters))
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

                key = (key * 397) ^ options.TreatEmptyElementAsString.GetHashCode();

                return key;
            }
        }
    }
}