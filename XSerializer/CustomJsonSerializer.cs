using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSerializer.Encryption;

namespace XSerializer
{
    internal sealed class CustomJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, CustomJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool>, CustomJsonSerializer>();

        private readonly Type _type;
        private readonly bool _encrypt;
        private readonly SerializableJsonProperty[] _serializableProperties;
        private readonly Dictionary<string, SerializableJsonProperty> _serializablePropertiesMap;

        private CustomJsonSerializer(Type type, bool encrypt)
        {
            _type = type;
            _encrypt = encrypt || Attribute.GetCustomAttribute(type, typeof(EncryptAttribute)) != null;

            _serializablePropertiesMap = type.GetProperties()
                .Where(p => p.IsJsonSerializable(type.GetConstructors().SelectMany(c => c.GetParameters())))
                .Select(p => new { Key = p.Name, Value = new SerializableJsonProperty(p, _encrypt || p.GetCustomAttributes(typeof(EncryptAttribute), false).Any()) })
                .ToDictionary(x => x.Key, x => x.Value);

            _serializableProperties = _serializablePropertiesMap.Values.ToArray();
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
        }

        public object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            if (!reader.ReadContent())
            {
                throw new XSerializerException("Unexpected end of input while attempting to parse '{' character.");
            }

            if (reader.NodeType == JsonNodeType.Null)
            {
                return null;
            }

            if (_encrypt)
            {
                var toggler = new DecryptReadsToggler(reader);
                toggler.Toggle();

                try
                {
                    return Read(reader, info);
                }
                finally
                {
                    toggler.Revert();
                }
            }

            return Read(reader, info);
        }

        private object Read(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            IHelper helper = GetHelper();

            foreach (var propertyName in reader.ReadProperties())
            {
                SerializableJsonProperty property;

                if (_serializablePropertiesMap.TryGetValue(propertyName, out property))
                {
                    helper.SetValue(property, reader, info);
                }
                else
                {
                    reader.Discard();
                }
            }

            return helper.GetInstance();
        }

        private static readonly ConcurrentDictionary<Type, Func<IHelper>> _createHelperCache = new ConcurrentDictionary<Type, Func<IHelper>>(); 

        private IHelper GetHelper()
        {
            var createHelper = _createHelperCache.GetOrAdd(_type, t =>
            {
                var constructor = t.GetConstructor(Type.EmptyTypes);

                if (constructor != null)
                {
                    Expression invokeConstructor = Expression.New(constructor);

                    if (t.IsValueType) // Boxing is necessary
                    {
                        invokeConstructor = Expression.Convert(invokeConstructor, typeof(object));
                    }

                    var lambda = Expression.Lambda<Func<object>>(invokeConstructor);
                    var createInstance = lambda.Compile();

                    return (() => new DefaultConstructorHelper(createInstance()));
                }

                throw new NotImplementedException();
            });

            return createHelper();
        }

        private interface IHelper
        {
            void SetValue(SerializableJsonProperty property, JsonReader reader, IJsonSerializeOperationInfo info);
            object GetInstance();
        }

        private class DefaultConstructorHelper : IHelper
        {
            private readonly object _instance;

            public DefaultConstructorHelper(object instance)
            {
                _instance = instance;
            }

            public void SetValue(SerializableJsonProperty property, JsonReader reader, IJsonSerializeOperationInfo info)
            {
                property.SetValue(_instance, reader, info);
            }

            public object GetInstance()
            {
                return _instance;
            }
        }

        private class NonDefaultConstructorHelper : IHelper
        {
            private readonly Dictionary<string, object> _propertyValues = new Dictionary<string, object>();

            public void SetValue(SerializableJsonProperty property, JsonReader reader, IJsonSerializeOperationInfo info)
            {
                var value = property.ReadValue(reader, info);
                _propertyValues[property.Name] = value;
            }

            public object GetInstance()
            {
                throw new NotImplementedException();
            }
        }
    }
}