using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class CustomSerializer
    {
        private static readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

        public static IXmlSerializerInternal GetSerializer(Type type, EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            return _serializerCache.GetOrAdd(
                XmlSerializerFactory.Instance.CreateKey(type, encryptAttribute, options),
                _ =>
                {
                    try
                    {
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(CustomSerializer<>).MakeGenericType(type), encryptAttribute, options);
                    }
                    catch (TargetInvocationException ex) // True exception gets masked due to reflection. Preserve stacktrace and rethrow
                    {
                        PreserveStackTrace(ex.InnerException);
                        throw ex.InnerException;
                    }
                });
        }

        //Stackoverflow is awesome
        private static void PreserveStackTrace(Exception e)
        {
            var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
            var mgr = new ObjectManager(null, ctx);
            var si = new SerializationInfo(e.GetType(), new FormatterConverter());

            e.GetObjectData(si, ctx);
            mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
            mgr.DoFixups(); // ObjectManager calls SetObjectData

            // voila, e is unmodified save for _remoteStackTraceString
        }
    }

    internal class CustomSerializer<T> : CustomSerializer, IXmlSerializerInternal
    {
        private readonly EncryptAttribute _encryptAttribute;
        private readonly IXmlSerializerOptions _options;

        private readonly HelperFactory _helperFactory;

        private readonly Dictionary<Type, SerializableProperty[]> _serializablePropertiesMap = new Dictionary<Type, SerializableProperty[]>();
        private readonly Dictionary<Type, SerializableProperty> _encryptedXmlElementListProperties = new Dictionary<Type, SerializableProperty>();

        public CustomSerializer(EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            var type = typeof(T);
            AssertValidHeirarchy(type);

            _encryptAttribute = encryptAttribute;

            _options = options.WithAdditionalExtraTypes(
                type.GetCustomAttributes(typeof(XmlIncludeAttribute), true)
                    .Cast<XmlIncludeAttribute>()
                    .Select(a => a.Type));

            if (string.IsNullOrWhiteSpace(_options.RootElementName))
            {
                _options = _options.WithRootElementName(GetRootElement(type));
            }

            var types = _options.ExtraTypes.ToList();
            if (!type.IsInterface && !type.IsAbstract)
            {
                types.Insert(0, type);
            }

            _serializablePropertiesMap =
                types.Distinct().ToDictionary(
                    t => t,
                    t =>
                    {
                        var serializableProperties =
                            t.GetProperties()
                                .Where(p => p.IsSerializable(t.GetConstructors().SelectMany(c => c.GetParameters())))
                                .Select(p => new SerializableProperty(p, _options))
                                .OrderBy(p => p.NodeType)
                                .ToArray();

                        // Cannot support:
                        // 1) Multiple XmlElement List Properties
                        // 2) When the XmlElement List Property is encrypted, Any Non-XmlAttribute Properties

                        if (serializableProperties.Count(p => p.IsListDecoratedWithXmlElement) > 1)
                        {
                            throw new InvalidOperationException("More than one list property is decorated with [XmlElement] attribute.");
                        }

                        var encryptedXmlElementListProperty =
                            serializableProperties.SingleOrDefault(
                                p => p.IsListDecoratedWithXmlElement && p.EncryptAttribute != null);

                        if (encryptedXmlElementListProperty != null)
                        {
                            if (serializableProperties
                                .Where(p => p != encryptedXmlElementListProperty)
                                .Any(p => p.NodeType != NodeType.Attribute))
                            {
                                throw
                                    new InvalidOperationException(
                                        "A list property decorated with [XmlElement] and [Encrypt] attributes exists"
                                        + " *and* one or more properties exist without [XmlAttribute] attribute.");
                            }

                            _encryptedXmlElementListProperties.Add(t, encryptedXmlElementListProperty);
                        }
                        else
                        {
                            _encryptedXmlElementListProperties.Add(t, null);
                        }

                        return serializableProperties;
                    });

            _helperFactory = new HelperFactory(_serializablePropertiesMap);
        }

        private void AssertValidHeirarchy(Type type)
        {
            if (type.BaseType == typeof(object)) return;

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var derivedXmlElement = GetAttribute<XmlElementAttribute>(property);
                var derivedXmlAttribute = GetAttribute<XmlAttributeAttribute>(property);
                var baseProperty = GetBaseProperty(property);
                var hasBaseProperty = baseProperty != null;

                if (hasBaseProperty)
                {
                    AssertPropertyHeirarchy(baseProperty, derivedXmlElement, derivedXmlAttribute);
                }

                if (derivedXmlAttribute != null && !hasBaseProperty)
                {
                    if (string.IsNullOrWhiteSpace(derivedXmlAttribute.AttributeName))
                    {
                        throw new InvalidOperationException("XmlAttribute must have a value.");
                    }
                }

                if (derivedXmlElement != null && !hasBaseProperty)
                {
                    if (string.IsNullOrWhiteSpace(derivedXmlElement.ElementName))
                    {
                        throw new InvalidOperationException("XmlElement must have a value.");
                    }
                }
            }
        }

        private void AssertPropertyHeirarchy(PropertyInfo baseProperty, XmlElementAttribute derivedXmlElement, XmlAttributeAttribute derivedXmlAttribute)
        {
            var baseXmlElement = GetAttribute<XmlElementAttribute>(baseProperty);
            var baseXmlAttribute = GetAttribute<XmlAttributeAttribute>(baseProperty);

            if (baseXmlAttribute != null)
            {
                if (derivedXmlElement != null)
                {
                    throw new InvalidOperationException("Derived XmlElement cannot override Base XmlAttribute.");
                }

                if (derivedXmlAttribute != null)
                {
                    if (string.IsNullOrWhiteSpace(derivedXmlAttribute.AttributeName))
                    {
                        if (!string.IsNullOrWhiteSpace(baseXmlAttribute.AttributeName))
                        {
                            throw new InvalidOperationException("Overridden property must have non-empty Attribute.");
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(baseXmlAttribute.AttributeName) && !string.IsNullOrWhiteSpace(derivedXmlAttribute.AttributeName))
                        {
                            throw new InvalidOperationException("Virtual property must have non-empty XmlAttribute.");
                        }

                        if (!string.IsNullOrWhiteSpace(baseXmlAttribute.AttributeName) && baseXmlAttribute.AttributeName != derivedXmlAttribute.AttributeName)
                        {
                            throw new InvalidOperationException("Base property and dervied property must have the same attribute.");
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(baseXmlAttribute.AttributeName))
                    {
                        throw new InvalidOperationException("Overridden property must override XmlAttribute");
                    }

                    if (string.IsNullOrWhiteSpace(baseXmlAttribute.AttributeName)) // && string.IsNullOrWhiteSpace(derivedXmlAttribute.AttributeName)
                    {
                        throw new InvalidOperationException("Virtual property must have non-empty XmlAttribute.");
                    }
                }
            }
            else
            {
                if (derivedXmlAttribute != null)
                {
                    throw new InvalidOperationException("Virtual property must have non-empty XmlAttribute.");
                }
            }

            if (baseXmlElement != null)
            {
                if (derivedXmlAttribute != null)
                {
                    throw new InvalidOperationException("Derived XmlAttribute cannot override Base XmlElement.");
                }

                if (derivedXmlElement != null)
                {
                    if (!string.IsNullOrWhiteSpace(baseXmlElement.ElementName))
                    {
                        if (string.IsNullOrWhiteSpace(derivedXmlElement.ElementName))
                        {
                            throw new InvalidOperationException("Cannot have non-empty Xml Element.");
                        }
                        
                        if (derivedXmlElement.ElementName != baseXmlElement.ElementName)
                        {
                            throw new InvalidOperationException("Dervied Element cannot be different from Base element.");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(derivedXmlElement.ElementName))
                        {
                            throw new InvalidOperationException("Base element cannot be empty.");
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(baseXmlElement.ElementName))
                    {
                        throw new InvalidOperationException("Dervied property must override base property XmlElement.");
                    }
                }
            }
            else
            {
                if (derivedXmlElement != null && !string.IsNullOrWhiteSpace(derivedXmlElement.ElementName))
                {
                    throw new InvalidOperationException("Base property must have XmlElement.");
                }
            }
        }

        private static PropertyInfo GetBaseProperty(PropertyInfo propertyInfo)
        {
            var method = propertyInfo.GetAccessors(true)[0];
            if (method == null)
                return null;

            var baseMethod = method.GetBaseDefinition();

            if (baseMethod == method)
                return propertyInfo;

            const BindingFlags allProperties = BindingFlags.Instance | BindingFlags.Public
                                               | BindingFlags.NonPublic | BindingFlags.Static;

            var arguments = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();

            Debug.Assert(baseMethod.DeclaringType != null);

            return baseMethod.DeclaringType.GetProperty(propertyInfo.Name, allProperties,
                null, propertyInfo.PropertyType, arguments, null);
        }

        private TAttribute GetAttribute<TAttribute>(PropertyInfo property) where TAttribute: Attribute
        {
            return property.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
        }

        private static string GetRootElement(Type type)
        {
            var xmlRootAttribute = (XmlRootAttribute)type.GetCustomAttributes(typeof(XmlRootAttribute), true).FirstOrDefault();
            if (xmlRootAttribute != null && !string.IsNullOrWhiteSpace(xmlRootAttribute.ElementName))
            {
                return xmlRootAttribute.ElementName;
            }
            return type.Name;
        }

        public void SerializeObject(XSerializerXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            if (instance == null && !options.ShouldEmitNil)
            {
                return;
            }

            writer.WriteStartDocument();
            writer.WriteStartElement(_options.RootElementName);
            writer.WriteDefaultDocumentNamespaces();

            using (writer.WriteDefaultNamespace(_options.DefaultNamespace))
            {
                if (instance == null)
                {
                    writer.WriteNilAttribute();
                    writer.WriteEndElement();
                    return;
                }

                var instanceType = instance.GetType();

                if (typeof(T).IsInterface || typeof(T).IsAbstract || typeof(T) != instanceType)
                {
                    writer.WriteAttributeString("xsi", "type", null, instance.GetType().GetXsdType());
                }

                var setIsEncryptionEnabledBackToFalse = writer.MaybeSetIsEncryptionEnabledToTrue(_encryptAttribute, options);

                if (instanceType.IsPrimitiveLike() || instanceType.IsNullablePrimitiveLike())
                {
                    var xmlTextSerializer = new XmlTextSerializer(instanceType, _options.RedactAttribute, null, _options.ExtraTypes);
                    xmlTextSerializer.SerializeObject(writer, instance, options);
                }
                else
                {
                    SerializableProperty[] properties;

                    while (!_serializablePropertiesMap.TryGetValue(instanceType, out properties))
                    {
                        instanceType = instanceType.BaseType;

                        if (instanceType == null)
                        {
                            throw new InvalidOperationException("Unable to find serializable properties for type " + instance.GetType());
                        }
                    }

                    foreach (var property in properties)
                    {
                        property.WriteValue(writer, instance, options);
                    }
                }

                if (setIsEncryptionEnabledBackToFalse)
                {
                    writer.IsEncryptionEnabled = false;
                }

                writer.WriteEndElement();
            }
        }

        public object DeserializeObject(XSerializerXmlReader reader, ISerializeOptions options)
        {
            var helper = NullHelper.Instance;

            bool shouldIssueRead;

            bool setIsDecryptionEnabledBackToFalse = false;

            do
            {
                shouldIssueRead = true;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _options.RootElementName && ReferenceEquals(helper, NullHelper.Instance))
                        {
                            if (!typeof(T).IsPrimitiveLike())
                            {
                                var type = reader.GetXsdType<T>(_options.ExtraTypes);

                                if (type == null && typeof(T).IsInterface)
                                {
                                    // We have no idea what concrete type we have here. The only
                                    // successful situation is if we have xsi:nil="true"
                                    if (reader.IsNil())
                                    {
                                        return default(T);
                                    }

                                    throw new InvalidOperationException("Unable to create concrete instance of interface type " + typeof(T) + " - no type hint found.");
                                }

                                if (type == null)
                                {
                                    type = typeof(T);
                                }

                                setIsDecryptionEnabledBackToFalse = reader.MaybeSetIsDecryptionEnabledToTrue(_encryptAttribute, options);

                                helper = _helperFactory.CreateHelper(type, reader);

                                while (reader.MoveToNextAttribute())
                                {
                                    helper.StageAttributeValue(options);
                                }

                                helper.FlushAttributeValues();

                                reader.MoveToElement();

                                if (reader.IsEmptyElement)
                                {
                                    return helper.GetInstance(setIsDecryptionEnabledBackToFalse);
                                }

                                SerializableProperty property;
                                var t = type;

                                do
                                {
                                    if (_encryptedXmlElementListProperties.TryGetValue(t, out property))
                                    {
                                        break;
                                    }
                                } while ((t = t.BaseType) != null);

                                if (property != null && reader.MaybeSetIsDecryptionEnabledToTrue(property.EncryptAttribute, options))
                                {
                                    reader.Read();
                                    helper.SetElementPropertyValue(options, out shouldIssueRead);
                                    reader.IsDecryptionEnabled = false;
                                }
                            }
                            else if (reader.IsEmptyElement)
                            {
                                return default(T);
                            }
                        }
                        else
                        {
                            helper.SetElementPropertyValue(options, out shouldIssueRead);
                        }
                        break;
                    case XmlNodeType.Text:
                        if (typeof(T).IsPrimitiveLike())
                        {
                            // This is left-over (and commented-out) from an earlier time. Is it even possible to have a CustomSerializer for a primitive type???

                            //instance = (T)XmlTextSerializer.GetSerializer(typeof(T), _options.RedactAttribute).DeserializeObject(reader);
                            //hasInstanceBeenCreated = true;
                        }
                        else
                        {
                            helper.SetTextNodePropertyValue(options);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == _options.RootElementName)
                        {
                            return helper.GetInstance(setIsDecryptionEnabledBackToFalse);
                        }
                        break;
                }
            } while (reader.ReadIfNeeded(shouldIssueRead));

            throw new InvalidOperationException("Deserialization error: reached the end of the document without returning a value.");
        }

        private class HelperFactory
        {
            private readonly IDictionary<Type, Lazy<Func<XSerializerXmlReader, IHelper>>> _createHelperFuncs;

            public HelperFactory(Dictionary<Type, SerializableProperty[]> serializablePropertiesMap)
            {
                _createHelperFuncs =
                    serializablePropertiesMap
                        .Where(item => typeof(T).IsAssignableFrom(item.Key))
                        .ToDictionary(
                            item => item.Key,
                            item => new Lazy<Func<XSerializerXmlReader, IHelper>>(() => GetCreateHelperFunc(item.Key, item.Value)));
            }

            private static Func<XSerializerXmlReader, IHelper> GetCreateHelperFunc(Type type, ICollection<SerializableProperty> serializableProperties)
            {
                if (type.IsAbstract)
                {
                    throw new InvalidOperationException("Cannot create instance of abstract type: " + type);
                }

                var properties = type.GetProperties();

                var validConstructors = 
                    type.GetConstructors()
                        .Where(constructor =>
                            constructor.GetParameters()
                                .All(parameter =>
                                    (parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault
                                    || properties.Any(
                                        property =>
                                            property.Name.ToLower() == parameter.Name.ToLower()
                                            && (parameter.ParameterType.IsAssignableFrom(property.PropertyType)
                                                || IsIListParameterAndReadOnlyCollectionProperty(parameter, property)
                                                || IsIDictionaryParameterAndReadOnlyDictionaryProperty(parameter, property)))))
                        .ToArray();

                var caseSensitiveSerializableProperties = serializableProperties.ToDictionary(p => p.Name);
                var textNodeProperty = serializableProperties.SingleOrDefault(p => p.NodeType == NodeType.Text);
                var attributeProperties = serializableProperties.Where(p => p.NodeType == NodeType.Attribute).ToDictionary(p => p.Name);

                if (validConstructors.Length == 0)
                {
                    if (type.IsValueType)
                    {
                        return reader => new DefaultConstructorHelper(
                            () => (T)FormatterServices.GetUninitializedObject(typeof(T)),
                            caseSensitiveSerializableProperties,
                            textNodeProperty,
                            attributeProperties,
                            reader);
                    }

                    throw new InvalidOperationException("No valid constructors were found for type: " + type);
                }

                if (validConstructors.Length == 1 && validConstructors[0].GetParameters().Length == 0)
                {
                    var ctor = validConstructors[0];
                    var lambda = Expression.Lambda<Func<T>>(Expression.Convert(Expression.New(ctor), typeof(T)));
                    var createInstance = lambda.Compile();

                    return reader => new DefaultConstructorHelper(
                        createInstance,
                        caseSensitiveSerializableProperties,
                        textNodeProperty,
                        attributeProperties,
                        reader);
                }

                var constructorWrappers = validConstructors.Select(c => new ConstructorWrapper(c)).ToArray();

                return reader => new NonDefaultConstructorHelper(
                    constructorWrappers,
                    serializableProperties,
                    textNodeProperty,
                    attributeProperties,
                    caseSensitiveSerializableProperties,
                    reader);
            }

            private static bool IsIListParameterAndReadOnlyCollectionProperty(ParameterInfo parameter, PropertyInfo property)
            {
                return (parameter.ParameterType.IsGenericType
                        && parameter.ParameterType.GetGenericTypeDefinition() == typeof(IList<>)
                        && property.PropertyType.IsReadOnlyCollection());
            }

            private static bool IsIDictionaryParameterAndReadOnlyDictionaryProperty(ParameterInfo parameter, PropertyInfo property)
            {
                return (parameter.ParameterType.IsGenericType
                        && parameter.ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                        && property.PropertyType.IsReadOnlyDictionary());
            }

            public IHelper CreateHelper(Type type, XSerializerXmlReader reader)
            {
                var temp = type;
                Lazy<Func<XSerializerXmlReader, IHelper>> createHelper;

                while (!_createHelperFuncs.TryGetValue(temp, out createHelper))
                {
                    temp = temp.BaseType;

                    if (temp == null)
                    {
                        throw new InvalidOperationException("Unable to find serializable properties for type " + type);
                    }
                }

                return createHelper.Value(reader);
            }
        }

        private interface IHelper
        {
            void SetElementPropertyValue(ISerializeOptions options, out bool shouldIssueRead);
            void SetTextNodePropertyValue(ISerializeOptions options);
            void StageAttributeValue(ISerializeOptions options);
            void FlushAttributeValues();
            object GetInstance(bool setIsDecryptionEnabledBackToFalse);
        }

        private class NullHelper : IHelper
        {
            public static readonly IHelper Instance = new NullHelper();

            private NullHelper()
            {
            }

            void IHelper.SetElementPropertyValue(ISerializeOptions options, out bool shouldIssueRead)
            {
                throw NotInitializedException();
            }

            void IHelper.SetTextNodePropertyValue(ISerializeOptions options)
            {
                throw NotInitializedException();
            }

            void IHelper.StageAttributeValue(ISerializeOptions options)
            {
                throw NotInitializedException();
            }

            void IHelper.FlushAttributeValues()
            {
                throw NotInitializedException();
            }

            object IHelper.GetInstance(bool setIsDecryptionEnabledBackToFalse)
            {
                throw NotInitializedException();
            }

            private static Exception NotInitializedException()
            {
                return new InvalidOperationException("Serialization Helper object has not been initialized.");
            }
        }

        private class DefaultConstructorHelper : IHelper
        {
            private readonly List<Action> _setPropertyActions = new List<Action>();

            private readonly XSerializerXmlReader _reader;
            private readonly T _instance;
            private readonly Dictionary<string, SerializableProperty> _caseSensitiveSerializableProperties;
            private readonly SerializableProperty _textNodeProperty;
            private readonly IDictionary<string, SerializableProperty> _attributeProperties;

            public DefaultConstructorHelper(
                Func<T> createInstance,
                Dictionary<string, SerializableProperty> caseSensitiveSerializableProperties,
                SerializableProperty textNodeProperty,
                IDictionary<string, SerializableProperty> attributeProperties,
                XSerializerXmlReader reader)
            {
                _reader = reader;
                _caseSensitiveSerializableProperties = caseSensitiveSerializableProperties;
                _textNodeProperty = textNodeProperty;
                _attributeProperties = attributeProperties;
                _instance = createInstance();
            }

            public void SetElementPropertyValue(ISerializeOptions options, out bool shouldIssueRead)
            {
                SerializableProperty property;
                if (_caseSensitiveSerializableProperties.TryGetValue(_reader.Name, out property))
                {
                    property.ReadValue(_reader, _instance, options);
                    shouldIssueRead = !property.ReadsPastLastElement;
                }
                else
                {
                    shouldIssueRead = true;
                }
            }

            public void SetTextNodePropertyValue(ISerializeOptions options)
            {
                if (_textNodeProperty != null)
                {
                    _textNodeProperty.ReadValue(_reader, _instance, options);
                }
            }

            public void StageAttributeValue(ISerializeOptions options)
            {
                SerializableProperty property;
                if (_attributeProperties.TryGetValue(_reader.Name, out property))
                {
                    _setPropertyActions.Add(() => property.ReadValue(_reader, _instance, options));
                }
            }

            public void FlushAttributeValues()
            {
                _setPropertyActions.ForEach(a => a());
                _setPropertyActions.Clear();
            }

            public object GetInstance(bool setIsDecryptionEnabledBackToFalse)
            {
                if (setIsDecryptionEnabledBackToFalse)
                {
                    _reader.IsDecryptionEnabled = false;
                }

                return _instance;
            }
        }

        private class NonDefaultConstructorHelper : IHelper
        {
            private readonly IDictionary<string, object> _accumulatedValues = new Dictionary<string, object>();
            private readonly List<Action> _setPropertyActions = new List<Action>();

            private readonly IEnumerable<ConstructorWrapper> _constructors;

            private readonly IEnumerable<SerializableProperty> _serializableProperties;
            private readonly SerializableProperty _textNodeProperty;
            private readonly IDictionary<string, SerializableProperty> _attributeProperties;
            private readonly IDictionary<string, SerializableProperty> _caseSensitiveSerializableProperties;

            private readonly XSerializerXmlReader _reader;

            public NonDefaultConstructorHelper(
                IEnumerable<ConstructorWrapper> constructors,
                IEnumerable<SerializableProperty> serializableProperties,
                SerializableProperty textNodeProperty,
                IDictionary<string, SerializableProperty> attributeProperties,
                IDictionary<string, SerializableProperty> caseSensitiveSerializableProperties,
                XSerializerXmlReader reader)
            {
                _constructors = constructors;
                _serializableProperties = serializableProperties;
                _textNodeProperty = textNodeProperty;
                _attributeProperties = attributeProperties;
                _caseSensitiveSerializableProperties = caseSensitiveSerializableProperties;
                _reader = reader;
            }

            public void SetElementPropertyValue(ISerializeOptions options, out bool shouldIssueRead)
            {
                SerializableProperty property;
                if (_caseSensitiveSerializableProperties.TryGetValue(_reader.Name, out property))
                {
                    var value = property.ReadValue(_reader, options);

                    if (_accumulatedValues.ContainsKey(property.Name.ToLower()))
                    {
                        var existingValue = _accumulatedValues[property.Name.ToLower()];
                        Combine(existingValue, value);
                    }
                    else
                    {
                        _accumulatedValues.Add(property.Name.ToLower(), value);
                    }

                    shouldIssueRead = !property.ReadsPastLastElement;
                }
                else
                {
                    shouldIssueRead = true;
                }
            }

            private static void Combine(object existingValue, object value)
            {
                var list = value as IList;
                var existingList = existingValue as IList;

                if (list != null && existingList != null)
                {
                    foreach (var item in list)
                    {
                        existingList.Add(item);
                    }

                    return;
                }

                throw new InvalidOperationException();
            }

            public void SetTextNodePropertyValue(ISerializeOptions options)
            {
                if (_textNodeProperty != null)
                {
                    var value = _textNodeProperty.ReadValue(_reader, options);

                    _accumulatedValues[_textNodeProperty.Name.ToLower()] = value;
                }
            }

            public void StageAttributeValue(ISerializeOptions options)
            {
                SerializableProperty property;
                if (_attributeProperties.TryGetValue(_reader.Name, out property))
                {
                    _setPropertyActions.Add(
                        () =>
                        {
                            var value = property.ReadValue(_reader, options);

                            _accumulatedValues[property.Name.ToLower()] = value;
                        });
                }
            }

            public void FlushAttributeValues()
            {
                _setPropertyActions.ForEach(a => a());
                _setPropertyActions.Clear();
            }

            public object GetInstance(bool setIsDecryptionEnabledBackToFalse)
            {
                var constructor = _constructors.OrderByDescending(c => c.GetScore(_accumulatedValues)).First();

                IEnumerable<SerializableProperty> remainingProperties;
                var instance = constructor.Invoke(_accumulatedValues, _serializableProperties, out remainingProperties);

                foreach (var property in remainingProperties)
                {
                    object value;
                    if (_accumulatedValues.TryGetValue(property.Name.ToLower(), out value))
                    {
                        property.SetValue(instance, value);
                    }
                }

                if (setIsDecryptionEnabledBackToFalse)
                {
                    _reader.IsDecryptionEnabled = false;
                }

                return instance;
            }
        }

        private class ConstructorWrapper
        {
            private readonly IList<ParameterInfo> _parameters;
            private readonly IList<string> _parameterNames;

            private readonly Func<object[], object> _createInstance; 

            public ConstructorWrapper(ConstructorInfo constructor)
            {
                _parameters = constructor.GetParameters().ToList();
                _parameterNames = _parameters.Select(p => p.Name.ToLower()).ToList();

                var argsParameter = Expression.Parameter(typeof(object[]), "args");

                var convertIfNecessaryMethod = typeof(SerializationExtensions).GetMethod("ConvertIfNecessary", BindingFlags.Static | BindingFlags.NonPublic);

                Expression body = Expression.New(
                    constructor,
                    _parameters.Select(
                        (p, i) =>
                            Expression.Convert(
                                Expression.Call(
                                    convertIfNecessaryMethod,
                                    Expression.ArrayAccess(
                                        argsParameter,
                                        Expression.Constant(i)),
                                    Expression.Constant(p.ParameterType)),
                                p.ParameterType)));

                if (typeof(T).IsValueType)
                {
                    body = Expression.Convert(body, typeof (object));
                }

                var lambda = Expression.Lambda<Func<object[], object>>(body, argsParameter);

                _createInstance = lambda.Compile();
            }

            public int GetScore(IDictionary<string, object> availableValues)
            {
                // TODO: this needs work - the algorithm isn't quite right.
                var matchedParameterCount = _parameterNames.Count(availableValues.ContainsKey);

                return (matchedParameterCount * 100) - ((_parameterNames.Count - matchedParameterCount) * 99);
            }

            public object Invoke(IDictionary<string, object> availableValues, IEnumerable<SerializableProperty> serializableProperties, out IEnumerable<SerializableProperty> remainingProperties)
            {
                remainingProperties = serializableProperties.Where(p => !_parameterNames.Contains(p.Name.ToLower()));

                var args = new object[_parameterNames.Count];

                for (int i = 0; i < args.Length; i++)
                {
                    object value;

                    if (!availableValues.TryGetValue(_parameterNames[i], out value))
                    {
                        if (HasDefaultValue(_parameters[i]))
                        {
                            value = _parameters[i].DefaultValue;
                        }
                        else
                        {
                            var parameterType = _parameters[i].ParameterType;
                            value =
                                parameterType.IsReferenceType() || parameterType.IsNullableType()
                                    ? null
                                    : parameterType.GetUninitializedObject();
                        }
                    }

                    args[i] = value;
                }

                return _createInstance(args);
            }

            private static bool HasDefaultValue(ParameterInfo parameter)
            {
                return (parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault;
            }
        }
    }
}