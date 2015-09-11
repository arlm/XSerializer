using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XSerializer
{
    internal sealed class DictionaryJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, DictionaryJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool>, DictionaryJsonSerializer>();

        private static readonly ConcurrentDictionary<Type, Func<object, string>> _getKeyFuncCache = new ConcurrentDictionary<Type, Func<object, string>>();
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _getValueFuncCache = new ConcurrentDictionary<Type, Func<object, object>>();

        private readonly Action<JsonWriter, object, IJsonSerializeOperationInfo> _write;
        private readonly IJsonSerializerInternal _valueSerializer;
        
        private readonly bool _encrypt;

        private DictionaryJsonSerializer(Type type, bool encrypt)
        {
            _encrypt = encrypt;

            if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
            {
                _valueSerializer = JsonSerializerFactory.GetSerializer(type.GetGenericArguments()[1], _encrypt);
                _write = GetIDictionaryOfStringToObjectWriteAction();
            }
            else if (type.IsAssignableToGenericIDictionaryOfStringToAnything())
            {
                _valueSerializer = JsonSerializerFactory.GetSerializer(type.GetGenericArguments()[1], _encrypt);
                _write = GetIDictionaryOfStringToAnythingWriteAction();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static DictionaryJsonSerializer Get(Type type, bool encrypt)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt), t => new DictionaryJsonSerializer(t.Item1, t.Item2));
        }

        public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            _write(writer, instance, info);
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            throw new NotImplementedException();
        }

        private Action<JsonWriter, object, IJsonSerializeOperationInfo> GetIDictionaryOfStringToObjectWriteAction()
        {
            return (writer, instance, info) =>
            {
                var toggler = new EncryptWritesToggler(writer);

                if (_encrypt)
                {
                    toggler.Toggle();
                }

                writer.WriteOpenObject();

                var dictionary = (IDictionary<string, object>)instance;

                var first = true;

                foreach (var item in dictionary)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteItemSeparator();
                    }

                    writer.WriteValue(item.Key);
                    writer.WriteNameValueSeparator();
                    _valueSerializer.SerializeObject(writer, item.Value, info);
                }

                writer.WriteCloseObject();

                toggler.Revert();
            };
        }

        private Action<JsonWriter, object, IJsonSerializeOperationInfo> GetIDictionaryOfStringToAnythingWriteAction()
        {
            return (writer, instance, info) =>
            {
                var toggler = new EncryptWritesToggler(writer);

                if (_encrypt)
                {
                    toggler.Toggle();
                }

                writer.WriteOpenObject();

                var dictionary = (IEnumerable)instance;

                var first = true;

                Func<object, string> getKeyFunc = null;
                Func<object, object> getValueFunc = null;

                foreach (var item in dictionary)
                {
                    var itemType = item.GetType();

                    if (first)
                    {
                        first = false;
                        getKeyFunc = _getKeyFuncCache.GetOrAdd(itemType, GetGetKeyFunc);
                        getValueFunc = _getValueFuncCache.GetOrAdd(itemType, GetGetValueFunc);
                    }
                    else
                    {
                        writer.WriteItemSeparator();
                    }

                    writer.WriteValue(getKeyFunc(item));
                    writer.WriteNameValueSeparator();
                    _valueSerializer.SerializeObject(writer, getValueFunc(item), info);
                }

                writer.WriteCloseObject();

                toggler.Revert();
            };
        }

        private static Func<object, string> GetGetKeyFunc(Type keyValuePairType)
        {
            var parameter = Expression.Parameter(typeof(object), "keyValuePair");

            var propertyInfo = keyValuePairType.GetProperty("Key");

            Expression body =
                    Expression.Property(
                        Expression.Convert(parameter, keyValuePairType),
                        propertyInfo);

            var lambda = Expression.Lambda<Func<object, string>>(body, new[] { parameter });
            return lambda.Compile();
        }

        private static Func<object, object> GetGetValueFunc(Type keyValuePairType)
        {
            var parameter = Expression.Parameter(typeof(object), "keyValuePair");

            var propertyInfo = keyValuePairType.GetProperty("Value");

            Expression body =
                Expression.Property(
                    Expression.Convert(parameter, keyValuePairType),
                    propertyInfo);

            if (propertyInfo.PropertyType.IsValueType)
            {
                body = Expression.Convert(body, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object, object>>(body, new[] { parameter });
            return lambda.Compile();
        }
    }
}