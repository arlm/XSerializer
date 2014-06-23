using System;
using System.Collections.Concurrent;
using System.Xml;

namespace XSerializer
{
    internal class XmlTextSerializer : IXmlSerializerInternal
    {
        private static readonly ConcurrentDictionary<int, XmlTextSerializer> _map = new ConcurrentDictionary<int, XmlTextSerializer>();

        private readonly IValueConverter _valueConverter;

        private XmlTextSerializer(Type type, RedactAttribute redactAttribute)
        {
            _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute);
        }

        public static XmlTextSerializer GetSerializer(Type type, RedactAttribute redactAttribute)
        {
            return _map.GetOrAdd(
                CreateKey(type, redactAttribute),
                _ => new XmlTextSerializer(type, redactAttribute));
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

        private static int CreateKey(Type type, RedactAttribute redactAttribute)
        {
            unchecked
            {
                var key = type.GetHashCode();

                if (redactAttribute != null)
                {
                    key = (key * 397) ^ redactAttribute.GetHashCode();
                }

                return key;
            }
        }
    }
}