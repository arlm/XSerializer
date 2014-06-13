using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    internal class CustomSerializer
    {
        private static readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

        public static IXmlSerializerInternal GetSerializer(Type type, IXmlSerializerOptions options)
        {
            return _serializerCache.GetOrAdd(
                XmlSerializerFactory.Instance.CreateKey(type, options),
                _ =>
                {
                    try
                    {
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(CustomSerializer<>).MakeGenericType(type), options);
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

    internal class CustomSerializer<T> : CustomSerializer, IXmlSerializerInternal<T>
    {
        private readonly IXmlSerializerOptions _options;

        private readonly IHelperFactory _helperFactory;

        private readonly Dictionary<Type, SerializableProperty[]> _serializablePropertiesMap = new Dictionary<Type, SerializableProperty[]>();

        public CustomSerializer(IXmlSerializerOptions options)
        {
            var type = typeof(T);
            AssertValidHeirarchy(type);

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
                        t.GetProperties()
                            .Where(p => p.IsSerializable(t.GetConstructors().SelectMany(c => c.GetParameters())))
                            .Select(p => new SerializableProperty(p, _options))
                            .OrderBy(p => p.NodeType)
                            .ToArray());

            // TODO: find the constructor with the most parameters whose names match properties of the type.
            // TODO: if the found constructor is no parameters, use DefaultConstructorHelperFactory.
            // TODO: else, use the other (as yet unimplemented) helper factory.
            _helperFactory = new DefaultConstructorHelperFactory(_serializablePropertiesMap, _options);
        }

        private static ILookup<Type, IEnumerable<ConstructorInfo>> GetValidConstructors(IEnumerable<Type> types)
        {
            return
                types.ToLookup(
                    t => t,
                    t =>
                    {
                        var properties = t.GetProperties();

                        return
                            t.GetConstructors()
                                .Where(constructor =>
                                    constructor.GetParameters()
                                        .All(parameter =>
                                            (parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault
                                            || properties.Any(property =>
                                                    property.Name.ToLower() == parameter.Name.ToLower() &&
                                                    property.PropertyType == parameter.ParameterType)));
                    });
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

        public void Serialize(SerializationXmlTextWriter writer, T instance, ISerializeOptions options)
        {
            if (instance == null)
            {
                return;
            }

            writer.WriteStartDocument();
            writer.WriteStartElement(_options.RootElementName);
            writer.WriteDefaultNamespaces();

            if (!string.IsNullOrWhiteSpace(_options.DefaultNamespace))
            {
                writer.WriteAttributeString("xmlns", null, null, _options.DefaultNamespace);
            }

            var instanceType = instance.GetType();

            if (typeof(T).IsInterface || typeof(T).IsAbstract || typeof(T) != instanceType)
            {
                writer.WriteAttributeString("xsi", "type", null, instance.GetType().GetXsdType());
            }

            if (instanceType.IsPrimitiveLike() || instanceType.IsNullablePrimitiveLike())
            {
                XmlTextSerializer.GetSerializer(instanceType, _options.RedactAttribute).SerializeObject(writer, instance, options);
            }
            else
            {
                foreach (var property in _serializablePropertiesMap[instanceType])
                {
                    property.WriteValue(writer, instance, options);
                }
            }

            writer.WriteEndElement();
        }

        void IXmlSerializerInternal.SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            Serialize(writer, (T)instance, options);
        }

        public T Deserialize(XmlReader reader)
        {
            var helper = _helperFactory.CreateHelper(reader);

            bool shouldIssueRead;

            do
            {
                shouldIssueRead = true;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _options.RootElementName)
                        {
                            if (!typeof(T).IsPrimitiveLike())
                            {
                                helper.CreateInstance();

                                while (reader.MoveToNextAttribute())
                                {
                                    helper.StageAttributeValue();
                                }

                                helper.FlushAttributeValues();

                                reader.MoveToElement();

                                if (reader.IsEmptyElement)
                                {
                                    return helper.GetInstance();
                                }
                            }
                            else if (reader.IsEmptyElement)
                            {
                                return default(T);
                            }
                        }
                        else
                        {
                            helper.SetElementPropertyValue(out shouldIssueRead);
                        }
                        break;
                    case XmlNodeType.Text:
                        if (typeof(T).IsPrimitiveLike())
                        {
                            //instance = (T)XmlTextSerializer.GetSerializer(typeof(T), _options.RedactAttribute).DeserializeObject(reader);
                            //hasInstanceBeenCreated = true;
                        }
                        else
                        {
                            helper.SetTextNodePropertyValue();
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == _options.RootElementName)
                        {
                            return helper.GetInstance();
                        }
                        break;
                }
            } while (reader.ReadIfNeeded(shouldIssueRead));

            throw new InvalidOperationException("Deserialization error: reached the end of the document without returning a value.");
        }

        object IXmlSerializerInternal.DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        public interface IHelperFactory
        {
            IHelper CreateHelper(XmlReader reader);
        }

        public class DefaultConstructorHelperFactory : IHelperFactory
        {
            private readonly IXmlSerializerOptions _options;
            private readonly Dictionary<Type, SerializableProperty[]> _serializablePropertiesMap = new Dictionary<Type, SerializableProperty[]>();

            public DefaultConstructorHelperFactory(Dictionary<Type, SerializableProperty[]> serializablePropertiesMap, IXmlSerializerOptions options)
            {
                _options = options;
                _serializablePropertiesMap = serializablePropertiesMap;
            }

            public IHelper CreateHelper(XmlReader reader)
            {
                return new DefaultConstructorHelper(_serializablePropertiesMap, _options, reader);
            }
        }

        public interface IHelper
        {
            void SetElementPropertyValue(out bool shouldIssueRead);
            void SetTextNodePropertyValue();
            void StageAttributeValue();
            void FlushAttributeValues();
            T GetInstance();
            void CreateInstance();
        }

        public class DefaultConstructorHelper : IHelper
        {
            private readonly IXmlSerializerOptions _options;
            private readonly XmlReader _reader;
            private T _instance;
            private readonly Dictionary<Type, SerializableProperty[]> _serializablePropertiesMap = new Dictionary<Type, SerializableProperty[]>();
            private bool _hasInstanceBeenCreated;

            public DefaultConstructorHelper(Dictionary<Type, SerializableProperty[]> serializablePropertiesMap, IXmlSerializerOptions options, XmlReader reader)
            {
                _reader = reader;
                _options = options;

                _serializablePropertiesMap = serializablePropertiesMap;
            }

            public void SetElementPropertyValue(out bool shouldIssueRead)
            {
                var property = _serializablePropertiesMap[_instance.GetType()].SingleOrDefault(p => _reader.Name == p.Name);
                if (property != null)
                {
                    property.ReadValue(_reader, _instance);
                    shouldIssueRead = !property.ReadsPastLastElement;
                }
                else
                {
                    shouldIssueRead = true;
                }
            }

            public void SetTextNodePropertyValue()
            {
                var property = _serializablePropertiesMap[_instance.GetType()].SingleOrDefault(p => p.NodeType == NodeType.Text);
                if (property != null)
                {
                    property.ReadValue(_reader, _instance);
                }
            }

            private readonly List<Action> _setPropertyActions = new List<Action>();

            public void StageAttributeValue()
            {
                var property =
                    _serializablePropertiesMap[_instance.GetType()]
                        .SingleOrDefault(p => p.NodeType == NodeType.Attribute && p.Name == _reader.Name);
                if (property != null)
                {
                    _setPropertyActions.Add(() => property.ReadValue(_reader, _instance));
                }
            }

            public void FlushAttributeValues()
            {
                _setPropertyActions.ForEach(a => a());
                _setPropertyActions.Clear();
            }

            public T GetInstance()
            {
                if (!_hasInstanceBeenCreated)
                {
                    throw new Exception("Instance has not been created");
                }

                return _instance;
            }

            public void CreateInstance()
            {
                var type = _reader.GetXsdType<T>(_options.ExtraTypes);

                // This is the spot where we would access the constructor from the lookup.

                if (type != null)
                {
                    _instance = (T)Activator.CreateInstance(type); // TODO: cache into constructor func
                }
                else
                {
                    _instance = Activator.CreateInstance<T>(); // TODO: cache into constructor func
                }

                _hasInstanceBeenCreated = true;
            }
        }
    }
}