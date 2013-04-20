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
        public static IXmlSerializer GetSerializer(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            IXmlSerializer serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(type, defaultNamespace, extraTypes, rootElementName);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                try
                {
                    serializer = (IXmlSerializer)Activator.CreateInstance(typeof(DefaultSerializer<>).MakeGenericType(type), defaultNamespace, extraTypes, rootElementName);
                }
                catch
                {
                    serializer = null;
                }
                
                _serializerCache[key] = serializer;
            }

            return serializer;
        }

        public static IXmlSerializer<T> GetSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            IXmlSerializer serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(typeof(T), defaultNamespace, extraTypes, rootElementName);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                try
                {
                    serializer = new DefaultSerializer<T>(defaultNamespace, extraTypes, rootElementName);
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

        public DefaultSerializer(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            _serializer = new XmlSerializer(
                typeof(T),
                null,
                extraTypes,
                string.IsNullOrWhiteSpace(rootElementName) ? null : new XmlRootAttribute(rootElementName),
                defaultNamespace);
        }

        public void Serialize(SerializationXmlTextWriter writer, T instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            _serializer.Serialize(writer, instance, namespaces);
        }

        void IXmlSerializer.SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            Serialize(writer, (T)instance, namespaces, alwaysEmitTypes);
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