using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace XSerializer
{
    internal sealed class ListJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool, JsonMappings>, ListJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool, JsonMappings>, ListJsonSerializer>();

        private readonly bool _encrypt;
        private readonly IJsonSerializerInternal _itemSerializer;

        private readonly Func<object> _createList;
        private readonly Action<object, object> _addItem;
        private readonly Func<object, object> _transformList;

        private ListJsonSerializer(Type type, bool encrypt, JsonMappings mappings)
        {
            _encrypt = encrypt;

            if (type.IsAssignableToGenericIEnumerable())
            {
                var itemType = type.GetGenericIEnumerableType().GetGenericArguments()[0];
                _itemSerializer = JsonSerializerFactory.GetSerializer(itemType, _encrypt, mappings);
            }
            else
            {
                _itemSerializer = JsonSerializerFactory.GetSerializer(typeof(object), _encrypt, mappings);
            }

            if (type.IsInterface)
            {
                if (type.IsGenericIEnumerable())
                {
                    var itemType = type.GetGenericArguments()[0];
                    type = typeof(List<>).MakeGenericType(itemType);
                }
                else
                {
                    type = typeof(List<object>);
                }
            }

            var listType = type;

            if (type.IsArray)
            {
                if (type.GetArrayRank() > 1)
                {
                    throw new NotSupportedException("Only arrays with a rank of one are supported: " + type + ".");
                }

                var itemType = type.GetElementType();
                listType = typeof(List<>).MakeGenericType(itemType);
            }

            _createList = GetCreateListFunc(listType);
            _addItem = GetAddItemAction(listType);
            _transformList = GetTransformListFunc(type, listType);
        }

        public static ListJsonSerializer Get(Type type, bool encrypt, JsonMappings mappings)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt, mappings), t => new ListJsonSerializer(t.Item1, t.Item2, t.Item3));
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

                    Write(writer, instance, info);

                    toggler.Revert();
                }
                else
                {
                    Write(writer, instance, info);
                }
            }
        }

        private void Write(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
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

                _itemSerializer.SerializeObject(writer, item, info);
            }

            writer.WriteCloseArray();
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

                throw new MalformedDocumentException(MalformedDocumentError.ArrayMissingOpenSquareBracket,
                        path, reader.Value, reader.Line, reader.Position);
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

                    if (reader.NodeType != JsonNodeType.OpenArray
                        && reader.NodeType != JsonNodeType.Null)
                    {
                        throw new MalformedDocumentException(MalformedDocumentError.ArrayMissingOpenSquareBracket,
                            path, reader.Value, reader.Line, reader.Position);
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
                            if (reader.ReadContent(path) || reader.NodeType == JsonNodeType.Invalid)
                            {
                                throw new MalformedDocumentException(MalformedDocumentError.ExpectedEndOfDecryptedString,
                                    path, reader.Value, reader.Line, reader.Position, null, reader.NodeType);
                            }

                            toggler.Revert();
                        }
                    }
                }
            }
            
            if (reader.NodeType != JsonNodeType.OpenArray
                && reader.NodeType != JsonNodeType.Null)
            {
                throw new MalformedDocumentException(MalformedDocumentError.ArrayMissingOpenSquareBracket,
                    path, reader.Value, reader.Line, reader.Position);
            }

            return Read(reader, info, path);
        }

        private object Read(JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            if (reader.NodeType == JsonNodeType.Null)
            {
                return null;
            }

            var list = _createList();

            if (reader.PeekContent() == JsonNodeType.CloseArray)
            {
                // If the next content node is CloseArray, we're reading an empty
                // array. Read the CloseArray node and return the empty list.
                reader.Read(path);
                return _transformList(list);
            }

            var index = 0;

            while (true)
            {
                var item = _itemSerializer.DeserializeObject(reader, info, path + "[" + index++ + "]");
                _addItem(list, item);

                if (!reader.ReadContent(path))
                {
                    Debug.Assert(reader.NodeType == JsonNodeType.EndOfString);

                    throw new MalformedDocumentException(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket,
                        path, reader.Line, reader.Position);
                }

                if (reader.NodeType == JsonNodeType.CloseArray)
                {
                    break;
                }

                if (reader.NodeType != JsonNodeType.ItemSeparator)
                {
                    throw new MalformedDocumentException(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket,
                        path, reader.Value, reader.Line, reader.Position);
                }
            }

            return _transformList(list);
        }

        private static Func<object> GetCreateListFunc(Type type)
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

        private static Action<object, object> GetAddItemAction(Type type)
        {
            var addMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).First(IsAddMethod);
            var itemType = addMethod.GetParameters()[0].ParameterType;

            var listParameter = Expression.Parameter(typeof(object), "list");
            var itemParameter = Expression.Parameter(typeof(object), "item");

            Expression item = itemParameter;

            if (itemType != typeof(object))
            {
                item = Expression.Convert(itemParameter, itemType);
            }

            var call = Expression.Call(
                Expression.Convert(listParameter, type),
                addMethod,
                new[] { item });

            var lambda = Expression.Lambda<Action<object, object>>(call, listParameter, itemParameter);
            return lambda.Compile();
        }

        private static bool IsAddMethod(MethodInfo methodInfo)
        {
            if (methodInfo.Name == "Add")
            {
                var parameters = methodInfo.GetParameters();

                if (parameters.Length == 1) // TODO: Better condition (check parameter type, etc.)
                {
                    return true;
                }
            }

            return false;
        }

        private static Func<object, object> GetTransformListFunc(Type type, Type listType)
        {
            if (type == listType)
            {
                return list => list;
            }

            var itemType = listType.GetGenericArguments()[0];
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(itemType);

            var listParameter = Expression.Parameter(typeof(object), "list");

            var lambda = Expression.Lambda<Func<object, object>>(
                Expression.Call(toArrayMethod, Expression.Convert(listParameter, enumerableType)),
                listParameter);

            return lambda.Compile();
        }
    }
}