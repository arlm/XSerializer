using System;

namespace XSerializer
{
    internal sealed class BooleanJsonSerializer : IJsonSerializerInternal
    {
        private static readonly Lazy<BooleanJsonSerializer> _clearText = new Lazy<BooleanJsonSerializer>(() => new BooleanJsonSerializer(false));
        private static readonly Lazy<BooleanJsonSerializer> _encrypted = new Lazy<BooleanJsonSerializer>(() => new BooleanJsonSerializer(true));

        private readonly bool _encrypt;

        private BooleanJsonSerializer(bool encrypt)
        {
            _encrypt = encrypt;
        }

        public static BooleanJsonSerializer Get(bool encrypt)
        {
            return encrypt ? _encrypted.Value : _clearText.Value;
        }

        public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            var b = (bool)instance;

            var toggler = new EncryptWritesToggler(writer);

            if (_encrypt)
            {
                toggler.Toggle();
            }

            writer.WriteValue(b);

            toggler.Revert();
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            if (!reader.ReadContent())
            {
                throw new XSerializerException("Reached end of stream while parsing boolean value.");
            }

            var toggler = new DecryptReadsToggler(reader);

            if (_encrypt)
            {
                toggler.Toggle();
            }

            if (reader.NodeType != JsonNodeType.Boolean)
            {
                throw new XSerializerException(string.Format(
                    "Unexpected node type '{0}' encountered in '{1}.DeserializeObject' method.",
                    reader.NodeType,
                    typeof(BooleanJsonSerializer)));
            }

            try
            {
                return reader.Value;
            }
            finally
            {
                toggler.Revert();
            }
        }
    }
}