using System;
using System.Xml;

namespace XSerializer
{
    internal class XmlElementSerializer<T> : IXmlSerializerInternal<T>
    {
        private readonly string _elementName;
        private readonly SimpleTypeValueConverter _valueConverter;

        public XmlElementSerializer(IXmlSerializerOptions options)
        {
            if (!typeof(T).IsPrimitiveLike() && !typeof(T).IsNullablePrimitiveLike())
            {
                throw new InvalidOperationException("Generic argument of XmlElementSerializer<T> must be an primitive, like a primitive (e.g. Guid, DateTime), or a nullable of either.");
            }

            _elementName = options.RootElementName;
            _valueConverter = SimpleTypeValueConverter.Create(typeof(T), options.RedactAttribute);
        }

        public void Serialize(SerializationXmlTextWriter writer, T value, ISerializeOptions options)
        {
            SerializeObject(writer, value, options);
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                writer.WriteElementString(_elementName, _valueConverter.GetString(value, options));
            }
        }

        public T Deserialize(XmlReader reader)
        {
            return (T)_valueConverter.ParseString(reader.ReadString());
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }
    }
}