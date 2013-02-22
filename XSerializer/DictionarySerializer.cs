namespace XSerializer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Serialization;

    public class DictionarySerializer
    {
        private static readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();
        private static readonly Dictionary<Type, object> _valueTypeDefaultValueMap = new Dictionary<Type, object>();

        public static IXmlSerializer GetSerializer(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            IXmlSerializer serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(type, defaultNamespace, extraTypes, rootElementName);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                serializer = (IXmlSerializer)Activator.CreateInstance(typeof(DictionarySerializer<>).MakeGenericType(type), defaultNamespace, extraTypes, rootElementName);
                _serializerCache[key] = serializer;
            }

            return serializer;
        }

        protected static object GetValueTypeDefault(Type valueType)
        {
            object defaultValue;
            if (!_valueTypeDefaultValueMap.TryGetValue(valueType, out defaultValue))
            {
                var getValueTypeDefaultMethod =
                    typeof(DictionarySerializer).GetMethod(
                        "GetValueTypeDefault", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[0], null)
                            .MakeGenericMethod(valueType);
                defaultValue = getValueTypeDefaultMethod.Invoke(null, null);
                _valueTypeDefaultValueMap[valueType] = defaultValue;
            }

            return defaultValue;
        }

        private static object GetValueTypeDefault<TValueType>()
            where TValueType : struct
        {
            return default(TValueType);
        }
    }

    public class DictionarySerializer<T> : DictionarySerializer, IXmlSerializer<T>
        where T : IDictionary, new()
    {
        private readonly string _defaultNamespace;
        private readonly Type[] _extraTypes;
        private readonly string _rootElementName;

        private readonly IXmlSerializer _keySerializer;
        private readonly IXmlSerializer _valueSerializer;

        private readonly Type _keyType;
        private readonly Type _valueType;

        public DictionarySerializer(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            _defaultNamespace = defaultNamespace;
            _extraTypes = extraTypes;
            _rootElementName = rootElementName;

            Type iDictionaryType = typeof(T).GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (iDictionaryType != null)
            {
                var genericArguments = iDictionaryType.GetGenericArguments();
                this._keyType = genericArguments[0];
                this._valueType = genericArguments[1];
            }
            else // TODO: check for the specialized dictionary types where the key and/or value type is known
            {
                this._keyType = typeof(object);
                this._valueType = typeof(object);
            }

            _keySerializer = XmlSerializerFactory.Instance.GetSerializer(this._keyType, _defaultNamespace, _extraTypes, "Key");
            _valueSerializer = XmlSerializerFactory.Instance.GetSerializer(this._valueType, _defaultNamespace, _extraTypes, "Value");
        }

        public void Serialize(T instance, SerializationXmlTextWriter writer, XmlSerializerNamespaces namespaces)
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

            var enumerator = instance.GetEnumerator();
            while (enumerator.MoveNext())
            {
                writer.WriteStartElement("Item");

                if (enumerator.Key != null)
                {
                    _keySerializer.SerializeObject(enumerator.Key, writer, namespaces);
                }

                if (enumerator.Value != null)
                {
                    _valueSerializer.SerializeObject(enumerator.Value, writer, namespaces);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public void SerializeObject(object instance, SerializationXmlTextWriter writer, XmlSerializerNamespaces namespaces)
        {
            Serialize((T)instance, writer, namespaces);
        }

        public T Deserialize(XmlReader reader)
        {
            T dictionary = default(T);

            var hasInstanceBeenCreated = false;
            var isInsideItemElement = false;

            object currentKey = null;
            object currentValue = null;

            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _rootElementName)
                        {
                            dictionary = new T();
                            hasInstanceBeenCreated = true;
                        }
                        else if (reader.Name == "Item")
                        {
                            isInsideItemElement = true;
                        }
                        else if (reader.Name == "Key")
                        {
                            currentKey = DeserializeKeyOrValue(reader, this._keySerializer, hasInstanceBeenCreated, isInsideItemElement);
                        }
                        else if (reader.Name == "Value")
                        {
                            currentValue = DeserializeKeyOrValue(reader, this._valueSerializer, hasInstanceBeenCreated, isInsideItemElement);
                        }

                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "Item")
                        {
                            AddItem(dictionary, currentKey, currentValue);
                            currentKey = null;
                            currentValue = null;
                            isInsideItemElement = false;
                        }
                        else if (reader.Name == _rootElementName)
                        {
                            return CheckAndReturn(hasInstanceBeenCreated, dictionary);
                        }

                        break;
                }
            } while (reader.Read());

            throw new SerializationException("Poop in a hoop.");
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        private static object DeserializeKeyOrValue(XmlReader reader, IXmlSerializer serializer, bool hasInstanceBeenCreated, bool isInsideElement)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("le sigh.");
            }

            if (!isInsideElement)
            {
                throw new SerializationException("Really? REALLY?");
            }

            return serializer.DeserializeObject(reader);
        }

        private void AddItem(T dictionary, object key, object value)
        {
            if (key == null)
            {
                throw new SerializationException("No key - how am I supposed to get in???");
            }
            
            if (value == null && _valueType.IsValueType)
            {
                value = GetValueTypeDefault(_valueType);
            }

            dictionary.Add(key, value);
        }

        private static T CheckAndReturn(bool hasInstanceBeenCreated, T instance)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("Awwww, crap.");
            }

            return instance;
        }
    }
}