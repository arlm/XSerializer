using System;

namespace XSerializer
{
    internal sealed class BooleanJsonSerializer : IJsonSerializerInternal
    {
        private static readonly Lazy<BooleanJsonSerializer> _clearText = new Lazy<BooleanJsonSerializer>(() => new BooleanJsonSerializer(false, false));
        private static readonly Lazy<BooleanJsonSerializer> _encrypted = new Lazy<BooleanJsonSerializer>(() => new BooleanJsonSerializer(true, false));
        private static readonly Lazy<BooleanJsonSerializer> _clearTextNullable = new Lazy<BooleanJsonSerializer>(() => new BooleanJsonSerializer(false, true));
        private static readonly Lazy<BooleanJsonSerializer> _encryptedNullable = new Lazy<BooleanJsonSerializer>(() => new BooleanJsonSerializer(true, true));

        private readonly bool _encrypt;
        private readonly bool _nullable;

        private BooleanJsonSerializer(bool encrypt, bool nullable)
        {
            _encrypt = encrypt;
            _nullable = nullable;
        }

        public static BooleanJsonSerializer Get(bool encrypt, bool nullable)
        {
            if (nullable)
            {
                return encrypt ? _encryptedNullable.Value : _clearTextNullable.Value;
            }

            return encrypt ? _encrypted.Value : _clearText.Value;
        }

        public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            if (instance == null)
            {
                writer.WriteNull();
            }
            else
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
                if (!_nullable || reader.NodeType != JsonNodeType.Null)
                {
                    throw new XSerializerException(string.Format(
                        "Unexpected node type '{0}' encountered in '{1}.DeserializeObject' method.",
                        reader.NodeType,
                        typeof(BooleanJsonSerializer)));
                }
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