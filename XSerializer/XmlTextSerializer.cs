using System;
using System.Xml;

namespace XSerializer
{
    internal class XmlTextSerializer : IXmlSerializerInternal
    {
        private readonly IValueConverter _valueConverter;

        public XmlTextSerializer(Type type, RedactAttribute redactAttribute, Type[] extraTypes)
        {
            if (!ValueTypes.TryGetValueConverter(type, redactAttribute, extraTypes, out _valueConverter))
            {
                _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute);
            }
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                writer.WriteValue(_valueConverter.GetString(value, options));
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            return _valueConverter.ParseString(reader.Value);
        }
    }
}