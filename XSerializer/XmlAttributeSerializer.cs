using System;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class XmlAttributeSerializer : IXmlSerializer
    {
        private readonly string _attributeName;
        private readonly Type _type;

        public XmlAttributeSerializer(string attributeName, Type type)
        {
            _attributeName = attributeName;
            _type = type;
        }

        public void SerializeObject(object value, SerializationXmlTextWriter writer, XmlSerializerNamespaces namespaces)
        {
            if (value != null)
            {
                if (_type == typeof(bool))
                {
                    writer.WriteAttributeString(_attributeName, value.ToString().ToLower());
                }
                else
                {
                    writer.WriteAttributeString(_attributeName, value.ToString());
                }
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            if (reader.MoveToAttribute(_attributeName))
            {
                var value = Convert.ChangeType(reader.Value, _type);
                reader.MoveToElement();
                return value;
            }

            return null;
        }
    }
}