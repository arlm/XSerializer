using System;
using System.Xml;

namespace XSerializer
{
    internal class XmlElementSerializer<T> : IXmlSerializerInternal
    {
        private readonly string _elementName;
        private readonly bool _alwaysEmitNil;
        private readonly IValueConverter _valueConverter;

        public XmlElementSerializer(IXmlSerializerOptions options)
        {
            if (!typeof(T).IsPrimitiveLike() && !typeof(T).IsNullablePrimitiveLike() && !ValueTypes.IsRegistered(typeof(T)))
            {
                throw new InvalidOperationException("Generic argument of XmlElementSerializer<T> must be an primitive, like a primitive (e.g. Guid, DateTime), a nullable of either, or one of: Enum, Type, or Uri.");
            }

            _elementName = options.RootElementName;
            _alwaysEmitNil = options.ShouldAlwaysEmitNil;

            if (!ValueTypes.TryGetValueConverter(typeof(T), options.RedactAttribute, options.ExtraTypes, out _valueConverter))
            {
                _valueConverter = SimpleTypeValueConverter.Create(typeof(T), options.RedactAttribute);
            }
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            var wasEmptyWriter = writer.IsEmpty;
            writer.WriteStartDocument();

            if (value != null)
            {
                WriteElement(writer, w => w.WriteValue(_valueConverter.GetString(value, options)));
            }
            else if (_alwaysEmitNil || options.ShouldEmitNil || wasEmptyWriter)
            {
                WriteElement(writer, w => w.WriteNilAttribute());
            }
        }

        private void WriteElement(SerializationXmlTextWriter writer, Action<SerializationXmlTextWriter> writeValueAction)
        {
            writer.WriteStartElement(_elementName);
            writer.WriteDefaultDocumentNamespaces();
            writeValueAction(writer);
            writer.WriteEndElement();
        }

        public object DeserializeObject(XmlReader reader)
        {
            if (ValueTypes.IsRegistered(typeof(T)))
            {
                while (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                }
            }

            if (reader.IsNil())
            {
                return default(T);
            }

            var value = reader.ReadString();
            return _valueConverter.ParseString(value);
        }
    }
}