using System;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class XmlAttributeSerializer : IXmlSerializerInternal
    {
        private readonly string _attributeName;
        private readonly EncryptAttribute _encryptAttribute;
        private readonly IValueConverter _valueConverter;

        public XmlAttributeSerializer(Type type, string attributeName, RedactAttribute redactAttribute, EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            _attributeName = attributeName;
            _encryptAttribute = encryptAttribute;

            if (!ValueTypes.TryGetValueConverter(type, redactAttribute, options.ExtraTypes, out _valueConverter))
            {
                _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute);
            }
        }

        public void SerializeObject(XSerializerXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                writer.WriteStartAttribute(_attributeName); // TODO: include namespaces
                
                var setToFalse = writer.MaybeSetIsEncryptionEnabled(_encryptAttribute);

                writer.WriteString(_valueConverter.GetString(value, options));

                if (setToFalse)
                {
                    writer.IsEncryptionEnabled = false;
                }

                writer.WriteEndAttribute();
            }
        }

        public object DeserializeObject(XSerializerXmlReader reader, ISerializeOptions options)
        {
            if (reader.MoveToAttribute(_attributeName))
            {
                var setToFalse = reader.MaybeSetIsDecryptionEnabled(_encryptAttribute);

                var value = _valueConverter.ParseString(reader.Value, options);

                if (setToFalse)
                {
                    reader.IsDecryptionEnabled = false;
                }

                reader.MoveToElement();
                return value;
            }

            return null;
        }
    }
}