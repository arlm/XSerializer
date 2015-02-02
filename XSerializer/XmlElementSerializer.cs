using System;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class XmlElementSerializer<T> : IXmlSerializerInternal
    {
        private readonly string _elementName;
        private readonly bool _alwaysEmitNil;
        private readonly EncryptAttribute _encryptAttribute;
        private readonly IValueConverter _valueConverter;

        public XmlElementSerializer(EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            if (!typeof(T).IsPrimitiveLike() && !typeof(T).IsNullablePrimitiveLike() && !ValueTypes.IsRegistered(typeof(T)))
            {
                throw new InvalidOperationException("Generic argument of XmlElementSerializer<T> must be an primitive, like a primitive (e.g. Guid, DateTime), a nullable of either, or one of: Enum, Type, or Uri.");
            }

            _elementName = options.RootElementName;
            _alwaysEmitNil = options.ShouldAlwaysEmitNil;
            _encryptAttribute = encryptAttribute;

            if (!ValueTypes.TryGetValueConverter(typeof(T), options.RedactAttribute, options.ExtraTypes, out _valueConverter))
            {
                _valueConverter = SimpleTypeValueConverter.Create(typeof(T), options.RedactAttribute);
            }
        }

        public void SerializeObject(XSerializerXmlTextWriter writer, object value, ISerializeOptions options)
        {
            var wasEmptyWriter = writer.IsEmpty;
            writer.WriteStartDocument();

            if (value != null)
            {
                WriteElement(writer, w => w.WriteValue(_valueConverter.GetString(value, options)), options);
            }
            else if (_alwaysEmitNil || options.ShouldEmitNil || wasEmptyWriter)
            {
                WriteElement(writer, w => w.WriteNilAttribute(), options);
            }
        }

        private void WriteElement(XSerializerXmlTextWriter writer, Action<XSerializerXmlTextWriter> writeValueAction, ISerializeOptions options)
        {
            writer.WriteStartElement(_elementName);
            writer.WriteDefaultDocumentNamespaces();

            var setIsEncryptionEnabledBackToFalse = writer.MaybeSetIsEncryptionEnabledToTrue(_encryptAttribute, options);

            writeValueAction(writer);

            if (setIsEncryptionEnabledBackToFalse)
            {
                writer.IsEncryptionEnabled = false;
            }

            writer.WriteEndElement();
        }

        public object DeserializeObject(XSerializerXmlReader reader, ISerializeOptions options)
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

            var setIsDecryptionEnabledBackToFalse = reader.MaybeSetIsDecryptionEnabledToTrue(_encryptAttribute, options);

            var value = reader.ReadString();

            if (setIsDecryptionEnabledBackToFalse)
            {
                reader.IsDecryptionEnabled = false;
            }

            return _valueConverter.ParseString(value, options);
        }
    }
}