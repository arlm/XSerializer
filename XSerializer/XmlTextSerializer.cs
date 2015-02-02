using System;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class XmlTextSerializer : IXmlSerializerInternal
    {
        private readonly EncryptAttribute _encryptAttribute;
        private readonly IValueConverter _valueConverter;

        public XmlTextSerializer(Type type, RedactAttribute redactAttribute, EncryptAttribute encryptAttribute, Type[] extraTypes)
        {
            _encryptAttribute = encryptAttribute;
            if (!ValueTypes.TryGetValueConverter(type, redactAttribute, extraTypes, out _valueConverter))
            {
                _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute);
            }
        }

        public void SerializeObject(XSerializerXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                var setToFalse = writer.MaybeSetIsEncryptionEnabled(_encryptAttribute);

                writer.WriteValue(_valueConverter.GetString(value, options));

                if (setToFalse)
                {
                    writer.IsEncryptionEnabled = false;
                }
            }
        }

        public object DeserializeObject(XSerializerXmlReader reader, ISerializeOptions options)
        {
            var setToFalse = reader.MaybeSetIsDecryptionEnabled(_encryptAttribute);

            var value = _valueConverter.ParseString(reader.Value, options);

            if (setToFalse)
            {
                reader.IsDecryptionEnabled = false;
            }

            return value;
        }
    }
}