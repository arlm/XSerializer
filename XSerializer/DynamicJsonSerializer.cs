using System;
using System.Collections;
using System.Collections.Concurrent;

namespace XSerializer
{
    internal sealed class DynamicJsonSerializer : IJsonSerializerInternal
    {
        private static readonly Lazy<DynamicJsonSerializer> _clearText = new Lazy<DynamicJsonSerializer>(() => new DynamicJsonSerializer(false));
        private static readonly Lazy<DynamicJsonSerializer> _encrypted = new Lazy<DynamicJsonSerializer>(() => new DynamicJsonSerializer(true));

        private readonly ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal> _serializerCache = new ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal>();
        
        private readonly bool _encrypt;

        private DynamicJsonSerializer(bool encrypt)
        {
            _encrypt = encrypt;
        }

        public static DynamicJsonSerializer Get(bool encrypt)
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
                var toggler = new EncryptWritesToggler(writer);

                if (_encrypt)
                {
                    toggler.Toggle();
                }

                var serializer = _serializerCache.GetOrAdd(Tuple.Create(instance.GetType(), _encrypt), tuple => GetSerializer(tuple.Item1));
                serializer.SerializeObject(writer, instance, info);

                toggler.Revert();
            }
        }

        private IJsonSerializerInternal GetSerializer(Type type)
        {
            if (type == typeof(string))
            {
                return StringJsonSerializer.Get(_encrypt);
            }
            
            if (type == typeof(double))
            {
                return NumberJsonSerializer.Get(_encrypt);
            }
            
            if (type == typeof(bool))
            {
                return BooleanJsonSerializer.Get(_encrypt);
            }
            
            if (type.IsAssignableToGenericIDictionaryOfStringToAnything())
            {
                return DictionaryJsonSerializer.Get(type, _encrypt);
            }
            
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return ListJsonSerializer.Get(type, _encrypt);
            }
            
            throw new NotSupportedException(string.Format("The type, '{0}', is not supported.", type));
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            throw new NotImplementedException();
        }
    }
}