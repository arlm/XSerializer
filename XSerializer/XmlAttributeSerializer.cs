using System;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class XmlAttributeSerializer : IXmlSerializerInternal
    {
        private readonly string _attributeName;
        private readonly IValueConverter _valueConverter;

        public XmlAttributeSerializer(Type type, string attributeName, RedactAttribute redactAttribute, EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            _attributeName = attributeName;

            if (!ValueTypes.TryGetValueConverter(type, redactAttribute, encryptAttribute, options.ExtraTypes, out _valueConverter))
            {
                _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute, encryptAttribute);
            }
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                writer.WriteAttributeString(_attributeName, _valueConverter.GetString(value, options));
            }
        }

        public object DeserializeObject(XmlReader reader, ISerializeOptions options)
        {
            if (reader.MoveToAttribute(_attributeName))
            {
                var value = _valueConverter.ParseString(reader.Value, options);
                reader.MoveToElement();
                return value;
            }

            return null;
        }
    }
}