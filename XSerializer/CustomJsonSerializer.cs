using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XSerializer.Encryption;

namespace XSerializer
{
    internal sealed class CustomJsonSerializer : IJsonSerializerInternal
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool, JsonMappings>, CustomJsonSerializer> _cache = new ConcurrentDictionary<Tuple<Type, bool, JsonMappings>, CustomJsonSerializer>();

        private readonly ConcurrentDictionary<Type, List<SerializableJsonProperty>> _serializingPropertiesMap = new ConcurrentDictionary<Type, List<SerializableJsonProperty>>();
        private readonly Dictionary<string, SerializableJsonProperty> _deserializingPropertiesMap;

        private readonly Type _type;
        private readonly JsonMappings _mappings;
        private readonly bool _encrypt;
        private readonly Lazy<Func<IObjectFactory>> _createObjectFactory;

        private CustomJsonSerializer(Type type, bool encrypt, JsonMappings mappings)
        {
            _mappings = mappings;
            _type = type;

            if (_mappings.MappingsByType.ContainsKey(type))
            {
                _type = _mappings.MappingsByType[type];
            }
            else
            {
                var mappingAttribute = (JsonMappingAttribute)Attribute.GetCustomAttribute(_type, typeof(JsonMappingAttribute));
                if (mappingAttribute != null)
                {
                    _type = mappingAttribute.Type;
                }
            }

            _encrypt = encrypt || Attribute.GetCustomAttribute(type, typeof(EncryptAttribute)) != null;

            var serializableProperties = GetSerializableProperties(_type);
            _deserializingPropertiesMap = serializableProperties.ToDictionary(p => p.Name);

            if (!_type.IsAbstract)
            {
                _serializingPropertiesMap[_type] = serializableProperties;
            }

            _createObjectFactory = new Lazy<Func<IObjectFactory>>(GetCreateObjectFactoryFunc);
        }

        private List<SerializableJsonProperty> GetSerializableProperties(Type type)
        {
            return type.GetProperties()
                .Where(p => p.IsJsonSerializable(type.GetConstructors().SelectMany(c => c.GetParameters())))
                .Select(p => new SerializableJsonProperty(p, _encrypt || p.GetCustomAttributes(typeof(EncryptAttribute), false).Any(), _mappings)).ToList();
        }

        public static CustomJsonSerializer Get(Type type, bool encrypt, JsonMappings mappings)
        {
            return _cache.GetOrAdd(Tuple.Create(type, encrypt, mappings), t => new CustomJsonSerializer(t.Item1, t.Item2, t.Item3));
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

            var serializingProperties = _serializingPropertiesMap.GetOrAdd(instance.GetType(), GetSerializableProperties);

            foreach (var property in serializingProperties)
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
            var factory = _createObjectFactory.Value.Invoke();

            foreach (var propertyName in reader.ReadProperties())
            {
                if (!factory.SetValue(reader, propertyName, info))
                {
                    reader.Discard();
                }
            }

            return factory.GetInstance();
        }

        private Func<IObjectFactory> GetCreateObjectFactoryFunc()
        {
            var constructor = GetConstructor(_type);
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
            {
                var createInstance = GetCreateInstanceFunc(constructor);

                return () => new DefaultConstructorObjectFactory(_deserializingPropertiesMap, createInstance());
            }
            else
            {
                var createInstance = GetCreateInstanceFunc(constructor, parameters);
                var getSerializerAndArgIndex = GetGetSerializerAndArgIndexFunc(parameters);
                var parametersLength = parameters.Length;

                return () => new NonDefaultConstructorObjectFactory(
                    _deserializingPropertiesMap,
                    createInstance,
                    getSerializerAndArgIndex,
                    parametersLength);
            }
        }

