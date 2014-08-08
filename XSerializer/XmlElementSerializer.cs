using System;
using System.Xml;

namespace XSerializer
{
    internal class XmlElementSerializer<T> : IXmlSerializerInternal<T>
    {
        private readonly string _elementName;
        private readonly IValueConverter _valueConverter;

        public XmlElementSerializer(IXmlSerializerOptions options)
        {
            if (!typeof(T).IsPrimitiveLike() && !typeof(T).IsNullablePrimitiveLike() && typeof(T) != typeof(Enum) && typeof(T) != typeof(Type))
            {
                throw new InvalidOperationException("Generic argument of XmlElementSerializer<T> must be an primitive, like a primitive (e.g. Guid, DateTime), a nullable of either, or the Enum or Type type.");
            }

            _elementName = options.RootElementName;

            if (typeof(T) == typeof(Enum))
            {
                _valueConverter = new EnumTypeValueConverter(options.RedactAttribute, options.ExtraTypes);
            }
            else if (typeof(T) == typeof(Type))
            {
                _valueConverter = new TypeTypeValueConverter(options.RedactAttribute);
            }
            else
            {
                _valueConverter = SimpleTypeValueConverter.Create(typeof(T), options.RedactAttribute);
            }
        }

        public void Serialize(SerializationXmlTextWriter writer, T value, ISerializeOptions options)
        {
            SerializeObject(writer, value, options);
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            writer.WriteStartElement(_elementName);

            if (value != null)
            {
                writer.WriteValue(_valueConverter.GetString(value, options));
            }
            else if (options.ShouldEmitNil)
            {
                writer.WriteNilAttribute();
            }

            writer.WriteEndElement();
        }

        public T Deserialize(XmlReader reader)
        {
            if (typeof(T) == typeof(Enum) || typeof(T) == typeof(Type))
            {
                while (reader.NodeType == XmlNodeType.None)
                {
                    reader.Read();
                }
            }

            var value = reader.ReadString();
            return (T)_valueConverter.ParseString(value);
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }
    }
}