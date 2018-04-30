using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace XSerializer
{
    internal sealed class DynamicJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<bool, JsonMappings, bool>, DynamicJsonSerializer> _cache = new ConcurrentDictionary<Tuple<bool, JsonMappings, bool>, DynamicJsonSerializer>();
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal> _serializerCache = new ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal>();

        private readonly JsonObjectSerializer _jsonObjectSerializer;
        private readonly JsonArraySerializer _jsonArraySerializer;

        private readonly bool _encrypt;
        private readonly JsonMappings _mappings;
        private readonly bool _shouldUseAttributeDefinedInInterface;

        private DynamicJsonSerializer(bool encrypt, JsonMappings mappings, bool shouldUseAttributeDefinedInInterface)
        {
            _jsonObjectSerializer = new JsonObjectSerializer(this);
            _jsonArraySerializer = new JsonArraySerializer(this);

            _encrypt = encrypt;
            _mappings = mappings;
            _shouldUseAttributeDefinedInInterface = shouldUseAttributeDefinedInInterface;
        }

        public static DynamicJsonSerializer Get(bool encrypt, JsonMappings mappings, bool shouldUseAttributeDefinedInInterface = false)
        {
            return _cache.GetOrAdd(Tuple.Create(encrypt, mappings, shouldUseAttributeDefinedInInterface), t => new DynamicJsonSerializer(t.Item1, t.Item2, t.Item3));
        }

        public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            if (instance == null)
            {
                writer.WriteNull();
            }
            else
            {
                var serializer = _serializerCache.GetOrAdd(Tuple.Create(instance.GetType(), _encrypt), tuple => GetSerializer(tuple.Item1));

                if (_encrypt)
                {
                    var toggler = new EncryptWritesToggler(writer);
                    toggler.Toggle();

                    serializer.SerializeObject(writer, instance, info);

                    toggler.Revert();
                }
                else
                {
                    serializer.SerializeObject(writer, instance, info);
                }
            }
        }

        private IJsonSerializerInternal GetSerializer(Type concreteType)
        {
            // Note that no checks are made for nullable types. The concreteType parameter is retrieved
            // from a call to .GetType() on a non-null instance.

            if (concreteType == typeof(string)
                || concreteType == typeof(DateTime)
                || concreteType == typeof(DateTimeOffset)
                || concreteType == typeof(Guid))
            {
                return StringJsonSerializer.Get(concreteType, _encrypt);
            }

            if (concreteType == typeof(double)
                || concreteType == typeof(int)
                || concreteType == typeof(float)
                || concreteType == typeof(long)
                || concreteType == typeof(decimal)
                || concreteType == typeof(byte)
                || concreteType == typeof(sbyte)
                || concreteType == typeof(short)
                || concreteType == typeof(ushort)
                || concreteType == typeof(uint)
                || concreteType == typeof(ulong))
            {
                return NumberJsonSerializer.Get(concreteType, _encrypt);
            }
            
            if (concreteType == typeof(bool))
            {
                return BooleanJsonSerializer.Get(_encrypt, false);
            }

            if (concreteType == typeof(JsonObject))
            {
                return _jsonObjectSerializer;
            }

            if (concreteType == typeof(JsonArray))
            {
                return _jsonArraySerializer;
            }
            
            if (concreteType.IsAssignableToGenericIDictionaryOfStringToAnything())
            {
                return DictionaryJsonSerializer.Get(concreteType, _encrypt, _mappings, _shouldUseAttributeDefinedInInterface);
            }
            
            if (typeof(IEnumerable).IsAssignableFrom(concreteType))
            {
                return ListJsonSerializer.Get(concreteType, _encrypt, _mappings, _shouldUseAttributeDefinedInInterface);
            }

            return CustomJsonSerializer.Get(concreteType, _encrypt, _mappings, _shouldUseAttributeDefinedInInterface);
        }

        public object DeserializeObject(
            JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            if (!reader.ReadContent(path))
            {
                if (reader.NodeType == JsonNodeType.Invalid)
                {
                    throw new MalformedDocumentException(MalformedDocumentError.InvalidValue,
                        path, reader.Value, reader.Line, reader.Position);
                }

                Debug.Assert(reader.NodeType == JsonNodeType.EndOfString);

                throw new MalformedDocumentException(MalformedDocumentError.MissingValue,
                    path, reader.Value, reader.Line, reader.Position);
            }

            if (_encrypt)
            {
                var toggler = new DecryptReadsToggler(reader, path);
                if (toggler.Toggle())
                {
                    if (reader.NodeType == JsonNodeType.Invalid)
                    {
                        throw new MalformedDocumentException(MalformedDocumentError.InvalidValue,
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
            switch (reader.NodeType)
            {
                case JsonNodeType.Null:
                case JsonNodeType.String:
                case JsonNodeType.Boolean:
                    return reader.Value;
                case JsonNodeType.Number:
                    try
                    {
                        return new JsonNumber((string)reader.Value);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new MalformedDocumentException(MalformedDocumentError.InvalidValue,
                            path, reader.Value, reader.Line, reader.Position, ex);
                    }
                case JsonNodeType.OpenObject:
                    return DeserializeJsonObject(reader, info, path);
                case JsonNodeType.OpenArray:
                    return DeserializeJsonArray(reader, info, path);
                case JsonNodeType.EndOfString:
                    throw new MalformedDocumentException(MalformedDocumentError.MissingValue,
                        path, reader.Line, reader.Position);
                default:
                    throw new MalformedDocumentException(MalformedDocumentError.InvalidValue,
                        path, reader.Value, reader.Line, reader.Position);
            }
        }

        private object DeserializeJsonObject(JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            var jsonObject = new JsonObject(info);

            foreach (var propertyName in reader.ReadProperties(path))
            {
                jsonObject.Add(propertyName, DeserializeObject(reader, info, path.AppendProperty(propertyName)));
            }

            return jsonObject;
        }

        private object DeserializeJsonArray(JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            var jsonArray = new JsonArray(info);

            if (reader.PeekContent() == JsonNodeType.CloseArray)
            {
                // If the next content node is CloseArray, we're reading an empty
                // array. Read the CloseArray node and return the empty array.
                reader.Read(path);
                return jsonArray;
            }

            var index = 0;

            while (true)
            {
                jsonArray.Add(DeserializeObject(reader, info, path + "[" + index++ + "]"));

                if (!reader.ReadContent(path))
                {
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
                        path, reader.Line, reader.Position);
                }
            }

            return jsonArray;
        }

        private class JsonObjectSerializer : IJsonSerializerInternal
        {
            private readonly DynamicJsonSerializer _dynamicJsonSerializer;

            public JsonObjectSerializer(DynamicJsonSerializer dynamicJsonSerializer)
            {
                _dynamicJsonSerializer = dynamicJsonSerializer;
            }

            public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
            {
                var jsonObject = (JsonObject)instance;
                
                writer.WriteOpenObject();

                var first = true;

                foreach (var item in jsonObject)
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
                    _dynamicJsonSerializer.SerializeObject(writer, item.Value, info);
                }

                writer.WriteCloseObject();
            }

            public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info, string path)
            {
                throw new NotImplementedException();
            }
        }

        private class JsonArraySerializer : IJsonSerializerInternal
        {
            private readonly DynamicJsonSerializer _dynamicJsonSerializer;

            public JsonArraySerializer(DynamicJsonSerializer dynamicJsonSerializer)
            {
                _dynamicJsonSerializer = dynamicJsonSerializer;
            }

            public void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
            {
                var jsonArray = (JsonArray)instance;

                writer.WriteOpenArray();

                var first = true;

                foreach (var item in jsonArray)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.WriteItemSeparator();
                    }

                    _dynamicJsonSerializer.SerializeObject(writer, item, info);
                }

                writer.WriteCloseArray();
            }

            public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info, string path)
            {
                throw new NotImplementedException();
            }
        }
    }
}