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

        private IJsonSerializerInternal GetSerializer(Type concreteType)
        {
            if (concreteType == typeof(string))
            {
                return StringJsonSerializer.Get(_encrypt);
            }
            
            if (concreteType == typeof(double))
            {
                return NumberJsonSerializer.Get(_encrypt);
            }
            
            if (concreteType == typeof(bool))
            {
                return BooleanJsonSerializer.Get(_encrypt);
            }
            
            if (concreteType.IsAssignableToGenericIDictionaryOfStringToAnything())
            {
                return DictionaryJsonSerializer.Get(concreteType, _encrypt);
            }
            
            if (typeof(IEnumerable).IsAssignableFrom(concreteType))
            {
                return ListJsonSerializer.Get(concreteType, _encrypt);
            }
            
            return CustomJsonSerializer.Get(concreteType, _encrypt);
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            throw new NotImplementedException();
        }
    }
}