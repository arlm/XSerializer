using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class XmlSerializerFactory
    {
        private readonly ConcurrentDictionary<Type, Func<EncryptAttribute, IXmlSerializerOptions, IXmlSerializerInternal>> _getSerializerMap = new ConcurrentDictionary<Type, Func<EncryptAttribute, IXmlSerializerOptions, IXmlSerializerInternal>>();
        private readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

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
                CreateKey(typeof(T), encryptAttribute, options),
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

        internal int CreateKey(Type type, EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
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

                key = (key * 397) ^ (encryptAttribute != null).GetHashCode();

                key = (key * 397) ^ options.TreatEmptyElementAsString.GetHashCode();

                return key;
            }
        }
    }
}