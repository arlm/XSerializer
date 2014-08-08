using System;
using System.Collections.Generic;
using System.Xml;

namespace XSerializer
{
    internal class XmlTextSerializer : IXmlSerializerInternal
    {
        private readonly IValueConverter _valueConverter;

        public XmlTextSerializer(Type type, RedactAttribute redactAttribute, IEnumerable<Type> extraTypes)
        {
            if (type == typeof(Enum))
            {
                _valueConverter = new EnumTypeValueConverter(redactAttribute, extraTypes);
            }
            else if (type == typeof(Type))
            {
                _valueConverter = new TypeTypeValueConverter(redactAttribute);
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
                writer.WriteValue(_valueConverter.GetString(value, options));
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            return _valueConverter.ParseString(reader.Value);
        }
    }
}