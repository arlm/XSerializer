using System;

namespace XSerializer
{
    internal sealed class StringJsonSerializer : IJsonSerializerInternal
    {
        private static readonly Lazy<StringJsonSerializer> _clearText = new Lazy<StringJsonSerializer>(() => new StringJsonSerializer(false));
        private static readonly Lazy<StringJsonSerializer> _encrypted = new Lazy<StringJsonSerializer>(() => new StringJsonSerializer(true));
        
        private readonly bool _encrypt;

        private StringJsonSerializer(bool encrypt)
        {
            _encrypt = encrypt;
        }

        public static StringJsonSerializer Get(bool encrypt)
        {
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
                var s = (string)instance;

                var toggler = new EncryptWritesToggler(writer);

                if (_encrypt)
                {
                    toggler.Toggle();
                }

                writer.WriteValue(s);

                toggler.Revert();
            }
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            if (!reader.ReadContent())
            {
                throw new XSerializerException("Reached end of stream while parsing string value.");
            }

            if (_encrypt)
            {
                reader.DecryptCurrentStringValue();
            }

            if (reader.NodeType != JsonNodeType.String)
            {
                throw new XSerializerException(string.Format(
                    "Unexpected node type '{0}' encountered in '{1}.DeserializeObject' method.",
                    reader.NodeType,
                    typeof(StringJsonSerializer)));
            }

            return reader.Value;
        }
    }
}