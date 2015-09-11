using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XSerializer
{
    internal sealed class DynamicJsonSerializer : IJsonSerializerInternal
    {
        public static readonly DynamicJsonSerializer Instance = new DynamicJsonSerializer();
        
        private readonly ConcurrentDictionary<Type, Func<object, string>> _getKeyFuncCache = new ConcurrentDictionary<Type, Func<object, string>>();
        private readonly ConcurrentDictionary<Type, Func<object, object>> _getValueFuncCache = new ConcurrentDictionary<Type, Func<object, object>>();

        private readonly ConcurrentDictionary<Type, Action<JsonWriter, object>> _writeInstanceActionCache = new ConcurrentDictionary<Type, Action<JsonWriter, object>>();

        private DynamicJsonSerializer()
        {
        }

        public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            if (instance == null)
            {
                writer.WriteNull();
            }
            else
            {
                var writeInstance =
                    _writeInstanceActionCache.GetOrAdd(
                        instance.GetType(),
                        type => GetWriteInstanceAction(type, info));

                writeInstance(writer, instance);
            }
        }

        private Action<JsonWriter, object> GetWriteInstanceAction(Type type, IJsonSerializeOperationInfo info)
        {
            if (type == typeof(string))
            {
                return (writer, instance) => writer.WriteValue((string)instance);
            }
            
            if (type == typeof(double))
            {
                return (writer, instance) => writer.WriteValue((double)instance);
            }
            
            if (type == typeof(bool))
            {
                return (writer, instance) => writer.WriteValue((bool)instance);
            }
            
            if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
            {
                return (writer, instance) =>
                {
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
                        SerializeObject(writer, item.Value, info);
                    }

                    writer.WriteCloseObject();
                };
            }
            
            if (IsAssignableToGenericIDictionaryOfStringToAnything(type))
            {
                return (writer, instance) =>
                {
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
                        SerializeObject(writer, getValueFunc(item), info);
                    }

                    writer.WriteCloseObject();
                };
            }
            
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return (writer, instance) =>
                {
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

                        SerializeObject(writer, item, info);
                    }

                    writer.WriteCloseArray();
                };
            }
            
            throw new NotSupportedException(string.Format("The type, '{0}', is not supported.", type));
        }

        private static bool IsAssignableToGenericIDictionaryOfStringToAnything(Type type)
        {
            if (type.IsAssignableToGenericIDictionary())
            {
                var dictionaryType = type.GetGenericIDictionaryType();
                var args = dictionaryType.GetGenericArguments();
                return args[0] == typeof(string);
            }

            return false;
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

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            throw new NotImplementedException();
        }
    }
}