using System;
using System.Collections.Concurrent;
using System.Dynamic;
using XSerializer.Encryption;
using CacheKey = System.Tuple<System.Type, XSerializer.Encryption.EncryptAttribute, XSerializer.IXmlSerializerOptions>;

namespace XSerializer
{
    internal class XmlSerializerFactory
    {
        private readonly ConcurrentDictionary<Type, Func<EncryptAttribute, IXmlSerializerOptions, IXmlSerializerInternal>> _getSerializerMap = new ConcurrentDictionary<Type, Func<EncryptAttribute, IXmlSerializerOptions, IXmlSerializerInternal>>();
        private readonly ConcurrentDictionary<CacheKey, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<CacheKey, IXmlSerializerInternal>(new CacheKeyEqualityComparer());

        public static readonly XmlSerializerFactory Instance = new XmlSerializerFactory();

        private XmlSerializerFactory()
        {
        }

        public IXmlSerializerInternal GetSerializer(Type type, EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            var getSerializer = _getSerializerMap.GetOrAdd(
                type,
                t =>
                    {
                        var getSerializerMethod =
                            typeof(XmlSerializerFactory)
                                .GetMethod("GetSerializer", new[] { typeof(EncryptAttribute), typeof(IXmlSerializerOptions) })
                                .MakeGenericMethod(type);

                        return (Func<EncryptAttribute, IXmlSerializerOptions, IXmlSerializerInternal>)Delegate.CreateDelegate(typeof(Func<EncryptAttribute, IXmlSerializerOptions, IXmlSerializerInternal>), this, getSerializerMethod);
                    });

            return getSerializer(encryptAttribute, options);
        }

        public IXmlSerializerInternal GetSerializer<T>(EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            return _serializerCache.GetOrAdd(
                Tuple.Create(typeof(T), encryptAttribute, options),
                _ =>
                {
                    var type = typeof(T);

                    if (type == typeof(object) || type == typeof(ExpandoObject))
                    {
                        return DynamicSerializer.GetSerializer<T>(encryptAttribute, options);
                    }

                    IXmlSerializerInternal serializer;

                    if (type.IsPrimitiveLike()
                        || type.IsNullablePrimitiveLike()
                        || ValueTypes.IsRegistered(type))
                    {
                        serializer = new XmlElementSerializer<T>(encryptAttribute, options);
                    }
                    else if (type.IsAssignableToNonGenericIDictionary() || type.IsAssignableToGenericIDictionary() || type.IsReadOnlyDictionary())
                    {
                        serializer = DictionarySerializer.GetSerializer(type, encryptAttribute, options);
                    }
                    else if (type.IsAssignableToNonGenericIEnumerable() || type.IsAssignableToGenericIEnumerable())
                    {
                        serializer = ListSerializer.GetSerializer(type, encryptAttribute, options, null);
                    }
                    else
                    {
                        serializer = CustomSerializer.GetSerializer(type, encryptAttribute, options);
                    }

                    return serializer;
                });
        }
    }
}