        private Func<object> GetCreateInstanceFunc(ConstructorInfo constructor)
        {
            Expression invokeConstructor = Expression.New(constructor);

            if (_type.IsValueType) // Boxing is necessary
            {
                invokeConstructor = Expression.Convert(invokeConstructor, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object>>(invokeConstructor);

            var createInstance = lambda.Compile();

            return createInstance;
        }

        private Func<object[], object> GetCreateInstanceFunc(ConstructorInfo constructor, ParameterInfo[] parameters)
        {
            var argsParameter = Expression.Parameter(typeof(object[]), "args");

            var constructorArgs = new Expression[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                constructorArgs[i] =
                    Expression.Convert(
                        Expression.ArrayAccess(argsParameter, Expression.Constant(i)),
                        parameters[i].ParameterType);
            }

            Expression invokeConstructor = Expression.New(constructor, constructorArgs);

            if (_type.IsValueType) // Boxing is necessary
            {
                invokeConstructor = Expression.Convert(invokeConstructor, typeof(object));
            }

            var createInstanceLambda = Expression.Lambda<Func<object[], object>>(invokeConstructor, argsParameter);

            var createInstance = createInstanceLambda.Compile();

            return createInstance;
        }

        private Func<string, Tuple<IJsonSerializerInternal, int>> GetGetSerializerAndArgIndexFunc(ParameterInfo[] parameters)
        {
            var propertyNameParameter = Expression.Parameter(typeof(string), "propertyName");

            var switchCases = new SwitchCase[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var serializer = JsonSerializerFactory.GetSerializer(parameters[i].ParameterType, _encrypt, _mappings);
                var serializerAndArgIndex = Tuple.Create(serializer, i);

                var matchingProperties =
                    _type.GetProperties().Where(p =>
                        p.Name.Equals(parameters[i].Name, StringComparison.OrdinalIgnoreCase)).ToList();
                
                var switchLabel = matchingProperties.Count == 1 ? matchingProperties[0].GetName() : parameters[i].Name;

                switchCases[i] = Expression.SwitchCase(
                    Expression.Constant(serializerAndArgIndex),
                    Expression.Constant(switchLabel));
            }

            var defaultCase = Expression.Constant(null, typeof(Tuple<IJsonSerializerInternal, int>));

            var switchExpression = Expression.Switch(propertyNameParameter, defaultCase, switchCases);

            var getSerializerAndArgIndexLambda =
                Expression.Lambda<Func<string, Tuple<IJsonSerializerInternal, int>>>(
                    switchExpression, propertyNameParameter);

            var getSerializerAndArgIndex = getSerializerAndArgIndexLambda.Compile();

            return getSerializerAndArgIndex;
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            if (type.IsAbstract)
            {
                throw new XSerializerException("Cannot instantiate abstract type: " + type.FullName);
            }

            var constructors = type.GetConstructors();

            if (constructors.Length == 0)
            {
                constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

                if (constructors.Length == 0)
                {
                    throw new XSerializerException("Cannot instantiate static class: " + type.FullName);
                }
            }

            if (constructors.Length == 1)
            {
                return constructors[0];
            }

            var decoratedConstructors = constructors.Where(IsDecoratedWithJsonConstructorAttribute).ToArray();

            if (decoratedConstructors.Length == 1)
            {
                return decoratedConstructors[0];
            }

            if (decoratedConstructors.Length > 1)
            {
                throw new XSerializerException("More than one constructor is decorated with the JsonConstructor attribute: " + type.FullName);
            }

            var defaultConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

            if (defaultConstructor != null)
            {
                return defaultConstructor;
            }

            throw new XSerializerException("More than one non-default constructor defined in type: " + type.FullName);
        }

        private static bool IsDecoratedWithJsonConstructorAttribute(ConstructorInfo constructor)
        {
            return Attribute.IsDefined(constructor, typeof(JsonConstructorAttribute))
                   || Attribute.GetCustomAttributes(constructor).Any(attribute =>
                       attribute.GetType().FullName == "Newtonsoft.Json.JsonConstructorAttribute");
        }

        private interface IObjectFactory
        {
            bool SetValue(JsonReader reader, string propertyName, IJsonSerializeOperationInfo info);
            object GetInstance();
        }

        private class DefaultConstructorObjectFactory : IObjectFactory
        {
            private readonly Dictionary<string, SerializableJsonProperty> _serializablePropertiesMap;
            private readonly object _instance;

            public DefaultConstructorObjectFactory(
                Dictionary<string, SerializableJsonProperty> serializablePropertiesMap,
                object instance)
            {
                _serializablePropertiesMap = serializablePropertiesMap;
                _instance = instance;
            }

            public bool SetValue(JsonReader reader, string propertyName, IJsonSerializeOperationInfo info)
            {
                SerializableJsonProperty property;

                if (_serializablePropertiesMap.TryGetValue(propertyName, out property))
                {
                    property.SetValue(_instance, reader, info);
                    return true;
                }

                return false;
            }

            public object GetInstance()
            {
                return _instance;
            }
        }

        private class NonDefaultConstructorObjectFactory : IObjectFactory
        {
            private readonly Dictionary<string, SerializableJsonProperty> _serializablePropertiesMap;
            private readonly Func<object[], object> _createInstance;
            private readonly Func<string, Tuple<IJsonSerializerInternal, int>> _getSerializerAndArgIndex;
            private readonly object[] _constructorArguments;
            private readonly List<Action<object>> _setPropertyValueActions = new List<Action<object>>(); 

            public NonDefaultConstructorObjectFactory(
                Dictionary<string, SerializableJsonProperty> serializablePropertiesMap,
                Func<object[], object> createInstance,
                Func<string, Tuple<IJsonSerializerInternal, int>> getSerializerAndArgIndex,
                int argumentsLength)
            {
                _serializablePropertiesMap = serializablePropertiesMap;
                _createInstance = createInstance;
                _getSerializerAndArgIndex = getSerializerAndArgIndex;
                _constructorArguments = new object[argumentsLength];
            }

            public bool SetValue(JsonReader reader, string propertyName, IJsonSerializeOperationInfo info)
            {
                var serializerAndArgIndex = _getSerializerAndArgIndex(propertyName);

                if (serializerAndArgIndex != null)
                {
                    var value = serializerAndArgIndex.Item1.DeserializeObject(reader, info);
                    _constructorArguments[serializerAndArgIndex.Item2] = value;
                    return true;
                }
                
                SerializableJsonProperty property;
                if (_serializablePropertiesMap.TryGetValue(propertyName, out property))
                {
                    var value = property.ReadValue(reader, info);
                    _setPropertyValueActions.Add(instance => property.SetValue(instance, value));
                    return true;
                }

                return false;
            }

            public object GetInstance()
            {
                var instance = _createInstance(_constructorArguments);

                foreach (var setPropertyValue in _setPropertyValueActions)
                {
                    setPropertyValue(instance);
                }

                return instance;
            }
        }
    }
}