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

                    if (type.IsPrimitiveLike()
                            || type.IsNullablePrimitiveLike()
                            || ValueTypes.IsRegistered(type))
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
                    else
                    {
                        serializer = (IXmlSerializerInternal<T>)CustomSerializer.GetSerializer(type, options);
                    }

                    return serializer;
                });
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