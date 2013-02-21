using System;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class XmlTextSerializer : IXmlSerializer
    {
        private readonly Type _type;

        public XmlTextSerializer(Type type)
        {
            _type = type;
        }

        public void SerializeObject(object value, SerializationXmlTextWriter writer, XmlSerializerNamespaces namespaces)
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