using System;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class XmlTextSerializer : IXmlSerializerInternal
    {
        private readonly IValueConverter _valueConverter;

        public XmlTextSerializer(Type type, RedactAttribute redactAttribute, EncryptAttribute encryptAttribute, Type[] extraTypes)
        {
            if (!ValueTypes.TryGetValueConverter(type, redactAttribute, encryptAttribute, extraTypes, out _valueConverter))
            {
                _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute, encryptAttribute);
            }
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                writer.WriteValue(_valueConverter.GetString(value, options));
            }
        }

        public object DeserializeObject(XmlReader reader, ISerializeOptions options)
        {
            return _valueConverter.ParseString(reader.Value, options);
        }
    }
}