using System;
using System.Diagnostics;

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

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            if (!reader.ReadContent(path))
            {
                if (reader.NodeType == JsonNodeType.EndOfString)
                {
                    throw new MalformedDocumentException(MalformedDocumentError.MissingValue,
                        path, reader.Line, reader.Position);
                }

                Debug.Assert(reader.NodeType == JsonNodeType.Invalid);

                throw new MalformedDocumentException(MalformedDocumentError.BooleanInvalidValue,
                    path, reader.Value, reader.Line, reader.Position);
            }

            if (_encrypt)
            {
                var toggler = new DecryptReadsToggler(reader, path);
                toggler.Toggle();

                if (reader.NodeType == JsonNodeType.EndOfString)
                {
                    throw new MalformedDocumentException(MalformedDocumentError.MissingValue,
                        path, reader.Line, reader.Position);
                }

                var exception = false;

                try
                {
                    return Read(reader, path);
                }
                catch (MalformedDocumentException)
                {
                    exception = true;
                    throw;
                }
                finally
                {
                    if (!exception)
                    {
                        toggler.Revert();
                    }
                }
            }

            return Read(reader, path);
        }

        private object Read(JsonReader reader, string path)
        {
            if (reader.NodeType == JsonNodeType.Boolean)
            {
                return reader.Value;
            }

            if (_nullable && reader.NodeType == JsonNodeType.Null)
            {
                return null;
            }

            if (reader.NodeType == JsonNodeType.String)
            {
                var value = (string)reader.Value;

                if (value == "true")
                {
                    return true;
                }

                if (value == "false")
                {
                    return false;
                }

                if (_nullable && value == "")
                {
                    return null;
                }
            }

            throw new MalformedDocumentException(MalformedDocumentError.BooleanInvalidValue,
                path, reader.Value, reader.Line, reader.Position);
        }
    }
}