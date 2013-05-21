using System;
using System.Collections.Concurrent;
using System.Xml;

namespace XSerializer
{
    public class XmlTextSerializer : IXmlSerializer
    {
        private static readonly ConcurrentDictionary<int, XmlTextSerializer> Map = new ConcurrentDictionary<int, XmlTextSerializer>();
        private static readonly object MapLocker = new object();

        private readonly SimpleTypeValueConverter _valueConverter;

        private XmlTextSerializer(Type type, RedactAttribute redactAttribute)
        {
            _valueConverter = SimpleTypeValueConverter.Create(type, redactAttribute);
        }

        public static XmlTextSerializer GetSerializer(Type type, RedactAttribute redactAttribute)
        {
            XmlTextSerializer serializer;

            var key = CreateKey(type, redactAttribute);

            if (!Map.TryGetValue(key, out serializer))
            {
                lock (MapLocker)
                {
                    if (!Map.TryGetValue(key, out serializer))
                    {
                        serializer = new XmlTextSerializer(type, redactAttribute);
                        Map[key] = serializer;
                    }
                }
            }

            return serializer;
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