using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class CustomSerializer
    {
        private static readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();
        protected static readonly Dictionary<int, Type> _typeCache = new Dictionary<int, Type>();

        public static IXmlSerializer GetSerializer(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            IXmlSerializer serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(type, defaultNamespace, extraTypes, rootElementName);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                try
                {
                    serializer = (IXmlSerializer)Activator.CreateInstance(typeof(CustomSerializer<>).MakeGenericType(type), defaultNamespace, extraTypes, rootElementName);
                }
                catch (TargetInvocationException ex) // True exception gets masked due to reflection. Preserve stacktrace and rethrow
                {
                    PreserveStackTrace(ex.InnerException);
                    throw ex.InnerException;
                }

                _serializerCache[key] = serializer;
            }

            return serializer;
        }

        protected static int CreateTypeCacheKey<T>(string typeName)
        {
            unchecked
            {
                var key = typeof(T).GetHashCode();
                key = (key * 397) ^ typeName.GetHashCode();
                return key;
            }
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

    public class CustomSerializer<T> : CustomSerializer, IXmlSerializer<T>
    {
        private readonly string _defaultNamespace;
        private readonly Type[] _extraTypes;
        private readonly string _rootElementName;
        private readonly Dictionary<Type, SerializableProperty[]> _serializablePropertiesMap = new Dictionary<Type, SerializableProperty[]>();

        public CustomSerializer(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            _defaultNamespace = defaultNamespace;
            var type = typeof (T);

            _rootElementName = !string.IsNullOrWhiteSpace(rootElementName) ? rootElementName : GetRootElement(type);

            AssertValidHeirarchy(type);

            var types = new List<Type>();

            if (extraTypes != null)
            {
                types.AddRange(extraTypes);
            }

            types.AddRange(type.GetCustomAttributes(typeof(XmlIncludeAttribute), true).Cast<XmlIncludeAttribute>().Select(a => a.Type));
            _extraTypes = types.ToArray();

            if (!type.IsInterface && !type.IsAbstract)
            {
                types.Insert(0, type);
            }

            _serializablePropertiesMap =
                types.Distinct().ToDictionary(
                    t => t,
                    t =>
                        t.GetProperties()
                        .Where(p => p.IsSerializable())
                        .Select(p => new SerializableProperty(p, defaultNamespace, _extraTypes))
                        .OrderBy(p => p.NodeType)
                        .ToArray());
        }

        private void AssertValidHeirarchy(Type type)
        {
            if (type.BaseType == typeof (object)) return;

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
                    if(string.IsNullOrWhiteSpace(derivedXmlAttribute.AttributeName))
                        throw new InvalidOperationException("XmlAttribute must have a value.");
                }

                if (derivedXmlElement != null && !hasBaseProperty)
                {
                    if (string.IsNullOrWhiteSpace(derivedXmlElement.ElementName))
                        throw new InvalidOperationException("XmlElement must have a value.");
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

        public void Serialize(SerializationXmlTextWriter writer, T instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            if (instance == null)
            {
                return;
            }

            writer.WriteStartDocument();
            writer.WriteStartElement(_rootElementName);
            writer.WriteDefaultNamespaces();

            if (!string.IsNullOrWhiteSpace(_defaultNamespace))
            {
                writer.WriteAttributeString("xmlns", null, null, _defaultNamespace);
            }

            var instanceType = instance.GetType();

            if (typeof(T).IsInterface || typeof(T).IsAbstract || typeof(T) != instanceType)
            {
                writer.WriteAttributeString("xsi", "type", null, instance.GetType().GetXsdType());
            }

            if (instanceType.IsPrimitiveLike()
                || (instanceType.IsGenericType && instanceType.GetGenericTypeDefinition() == typeof(Nullable<>) && instanceType.GetGenericArguments()[0].IsPrimitiveLike()))
            {
                XmlTextSerializer.GetSerializer(instanceType).SerializeObject(writer, instance, namespaces, alwaysEmitTypes);
            }
            else
            {
                foreach (var property in _serializablePropertiesMap[instanceType])
                {
                    property.WriteValue(writer, instance, namespaces, alwaysEmitTypes);
                }
            }

            writer.WriteEndElement();
        }

        void IXmlSerializer.SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            Serialize(writer, (T)instance, namespaces, alwaysEmitTypes);
        }

        public T Deserialize(XmlReader reader)
        {
            T instance = default(T);
            var hasInstanceBeenCreated = false;

            bool shouldIssueRead;

            do
            {
                shouldIssueRead = true;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _rootElementName)
                        {
                            if (!typeof(T).IsPrimitiveLike())
                            {
                                instance = CreateInstance(reader);
                                hasInstanceBeenCreated = true;

                                var setPropertyActions = new List<Action>();

                                while (reader.MoveToNextAttribute())
                                {
                                    var property =
                                        _serializablePropertiesMap[instance.GetType()]
                                            .SingleOrDefault(p => p.NodeType == NodeType.Attribute && p.Name == reader.Name);
                                    if (property != null)
                                    {
                                        setPropertyActions.Add(() => property.ReadValue(reader, instance));
                                    }
                                }

                                setPropertyActions.ForEach(action => action());

                                reader.MoveToElement();

                                if (reader.IsEmptyElement)
                                {
                                    return instance;
                                }
                            }
                            else if (reader.IsEmptyElement)
                            {
                                return default(T);
                            }
                        }
                        else
                        {
                            SetElementPropertyValue(reader, hasInstanceBeenCreated, instance, out shouldIssueRead);
                        }
                        break;
                    case XmlNodeType.Text:
                        if (typeof(T).IsPrimitiveLike())
                        {
                            instance = (T)XmlTextSerializer.GetSerializer(typeof(T)).DeserializeObject(reader);
                            hasInstanceBeenCreated = true;
                        }
                        else
                        {
                            SetTextNodePropertyValue(reader, hasInstanceBeenCreated, instance);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == _rootElementName)
                        {
                            return CheckAndReturn(hasInstanceBeenCreated, instance);
                        }
                        break;
                }
            } while (reader.ReadIfNeeded(shouldIssueRead));

            throw new SerializationException("Couldn't serialize... for some reason. (You know, I should put a better exception message here...)");
        }

        object IXmlSerializer.DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        // ReSharper disable UnusedParameter.Local
        private void SetElementPropertyValue(XmlReader reader, bool hasInstanceBeenCreated, T instance, out bool shouldIssueRead)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("Boo hoo!");
            }

            var property = _serializablePropertiesMap[instance.GetType()].SingleOrDefault(p => reader.Name == p.Name);
            if (property != null)
            {
                property.ReadValue(reader, instance);
                shouldIssueRead = !property.ReadsPastLastElement;
            }
            else
            {
                shouldIssueRead = true;
            }
        }

        private void SetTextNodePropertyValue(XmlReader reader, bool hasInstanceBeenCreated, T instance)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("I'm think I'm gonna cry!");
            }

            var property = _serializablePropertiesMap[instance.GetType()].SingleOrDefault(p => p.NodeType == NodeType.Text);
            if (property != null)
            {
                property.ReadValue(reader, instance);
            }
        }

        private static T CheckAndReturn(bool hasInstanceBeenCreated, T instance)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("Awwww, crap.");
            }

            return instance;
        }
        // ReSharper restore UnusedParameter.Local

        private T CreateInstance(XmlReader reader)
        {
            T instance;
            var type = reader.GetXsdType<T>(_extraTypes);

            if (type != null)
            {
                instance = (T)Activator.CreateInstance(type); // TODO: cache into constructor func
            }
            else
            {
                instance = Activator.CreateInstance<T>(); // TODO: cache into constructor func
            }
            
            return instance;
        }
    }
}