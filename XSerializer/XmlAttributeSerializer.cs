using System;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class XmlAttributeSerializer : IXmlSerializer
    {
        private readonly string _attributeName;
        private readonly Type _type;
        private readonly Func<string, object> _parseValue;

        public XmlAttributeSerializer(string attributeName, Type type)
        {
            _attributeName = attributeName;
            _type = type;

            if (_type.IsEnum)
            {
                _parseValue = value => Enum.Parse(_type, value);
            }
            else
            {
                _parseValue = value => Convert.ChangeType(value, _type);
            }
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
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
                var value = _parseValue(reader.Value);
                reader.MoveToElement();
                return value;
            }

            return null;
        }
    }
}