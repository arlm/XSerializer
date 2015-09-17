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
                if (_encrypt)
                {
                    var toggler = new EncryptWritesToggler(writer);
                    toggler.Toggle();

                    writer.WriteValue((bool)instance);

                    toggler.Revert();
                }
                else
                {
                    writer.WriteValue((bool)instance);
                }
            }
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            if (!reader.ReadContent())
            {
                throw new XSerializerException("Reached end of stream while parsing boolean value.");
            }

            if (_encrypt)
            {
                var toggler = new DecryptReadsToggler(reader);
                toggler.Toggle();

                try
                {
                    return Read(reader);
                }
                finally
                {
                    toggler.Revert();
                }
            }

            return Read(reader);
        }

        private object Read(JsonReader reader)
        {
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

            return reader.Value;
        }
    }
}