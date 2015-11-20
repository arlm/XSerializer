using System;
using System.Collections.Concurrent;

namespace XSerializer
{
    internal sealed class NumberJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, NumberJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool>, NumberJsonSerializer>();

        private readonly bool _encrypt;
        private readonly bool _nullable;
        private readonly Action<JsonWriter, object> _write;
        private readonly Func<string, object> _read;

        private NumberJsonSerializer(Type type, bool encrypt)
        {
            _encrypt = encrypt;
            _nullable = type.IsNullableType();
            SetDelegates(type, out _write, out _read);
        }

        public static NumberJsonSerializer Get(Type type, bool encrypt)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt), t => new NumberJsonSerializer(t.Item1, t.Item2));
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
                throw new XSerializerException("Reached end of stream while parsing number value.");
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
            if (reader.NodeType != JsonNodeType.Number
                && reader.NodeType != JsonNodeType.String)
            {
                if (!_nullable && reader.NodeType != JsonNodeType.Null)
                {
                    throw new XSerializerException(string.Format(
                        "Unexpected node type '{0}' encountered in '{1}.DeserializeObject' method.",
                        reader.NodeType,
                        typeof(NumberJsonSerializer)));
                }
            }

            return _read((string)reader.Value);
        }

        private static void SetDelegates(
            Type type,
            out Action<JsonWriter, object> writeAction,
            out Func<string, object> readFunc)
        {
            Func<string, object> readFuncLocal;

            if (type == typeof(double) || type == typeof(double?))
            {
                writeAction = (writer, value) => writer.WriteValue((double)value);
                readFuncLocal = value => double.Parse(value);
            }
            else if(type == typeof(int) || type == typeof(int?))
            {
                writeAction = (writer, value) => writer.WriteValue((int)value);
                readFuncLocal = value => int.Parse(value);
            }
            else if(type == typeof(long) || type == typeof(long?))
            {
                writeAction = (writer, value) => writer.WriteValue((long)value);
                readFuncLocal = value => long.Parse(value);
            }
            else if(type == typeof(uint) || type == typeof(uint?))
            {
                writeAction = (writer, value) => writer.WriteValue((uint)value);
                readFuncLocal = value => uint.Parse(value);
            }
            else if(type == typeof(byte) || type == typeof(byte?))
            {
                writeAction = (writer, value) => writer.WriteValue((byte)value);
                readFuncLocal = value => byte.Parse(value);
            }
            else if(type == typeof(sbyte) || type == typeof(sbyte?))
            {
                writeAction = (writer, value) => writer.WriteValue((sbyte)value);
                readFuncLocal = value => sbyte.Parse(value);
            }
            else if(type == typeof(short) || type == typeof(short?))
            {
                writeAction = (writer, value) => writer.WriteValue((short)value);
                readFuncLocal = value => short.Parse(value);
            }
            else if(type == typeof(ushort) || type == typeof(ushort?))
            {
                writeAction = (writer, value) => writer.WriteValue((ushort)value);
                readFuncLocal = value => ushort.Parse(value);
            }
            else if(type == typeof(ulong) || type == typeof(ulong?))
            {
                writeAction = (writer, value) => writer.WriteValue((ulong)value);
                readFuncLocal = value => ulong.Parse(value);
            }
            else if(type == typeof(float) || type == typeof(float?))
            {
                writeAction = (writer, value) => writer.WriteValue((float)value);
                readFuncLocal = value => float.Parse(value);
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                writeAction = (writer, value) => writer.WriteValue((decimal)value);
                readFuncLocal = value => decimal.Parse(value);
            }
            else
            {
                throw new ArgumentException("Unknown number type: " + type, "type");
            }

            readFunc =
                !type.IsNullableType()
                    ? readFuncLocal
                    : value => value == null ? null : readFuncLocal(value);
        }
    }
}