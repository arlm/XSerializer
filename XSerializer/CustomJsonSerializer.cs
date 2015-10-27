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
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.InvariantCultureIgnoreCase);

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
                if (!helper.SetValue(reader, propertyName, info))
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
                var constructor = GetConstructor(t);
                var parameters = constructor.GetParameters();

                if (parameters.Length == 0)
                {
                    Expression invokeConstructor = Expression.New(constructor);

                    if (t.IsValueType) // Boxing is necessary
                    {
                        invokeConstructor = Expression.Convert(invokeConstructor, typeof(object));
                    }

                    var lambda = Expression.Lambda<Func<object>>(invokeConstructor);
                    var createInstance = lambda.Compile();

                    return () => new DefaultConstructorHelper(_serializablePropertiesMap, createInstance());
                }
                else
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

                    if (t.IsValueType) // Boxing is necessary
                    {
                        invokeConstructor = Expression.Convert(invokeConstructor, typeof(object));
                    }

                    var createInstanceLambda = Expression.Lambda<Func<object[], object>>(invokeConstructor, argsParameter);
                    var createInstance = createInstanceLambda.Compile();

                    var tupleConstructor = typeof(Tuple<IJsonSerializerInternal, int>).GetConstructors()[0];

                    var propertyNameParameter = Expression.Parameter(typeof(string), "propertyName");

                    var switchCases = new SwitchCase[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var serializer = JsonSerializerFactory.GetSerializer(parameters[i].ParameterType, _encrypt);
                        
                        switchCases[i] = Expression.SwitchCase(
                            Expression.New(tupleConstructor, Expression.Constant(serializer), Expression.Constant(i)),
                            Expression.Constant(parameters[i].Name.ToLower()));
                    }

                    var defaultCase = Expression.Constant(null, typeof(Tuple<IJsonSerializerInternal, int>));

                    var switchExpression = Expression.Switch(propertyNameParameter, defaultCase, switchCases);

                    var getConstructorArgumentSerializerInfoLambda =
                        Expression.Lambda<Func<string, Tuple<IJsonSerializerInternal, int>>>(
                            switchExpression, propertyNameParameter);
                    var getConstructorArgumentSerializerInfo = getConstructorArgumentSerializerInfoLambda.Compile();

                    var constructorParameters = parameters.Length;

                    return () => new NonDefaultConstructorHelper(
                        _serializablePropertiesMap,
                        createInstance,
                        getConstructorArgumentSerializerInfo,
                        constructorParameters);
                }
            });

            return createHelper();
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
            // TODO: Add support for JSON.NET's JsonConstructorAttribute.

            return Attribute.IsDefined(constructor, typeof(JsonConstructorAttribute));
        }

        private interface IHelper
        {
            bool SetValue(JsonReader reader, string propertyName, IJsonSerializeOperationInfo info);
            object GetInstance();
        }

        private class DefaultConstructorHelper : IHelper
        {
            private readonly Dictionary<string, SerializableJsonProperty> _serializablePropertiesMap;
            private readonly object _instance;

            public DefaultConstructorHelper(
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

        private class NonDefaultConstructorHelper : IHelper
        {
            private readonly Dictionary<string, SerializableJsonProperty> _serializablePropertiesMap;
            private readonly Func<object[], object> _createInstance;
            private readonly Func<string, Tuple<IJsonSerializerInternal, int>> _getConstructorArgumentSerializerInfo;
            private readonly object[] _constructorArguments;
            private readonly List<Action<object>> _setValueActions = new List<Action<object>>(); 

            public NonDefaultConstructorHelper(
                Dictionary<string, SerializableJsonProperty> serializablePropertiesMap,
                Func<object[], object> createInstance,
                Func<string, Tuple<IJsonSerializerInternal, int>> getConstructorArgumentSerializerInfo,
                int constructorParameters)
            {
                _serializablePropertiesMap = serializablePropertiesMap;
                _createInstance = createInstance;
                _getConstructorArgumentSerializerInfo = getConstructorArgumentSerializerInfo;
                _constructorArguments = new object[constructorParameters];
            }

            public bool SetValue(JsonReader reader, string propertyName, IJsonSerializeOperationInfo info)
            {
                var constructorArgumentSerializerInfo = _getConstructorArgumentSerializerInfo(propertyName.ToLower());

                if (constructorArgumentSerializerInfo != null)
                {
                    var value = constructorArgumentSerializerInfo.Item1.DeserializeObject(reader, info);
                    _constructorArguments[constructorArgumentSerializerInfo.Item2] = value;
                    return true;
                }
                
                SerializableJsonProperty property;
                if (_serializablePropertiesMap.TryGetValue(propertyName, out property))
                {
                    var value = property.ReadValue(reader, info);
                    _setValueActions.Add(instance => property.SetValue(instance, value));
                    return true;
                }

                return false;
            }

            public object GetInstance()
            {
                var instance = _createInstance(_constructorArguments);

                foreach (var setValue in _setValueActions)
                {
                    setValue(instance);
                }

                return instance;
            }
        }
    }
}