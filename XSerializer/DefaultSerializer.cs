using System;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    internal class DefaultSerializer
    {
        private static readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

        [Obsolete("Use generic GetSerializer<T> method instead.")]
        public static IXmlSerializerInternal GetSerializer(Type type, IXmlSerializerOptions options)
        {
            return _serializerCache.GetOrAdd(
                XmlSerializerFactory.Instance.CreateKey(type, options),
                _ =>
                {
                    try
                    {
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(DefaultSerializer<>).MakeGenericType(type), options);
                    }
                    catch
                    {
                        return null;
                    }
                });
        }

        public static IXmlSerializerInternal<T> GetSerializer<T>(IXmlSerializerOptions options)
        {
            IXmlSerializerInternal serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(typeof(T), options);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                try
                {
                    serializer = new DefaultSerializer<T>(options);
                }
                catch
                {
                    serializer = null;
                }

                _serializerCache[key] = serializer;
            }

            return (IXmlSerializerInternal<T>)serializer;
        }
    }

    internal class DefaultSerializer<T> : DefaultSerializer, IXmlSerializerInternal<T>
    {
        private readonly System.Xml.Serialization.XmlSerializer _serializer;

        public DefaultSerializer(IXmlSerializerOptions options)
        {
            _serializer = new System.Xml.Serialization.XmlSerializer(
                typeof(T),
                null,
                options.ExtraTypes,
                string.IsNullOrWhiteSpace(options.RootElementName) ? null : new XmlRootAttribute(options.RootElementName),
                options.DefaultNamespace);
        }

        public void Serialize(SerializationXmlTextWriter writer, T instance, ISerializeOptions options)
        {
            _serializer.Serialize(writer, instance, options.Namespaces);
        }

        void IXmlSerializerInternal.SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            Serialize(writer, (T)instance, options);
        }

        public T Deserialize(XmlReader reader)
        {
            return (T)_serializer.Deserialize(reader);
        }

        object IXmlSerializerInternal.DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }
    }
}