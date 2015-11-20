using System;
using System.Collections.Concurrent;

namespace XSerializer
{
    internal sealed class StringJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, StringJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool>, StringJsonSerializer>();

        private readonly bool _encrypt;
        private readonly bool _nullable;
        private readonly Action<JsonWriter, object> _write;
        private readonly Func<string, IJsonSerializeOperationInfo, object> _read;

        private StringJsonSerializer(Type type, bool encrypt)
        {
            _encrypt = encrypt;
            _nullable = type.IsNullableType();
            SetDelegates(type, out _write, out _read);
        }

        public static StringJsonSerializer Get(Type type, bool encrypt)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt), t => new StringJsonSerializer(t.Item1, t.Item2));
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

                    _write(writer, instance);

                    toggler.Revert();
                }
                else
                {
                    _write(writer, instance);
                }
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
                var toggler = new DecryptReadsToggler(reader);
                toggler.Toggle();

                try
                {
                    return Read(reader, info);
                }
                finally
                {
                    toggler.Revert();
                }
            }

            return Read(reader, info);
        }

        private object Read(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            string value;

            switch (reader.NodeType)
            {
                case JsonNodeType.Number:
                case JsonNodeType.String:
                    value = (string)reader.Value;
                    break;
                case JsonNodeType.Boolean:
                    value = (bool)reader.Value ? "true" : "false";
                    break;
                default:
                    if (_nullable && reader.NodeType == JsonNodeType.Null)
                    {
                        value = null;
                    }
                    else
                    {
                        throw new XSerializerException(string.Format(
                            "Unexpected node type '{0}' encountered in '{1}.DeserializeObject' method.",
                            reader.NodeType,
                            typeof(StringJsonSerializer)));
                    }
                    break;
            }

            return _read(value, info);
        }

        private void SetDelegates(
            Type type,
            out Action<JsonWriter, object> writeAction,
            out Func<string, IJsonSerializeOperationInfo, object> readFunc)
        {
            Func<string, IJsonSerializeOperationInfo, object> readFuncLocal;

            if (type == typeof(string))
            {
                writeAction = (writer, value) => writer.WriteValue((string)value);
                readFuncLocal = (value, info) => value;
            }
            else if (type == typeof(DateTime)
                || type == typeof(DateTime?))
            {
                writeAction = (writer, value) => writer.WriteValue((DateTime)value);
                readFuncLocal = (value, info) => info.DateTimeHandler.ParseDateTime(value);
            }
            else if (type == typeof(DateTimeOffset)
                || type == typeof(DateTimeOffset?))
            {
                writeAction = (writer, value) => writer.WriteValue((DateTimeOffset)value);
                readFuncLocal = (value, info) => info.DateTimeHandler.ParseDateTimeOffset(value);
            }
            else if (type == typeof(Guid)
                     || type == typeof(Guid?))
            {
                writeAction = (writer, value) => writer.WriteValue((Guid)value);
                readFuncLocal = (value, info) => Guid.Parse(value);
            }
            else
            {
                throw new ArgumentException("Unknown string type: " + type, "type");
            }

            readFunc =
                type == typeof(string) || !type.IsNullableType()
                    ? readFuncLocal
                    : (value, info) => value == null ? null : readFuncLocal(value, info);
        }
    }
}