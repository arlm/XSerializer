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
        private readonly Func<string, object> _read;

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
                var toggler = new EncryptWritesToggler(writer);

                if (_encrypt)
                {
                    toggler.Toggle();
                }

                _write(writer, instance);

                toggler.Revert();
            }
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            if (!reader.ReadContent())
            {
                throw new XSerializerException("Reached end of stream while parsing string value.");
            }

            var toggler = new DecryptReadsToggler(reader);

            if (_encrypt)
            {
                toggler.Toggle();
            }

            if (reader.NodeType != JsonNodeType.String)
            {
                if (!_nullable || reader.NodeType != JsonNodeType.Null)
                {
                    throw new XSerializerException(string.Format(
                        "Unexpected node type '{0}' encountered in '{1}.DeserializeObject' method.",
                        reader.NodeType,
                        typeof(StringJsonSerializer)));
                }
            }

            try
            {
                return _read((string)reader.Value);
            }
            finally
            {
                toggler.Revert();
            }
        }

        private static void SetDelegates(
            Type type,
            out Action<JsonWriter, object> writeAction,
            out Func<string, object> readFunc)
        {
            Func<string, object> readFuncLocal;

            if (type == typeof(string))
            {
                writeAction = (writer, value) => writer.WriteValue((string)value);
                readFuncLocal = value => value;
            }
            else if (type == typeof(DateTime)
                || type == typeof(DateTime?))
            {
                writeAction = (writer, value) => writer.WriteValue((DateTime)value);
                readFuncLocal = value => DateTime.Parse(value);
            }
            else if (type == typeof(DateTimeOffset)
                || type == typeof(DateTimeOffset?))
            {
                writeAction = (writer, value) => writer.WriteValue((DateTimeOffset)value);
                readFuncLocal = value => DateTimeOffset.Parse(value);
            }
            else if (type == typeof(Guid)
                     || type == typeof(Guid?))
            {
                writeAction = (writer, value) => writer.WriteValue((Guid)value);
                readFuncLocal = value => Guid.Parse(value);
            }
            else
            {
                throw new ArgumentException("Unknown string type: " + type, "type");
            }

            readFunc =
                type == typeof(string) || !type.IsNullableType()
                    ? readFuncLocal
                    : value => value == null ? null : readFuncLocal(value);
        }
    }
}