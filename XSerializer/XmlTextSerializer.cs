using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class XmlTextSerializer : IXmlSerializer
    {
        private static readonly Dictionary<Type, XmlTextSerializer> Map = new Dictionary<Type, XmlTextSerializer>();

        private readonly Type _type;

        private XmlTextSerializer(Type type)
        {
            _type = type;
        }

        public static XmlTextSerializer GetSerializer(Type type)
        {
            XmlTextSerializer serializer;
            if (!Map.TryGetValue(type, out serializer))
            {
                serializer = new XmlTextSerializer(type);
                Map[type] = serializer;
            }
            return serializer;
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            if (value != null)
            {
                writer.WriteValue(value);
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Convert.ChangeType(reader.Value, _type);
        }
    }
}