using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class DefaultSerializer
    {
        private static readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();

        [Obsolete("Use generic GetSerializer<T> method instead.")]
        public static IXmlSerializer GetSerializer(Type type, IXmlSerializerOptions options)
        {
            IXmlSerializer serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(type, options);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                try
                {
                    serializer = (IXmlSerializer)Activator.CreateInstance(typeof(DefaultSerializer<>).MakeGenericType(type), options);
                }
                catch
                {
                    serializer = null;
                }
                
                _serializerCache[key] = serializer;
            }

            return serializer;
        }

        public static IXmlSerializer<T> GetSerializer<T>(IXmlSerializerOptions options)
        {
            IXmlSerializer serializer;
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

            return (IXmlSerializer<T>)serializer;
        }
    }

    public class DefaultSerializer<T> : DefaultSerializer, IXmlSerializer<T>
    {
        private readonly XmlSerializer _serializer;

        public DefaultSerializer(IXmlSerializerOptions options)
        {
            _serializer = new XmlSerializer(
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

        void IXmlSerializer.SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            Serialize(writer, (T)instance, options);
        }

        public T Deserialize(XmlReader reader)
        {
            return (T)_serializer.Deserialize(reader);
        }

        object IXmlSerializer.DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }
    }
}