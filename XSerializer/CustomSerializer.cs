using System;
using System.Collections.Generic;
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
                serializer = (IXmlSerializer)Activator.CreateInstance(typeof(CustomSerializer<>).MakeGenericType(type), defaultNamespace, extraTypes, rootElementName);
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
    }

    public class CustomSerializer<T> : CustomSerializer, IXmlSerializer<T>
    {
        private readonly string _defaultNamespace;
        private readonly string _rootElementName;
        private readonly Dictionary<Type, SerializableProperty[]> _serializablePropertiesMap = new Dictionary<Type, SerializableProperty[]>();

        public CustomSerializer(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            _defaultNamespace = defaultNamespace;
            if (!string.IsNullOrWhiteSpace(rootElementName))
            {
                _rootElementName = rootElementName;
            }
            else
            {
                var xmlRootAttribute = (XmlRootAttribute)typeof(T).GetCustomAttributes(typeof(XmlRootAttribute), true).FirstOrDefault();
                if (xmlRootAttribute != null && !string.IsNullOrWhiteSpace(xmlRootAttribute.ElementName))
                {
                    _rootElementName = xmlRootAttribute.ElementName;
                }
                else
                {
                    _rootElementName = typeof(T).Name;
                }
            }

            var types = new List<Type>();

            if (extraTypes != null)
            {
                types.AddRange(extraTypes);
            }

            types.AddRange(typeof(T).GetCustomAttributes(typeof(XmlIncludeAttribute), true).Cast<XmlIncludeAttribute>().Select(a => a.Type));

            if (!typeof(T).IsInterface && !typeof(T).IsAbstract)
            {
                types.Insert(0, typeof(T));
            }

            _serializablePropertiesMap =
                types.Distinct().ToDictionary(
                    t => t,
                    t =>
                        t.GetProperties()
                        .Where(p => p.IsSerializable())
                        .Select(p => new SerializableProperty(p, defaultNamespace, extraTypes))
                        .OrderBy(p => p.NodeType)
                        .ToArray());
        }

        public void Serialize(SerializationXmlTextWriter writer, T instance, XmlSerializerNamespaces namespaces)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(_rootElementName);
            writer.WriteDefaultNamespaces();

            if (!string.IsNullOrWhiteSpace(_defaultNamespace))
            {
                writer.WriteAttributeString("xmlns", null, null, _defaultNamespace);
            }

            if (typeof(T).IsInterface || typeof(T).IsAbstract)
            {
                writer.WriteAttributeString("xsi", "type", null, instance.GetType().Name);
            }

            var type = instance.GetType();
            foreach (var property in _serializablePropertiesMap[type])
            {
                property.WriteValue(writer, instance, namespaces);
            }

            writer.WriteEndElement();
        }

        void IXmlSerializer.SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces)
        {
            Serialize(writer, (T)instance, namespaces);
        }

        public T Deserialize(XmlReader reader)
        {
            T instance = default(T);
            var hasInstanceBeenCreated = false;

            var attributes = new Dictionary<string, string>();

            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _rootElementName)
                        {
                            instance = CreateInstanceAndSetAttributePropertyValues(reader, attributes);
                            hasInstanceBeenCreated = true;
                        }
                        else
                        {
                            SetElementPropertyValue(reader, hasInstanceBeenCreated, instance);
                        }
                        break;
                    case XmlNodeType.Text:
                        SetTextNodePropertyValue(reader, hasInstanceBeenCreated, instance);
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == _rootElementName)
                        {
                            return CheckAndReturn(hasInstanceBeenCreated, instance);
                        }
                        break;
                    case XmlNodeType.CDATA:
                        break;
                }
            } while (reader.Read());

            throw new SerializationException("Couldn't serialize... for some reason. (You know, I should put a better exception message here...)");
        }

        object IXmlSerializer.DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        // ReSharper disable UnusedParameter.Local
        private void SetElementPropertyValue(XmlReader reader, bool hasInstanceBeenCreated, T instance)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("Boo hoo!");
            }

            var property = _serializablePropertiesMap[instance.GetType()].SingleOrDefault(p => reader.Name == p.Name);
            if (property != null)
            {
                property.ReadValue(reader, instance);
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

        private T CreateInstanceAndSetAttributePropertyValues(XmlReader reader, Dictionary<string, string> attributes)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    attributes.Add(reader.Name, reader.Value);
                } while (reader.MoveToNextAttribute());
            }

            var instance = CreateInstance(attributes);

            foreach (var attribute in attributes)
            {
                var property =
                    _serializablePropertiesMap[instance.GetType()]
                        .SingleOrDefault(p => p.NodeType == NodeType.Attribute && p.Name == attribute.Key);
                if (property != null)
                {
                    property.ReadValue(reader, instance);
                }
            }

            return instance;
        }

        private static T CreateInstance(IDictionary<string, string> attributes)
        {
            T instance;

            string typeName;
            if (attributes.TryGetValue("xsi:type", out typeName))
            {
                Type type;
                var key = CreateTypeCacheKey<T>(typeName);
                if (!_typeCache.TryGetValue(key, out type))
                {
                    // try REAL hard to get the type. (holy crap, this is UUUUUGLY!!!!)

                    var typeNameWithPossibleNamespace = typeName;

                    if (!typeName.Contains('.'))
                    {
                        typeNameWithPossibleNamespace = typeof(T).Namespace + "." + typeName;
                    }

                    var checkPossibleNamespace = typeName != typeNameWithPossibleNamespace;

                    type = Type.GetType(typeName);
                    if (type == null)
                    {
                        type = checkPossibleNamespace ? Type.GetType(typeNameWithPossibleNamespace) : null;
                        if (type == null)
                        {
                            type = typeof(T).Assembly.GetType(typeName);
                            if (type == null)
                            {
                                type = checkPossibleNamespace ? typeof(T).Assembly.GetType(typeNameWithPossibleNamespace) : null;
                                if (type == null)
                                {
                                    var entryAssembly = Assembly.GetEntryAssembly();
                                    if (entryAssembly != null)
                                    {
                                        type = entryAssembly.GetType(typeName);
                                        if (type == null)
                                        {
                                            type = checkPossibleNamespace ? entryAssembly.GetType(typeNameWithPossibleNamespace) : null;
                                        }
                                    }

                                    if (type == null)
                                    {
                                        type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.Name == typeName);
                                        if (type == null)
                                        {
                                            type = checkPossibleNamespace ? AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.Name == typeNameWithPossibleNamespace) : null;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (type == null)
                    {
                        throw new SerializationException("WAAAAAAA!");
                    }

                    _typeCache[key] = type;
                }

                instance = (T)Activator.CreateInstance(type);
            }
            else
            {
                instance = Activator.CreateInstance<T>();
            }
            
            return instance;
        }
    }
}