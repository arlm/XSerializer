using System;
using System.Collections;
using System.Collections.Concurrent;

namespace XSerializer
{
    internal sealed class ListJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, ListJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool>, ListJsonSerializer>();

        private readonly bool _encrypt;
        private readonly IJsonSerializerInternal _itemSerializer;

        private ListJsonSerializer(Type type, bool encrypt)
        {
            _encrypt = encrypt;

            if (type.IsAssignableToGenericIEnumerable())
            {
                _itemSerializer = JsonSerializerFactory.GetSerializer(type.GetGenericIEnumerableType().GetGenericArguments()[0], _encrypt);
            }
            else
            {
                _itemSerializer = JsonSerializerFactory.GetSerializer(typeof(object), _encrypt);
            }
        }

        public static ListJsonSerializer Get(Type type, bool encrypt)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt), t => new ListJsonSerializer(t.Item1, t.Item2));
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

                writer.WriteOpenArray();

                var enumerable = (IEnumerable)instance;

                var first = true;

                foreach (var item in enumerable)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteItemSeparator();
                    }

                    _itemSerializer.SerializeObject(writer, item, info);
                }

                writer.WriteCloseArray();

                toggler.Revert();
            }
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            throw new NotImplementedException();
        }
    }
}