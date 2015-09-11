using System;
using System.Collections.Concurrent;
using System.Linq;

namespace XSerializer
{
    internal sealed class CustomJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, CustomJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool>, CustomJsonSerializer>();

        private readonly bool _encrypt;
        private readonly SerializableJsonProperty[] _serializableProperties;

        private CustomJsonSerializer(Type type, bool encrypt)
        {
            _encrypt = encrypt;

            _serializableProperties = type.GetProperties()
                .Where(p => p.IsSerializable(type.GetConstructors().SelectMany(c => c.GetParameters())))
                .Select(p => new SerializableJsonProperty(p, _encrypt))
                .ToArray();
        }

        public static CustomJsonSerializer Get(Type type, bool encrypt)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt), t => new CustomJsonSerializer(t.Item1, t.Item2));
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

                writer.WriteOpenObject();

                var first = true;

                foreach (var property in _serializableProperties)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteItemSeparator();
                    }

                    property.WriteValue(writer, instance, info);
                }

                writer.WriteCloseObject();

                toggler.Revert();
            }
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            throw new NotImplementedException();
        }
    }
}