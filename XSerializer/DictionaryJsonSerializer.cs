using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XSerializer
{
    internal sealed class DictionaryJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool, JsonMappings, bool>, DictionaryJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool, JsonMappings, bool>, DictionaryJsonSerializer>();

        private static readonly ConcurrentDictionary<Type, Func<object, object>> _getKeyFuncCache = new ConcurrentDictionary<Type, Func<object, object>>();
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _getValueFuncCache = new ConcurrentDictionary<Type, Func<object, object>>();

        private readonly Action<JsonWriter, object, IJsonSerializeOperationInfo> _write;
        private readonly IJsonSerializerInternal _keySerializer;
        private readonly IJsonSerializerInternal _valueSerializer;

        private readonly Func<object> _createDictionary;
        private readonly Func<string, IJsonSerializeOperationInfo, string, object> _deserializeKey;
        private readonly Action<object, object, object> _addToDictionary;

        private readonly bool _encrypt;
        private readonly JsonMappings _mappings;
        private readonly bool _shouldUseAttributeDefinedInInterface;

        private DictionaryJsonSerializer(Type type, bool encrypt, JsonMappings mappings, bool shouldUseAttributeDefinedInInterface)
        {
            _encrypt = encrypt;
            _mappings = mappings;
            _shouldUseAttributeDefinedInInterface = shouldUseAttributeDefinedInInterface;

            Type keyType;

            if (type.IsAssignableToGenericIDictionary())
            {
                var genericArguments = type.GetGenericArguments();
                keyType = genericArguments[0];

                if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
                {
                    _valueSerializer = JsonSerializerFactory.GetSerializer(genericArguments[1], _encrypt, _mappings, shouldUseAttributeDefinedInInterface);
                    _write = GetIDictionaryOfStringToObjectWriteAction();
                }
                else if (type.IsAssignableToGenericIDictionaryOfStringToAnything())
                {
                    _valueSerializer = JsonSerializerFactory.GetSerializer(genericArguments[1], _encrypt, _mappings, shouldUseAttributeDefinedInInterface);
                    _write = GetIDictionaryOfStringToAnythingWriteAction();
                }
                else
                {
                    _keySerializer = JsonSerializerFactory.GetSerializer(genericArguments[0], _encrypt, _mappings, shouldUseAttributeDefinedInInterface);
                    _valueSerializer = JsonSerializerFactory.GetSerializer(genericArguments[1], _encrypt, _mappings, shouldUseAttributeDefinedInInterface);
                    _write = GetIDictionaryOfAnythingToAnythingWriteAction();
                }
            }
            else
            {
                keyType = typeof(object);

                _keySerializer = JsonSerializerFactory.GetSerializer(typeof(object), _encrypt, _mappings, shouldUseAttributeDefinedInInterface);
                _valueSerializer = _keySerializer;
                _write = GetIDictionaryOfAnythingToAnythingWriteAction();
            }

            if (type.IsInterface)
            {
                if (type.IsGenericIDictionary())
                {
                    type = typeof(Dictionary<,>).MakeGenericType(
                        type.GetGenericArguments()[0], type.GetGenericArguments()[1]);
                }
                else if (type == typeof(IDictionary))
                {
                    type = typeof(Dictionary<object, object>);
                }
                else
                {
                    throw new NotSupportedException(type.FullName);
                }
            }

            _createDictionary = GetCreateDictionaryFunc(type);
            _deserializeKey = GetDeserializeKeyFunc(keyType);
            _addToDictionary = GetAddToDictionaryAction(type);
        }

        public static DictionaryJsonSerializer Get(Type type, bool encrypt, JsonMappings mappings, bool shouldUseAttributeDefinedInInterface)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt, mappings, shouldUseAttributeDefinedInInterface), t => new DictionaryJsonSerializer(t.Item1, t.Item2, t.Item3, t.Item4));
        }

        public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            _write(writer, instance, info);
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

                throw new MalformedDocumentException(MalformedDocumentError.ObjectMissingOpenCurlyBrace,
                    path, reader.Value, reader.Line, reader.Position);
            }

            if (reader.NodeType == JsonNodeType.Null)
            {
                return null;
            }

            if (_encrypt)
            {
                var toggler = new DecryptReadsToggler(reader, path);
                if (toggler.Toggle())
                {
                    if (reader.NodeType == JsonNodeType.EndOfString)
                    {
                        throw new MalformedDocumentException(MalformedDocumentError.MissingValue,
                            path, reader.Line, reader.Position);
                    }

                    var exception = false;

                    try
                    {
                        return Read(reader, info, path);
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
                            if (reader.DecryptReads && (reader.ReadContent(path) || reader.NodeType == JsonNodeType.Invalid))
                            {
                                throw new MalformedDocumentException(MalformedDocumentError.ExpectedEndOfDecryptedString,
                                    path, reader.Value, reader.Line, reader.Position, null, reader.NodeType);
                            }

                            toggler.Revert();
                        }
                    }
                }
            }
            
            return Read(reader, info, path);
        }

        private object Read(JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            if (reader.NodeType == JsonNodeType.Null)
            {
                return null;
            }

            var dictionary = _createDictionary();

            foreach (var keyString in reader.ReadProperties(path))
            {
                var key = _deserializeKey(keyString, info, path);
                var value = _valueSerializer.DeserializeObject(reader, info, path.AppendProperty(keyString));

                var jsonNumber = value as JsonNumber;
                if (jsonNumber != null)
                {
                    value = jsonNumber.DoubleValue;
                }

                _addToDictionary(dictionary, key, value);
            }

            return dictionary;
        }

        private static Func<object> GetCreateDictionaryFunc(Type type)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);

            if (constructor == null)
            {
                throw new ArgumentException("No default constructor for type: " + type + ".", "type");
            }

            Expression invokeConstructor = Expression.New(constructor);

            if (type.IsValueType) // Boxing is necessary
            {
                invokeConstructor = Expression.Convert(invokeConstructor, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object>>(invokeConstructor);
            return lambda.Compile();
        }

        private Func<string, IJsonSerializeOperationInfo, string, object> GetDeserializeKeyFunc(Type type)
        {
            if (type == typeof(string))
            {
                return (keyString, info, path) => keyString;
            }

            var serializer = JsonSerializerFactory.GetSerializer(type, _encrypt, _mappings, _shouldUseAttributeDefinedInInterface);

            return (keyString, info, path) =>
            {
                try
                {
                    using (var stringReader = new StringReader(keyString))
                    {
                        using (var reader = new JsonReader(stringReader, info))
                        {
                            return serializer.DeserializeObject(reader, info, path);
                        }
                    }
                }
                catch (XSerializerException)
                {
                    return keyString;
                }
            };
        }

        private static Action<object, object, object> GetAddToDictionaryAction(Type type)
        {
            var addMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).First(IsAddMethod);
            var keyType = addMethod.GetParameters()[0].ParameterType;
            var valueType = addMethod.GetParameters()[1].ParameterType;

            var dictionaryParameter = Expression.Parameter(typeof(object), "dictionary");
            var keyParameter = Expression.Parameter(typeof(object), "key");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            Expression key = keyParameter;

            if (keyType != typeof(object))
            {
                key = Expression.Convert(keyParameter, keyType);
            }

            Expression value = valueParameter;

            if (valueType != typeof(object))
            {
                value = Expression.Convert(valueParameter, valueType);
            }

            var call = Expression.Call(
                Expression.Convert(dictionaryParameter, type),
                addMethod,
                new[] { key, value });

            var lambda = Expression.Lambda<Action<object, object, object>>(call, dictionaryParameter, keyParameter, valueParameter);
            return lambda.Compile();
        }

        private static bool IsAddMethod(MethodInfo methodInfo)
        {
            if (methodInfo.Name == "Add")
            {
                var parameters = methodInfo.GetParameters();

                if (parameters.Length == 2) // TODO: Better condition (check parameter type, etc.)
                {
                    return true;
                }
            }

            return false;
        }

        private Action<JsonWriter, object, IJsonSerializeOperationInfo> GetIDictionaryOfStringToObjectWriteAction()
        {
            return (writer, instance, info) =>
            {
                if (_encrypt)
                {
                    var toggler = new EncryptWritesToggler(writer);
                    toggler.Toggle();

                    WriteIDictionaryOfStringToObject(writer, instance, info);

                    toggler.Revert();
                }
                else
                {
                    WriteIDictionaryOfStringToObject(writer, instance, info);
                }
            };
        }

        private void WriteIDictionaryOfStringToObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
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
                _valueSerializer.SerializeObject(writer, item.Value, info);
            }

            writer.WriteCloseObject();
        }

        private Action<JsonWriter, object, IJsonSerializeOperationInfo> GetIDictionaryOfStringToAnythingWriteAction()
        {
            return (writer, instance, info) =>
            {
                if (_encrypt)
                {
                    var toggler = new EncryptWritesToggler(writer);
                    toggler.Toggle();

                    WriteIDictionaryOfStringToAnything(writer, instance, info);

                    toggler.Revert();
                }
                else
                {
                    WriteIDictionaryOfStringToAnything(writer, instance, info);                    
                }
            };
        }

        private void WriteIDictionaryOfStringToAnything(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            writer.WriteOpenObject();

            var dictionary = (IEnumerable)instance;

            var first = true;

            Func<object, object> getKeyFunc = null;
            Func<object, object> getValueFunc = null;

            foreach (var item in dictionary)
            {
                if (first)
                {
                    first = false;
                    var itemType = item.GetType();
                    getKeyFunc = _getKeyFuncCache.GetOrAdd(itemType, GetGetKeyFunc);
                    getValueFunc = _getValueFuncCache.GetOrAdd(itemType, GetGetValueFunc);
                }
                else
                {
                    writer.WriteItemSeparator();
                }

                writer.WriteValue((string)getKeyFunc(item));
                writer.WriteNameValueSeparator();
                _valueSerializer.SerializeObject(writer, getValueFunc(item), info);
            }

            writer.WriteCloseObject();
        }

        private Action<JsonWriter, object, IJsonSerializeOperationInfo> GetIDictionaryOfAnythingToAnythingWriteAction()
        {
            return (writer, instance, info) =>
            {
                if (_encrypt)
                {
                    var toggler = new EncryptWritesToggler(writer);
                    toggler.Toggle();

                    WriteIDictionaryOfAnythingToAnything(writer, instance, info);

                    toggler.Revert();
                }
                else
                {
                    WriteIDictionaryOfAnythingToAnything(writer, instance, info);                    
                }
            };
        }

        private void WriteIDictionaryOfAnythingToAnything(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            writer.WriteOpenObject();

            var dictionary = (IEnumerable)instance;

            var first = true;

            Func<object, object> getKeyFunc = null;
            Func<object, object> getValueFunc = null;

            foreach (var item in dictionary)
            {
                if (first)
                {
                    first = false;
                    var itemType = item.GetType();
                    getKeyFunc = _getKeyFuncCache.GetOrAdd(itemType, GetGetKeyFunc);
                    getValueFunc = _getValueFuncCache.GetOrAdd(itemType, GetGetValueFunc);
                }
                else
                {
                    writer.WriteItemSeparator();
                }

                var key = getKeyFunc(item);

                var keyString = key as string;
                if (keyString != null)
                {
                    writer.WriteValue(keyString);
                }
                else
                {
                    var sb = new StringBuilder();

                    using (var stringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8))
                    {
                        using (var keyWriter = new JsonWriter(stringWriter, info))
                        {
                            _keySerializer.SerializeObject(keyWriter, key, info);
                        }
                    }

                    writer.WriteValue((sb.ToString()));
                }

                writer.WriteNameValueSeparator();
                _valueSerializer.SerializeObject(writer, getValueFunc(item), info);
            }

            writer.WriteCloseObject();
        }

        private static Func<object, object> GetGetKeyFunc(Type keyValuePairType)
        {
            var parameter = Expression.Parameter(typeof(object), "keyValuePair");

            var propertyInfo = keyValuePairType.GetProperty("Key");

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