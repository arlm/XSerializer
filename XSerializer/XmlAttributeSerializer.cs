using System;
using System.Xml;

namespace XSerializer
{
    internal class XmlAttributeSerializer : IXmlSerializerInternal
    {
        private readonly string _attributeName;
        private readonly SimpleTypeValueConverter _valueConverter;

        public XmlAttributeSerializer(Type type, string attributeName, RedactAttribute redactAttribute)
        {
            _attributeName = attributeName;
            _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute);
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                writer.WriteAttributeString(_attributeName, _valueConverter.GetString(value, options));
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            if (reader.MoveToAttribute(_attributeName))
            {
                var value = _valueConverter.ParseString(reader.Value);
                reader.MoveToElement();
                return value;
            }

            return null;
        }
    }
}