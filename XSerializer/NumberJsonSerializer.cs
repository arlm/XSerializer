using System;

namespace XSerializer
{
    internal sealed class NumberJsonSerializer : IJsonSerializerInternal
    {
        private static readonly Lazy<NumberJsonSerializer> _clearText = new Lazy<NumberJsonSerializer>(() => new NumberJsonSerializer(false));
        private static readonly Lazy<NumberJsonSerializer> _encrypted = new Lazy<NumberJsonSerializer>(() => new NumberJsonSerializer(true));
        
        private readonly bool _encrypt;

        private NumberJsonSerializer(bool encrypt)
        {
            _encrypt = encrypt;
        }

        public static NumberJsonSerializer Get(bool encrypt)
        {
            return encrypt ? _encrypted.Value : _clearText.Value;
        }

        public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            var d = (double)instance;

            var toggler = new EncryptWritesToggler(writer);

            if (_encrypt)
            {
                toggler.Toggle();
            }

            writer.WriteValue(d);

            toggler.Revert();
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            if (!reader.ReadContent())
            {
                throw new XSerializerException("Reached end of stream while parsing number value.");
            }

            var toggler = new DecryptReadsToggler(reader);

            if (_encrypt)
            {
                toggler.Toggle();
            }

            if (reader.NodeType != JsonNodeType.Number)
            {
                throw new XSerializerException(string.Format(
                    "Unexpected node type '{0}' encountered in '{1}.DeserializeObject' method.",
                    reader.NodeType,
                    typeof(NumberJsonSerializer)));
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