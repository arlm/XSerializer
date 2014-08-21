using System;
using System.Xml;

namespace XSerializer
{
    internal class XmlAttributeSerializer : IXmlSerializerInternal
    {
        private readonly string _attributeName;
        private readonly IValueConverter _valueConverter;

        public XmlAttributeSerializer(Type type, string attributeName, RedactAttribute redactAttribute, IXmlSerializerOptions options)
        {
            _attributeName = attributeName;

            if (type == typeof(Enum))
            {
                _valueConverter = new EnumTypeValueConverter(redactAttribute, options.ExtraTypes);
            }
            else if (type == typeof(Type))
            {
                _valueConverter = new TypeTypeValueConverter(options.RedactAttribute);
            }
            else if (type == typeof(Uri))
            {
                _valueConverter = new UriTypeValueConverter(options.RedactAttribute);
            }
            else
            {
                _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute);
            }
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