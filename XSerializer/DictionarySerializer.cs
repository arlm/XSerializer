using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public abstract class DictionarySerializer : IXmlSerializer
    {
        private static readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();

        private readonly IXmlSerializerOptions _options;

        private readonly IXmlSerializer _keySerializer;
        private readonly IXmlSerializer _valueSerializer;

        private readonly Func<object> _createDictionary;

        protected DictionarySerializer(IXmlSerializerOptions options)
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor

            _options = options;
            _keySerializer = XmlSerializerFactory.Instance.GetSerializer(KeyType, _options.WithRootElementName("Key"));
            _valueSerializer = XmlSerializerFactory.Instance.GetSerializer(ValueType, _options.WithRootElementName("Value"));

            if (DictionaryType.IsInterface || DictionaryType.IsAbstract)
            {
                if (DictionaryType.IsAssignableFrom(DefaultDictionaryType))
                {
                    _createDictionary = DefaultDictionaryType.CreateDefaultConstructorFunc<object>();
                }
                else
                {
                    var dictionaryInheritorType =
                        _options.ExtraTypes.FirstOrDefault(t =>
                            !t.IsInterface
                            && !t.IsAbstract
                            && DictionaryType.IsAssignableFrom(t)
                            && t.HasDefaultConstructor());
                    _createDictionary = dictionaryInheritorType.CreateDefaultConstructorFunc<object>();
                }
            }
            else if (DictionaryType.HasDefaultConstructor())
            {
                _createDictionary = DictionaryType.CreateDefaultConstructorFunc<object>();
            }
            else
            {
                throw new ArgumentException("Unable to find suitable dictionary to create.");
            }

            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        protected abstract Type DictionaryType { get; }
        protected abstract Type DefaultDictionaryType { get; }
        protected abstract Type KeyType { get; }
        protected abstract Type ValueType { get; }

        protected abstract IEnumerable<DictionaryEntry> GetDictionaryEntries(object dictionary);
        protected abstract void AddItemToDictionary(object dictionary, object key, object value);

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(_options.RootElementName);
            writer.WriteDefaultNamespaces();

            if (!string.IsNullOrWhiteSpace(_options.DefaultNamespace))
            {
                writer.WriteAttributeString("xmlns", null, null, _options.DefaultNamespace);
            }

            foreach (var item in GetDictionaryEntries(instance))
            {
                writer.WriteStartElement("Item");

                if (item.Key != null)
                {
                    _keySerializer.SerializeObject(writer, item.Key, namespaces, alwaysEmitTypes);
                }

                if (item.Value != null)
                {
                    _valueSerializer.SerializeObject(writer, item.Value, namespaces, alwaysEmitTypes);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public object DeserializeObject(XmlReader reader)
        {
            object dictionary = null;

            var hasInstanceBeenCreated = false;
            var isInsideItemElement = false;

            object currentKey = null;
            object currentValue = null;
            bool shouldIssueRead;

            do
            {
                shouldIssueRead = true;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _options.RootElementName)
                        {
                            dictionary = _createDictionary();
                            hasInstanceBeenCreated = true;
                        }
                        else if (reader.Name == "Item")
                        {
                            isInsideItemElement = true;
                        }
                        else if (reader.Name == "Key")
                        {
                            currentKey = DeserializeKeyOrValue(reader, _keySerializer, hasInstanceBeenCreated, isInsideItemElement, out shouldIssueRead);
                        }
                        else if (reader.Name == "Value")
                        {
                            currentValue = DeserializeKeyOrValue(reader, _valueSerializer, hasInstanceBeenCreated, isInsideItemElement, out shouldIssueRead);
                        }

                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "Item")
                        {
                            AddItemToDictionary(dictionary, currentKey, currentValue);
                            currentKey = null;
                            currentValue = null;
                            isInsideItemElement = false;
                        }
                        else if (reader.Name == _options.RootElementName)
                        {
                            return CheckAndReturn(hasInstanceBeenCreated, dictionary);
                        }

                        break;
                }
            } while (reader.ReadIfNeeded(shouldIssueRead));

            throw new SerializationException("Poop in a hoop.");
        }

        private static object DeserializeKeyOrValue(XmlReader reader, IXmlSerializer serializer, bool hasInstanceBeenCreated, bool isInsideElement, out bool shouldIssueRead)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("le sigh.");
            }

            if (!isInsideElement)
            {
                throw new SerializationException("Really? REALLY?");
            }

            var deserialized = serializer.DeserializeObject(reader);

            shouldIssueRead = !(serializer is DefaultSerializer);

            return deserialized;
        }

        private static object CheckAndReturn(bool hasInstanceBeenCreated, object instance)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("Awwww, crap.");
            }

            return instance;
        }

        public static IXmlSerializer GetSerializer(Type type, IXmlSerializerOptions options)
        {
            IXmlSerializer serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(type, options);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                if (type.IsAssignableToGenericIDictionary())
                {
                    var genericArguments = type.GetGenericIDictionaryType().GetGenericArguments();
                    var keyType = genericArguments[0];
                    var valueType = genericArguments[1];
                    serializer = (IXmlSerializer)Activator.CreateInstance(typeof(DictionarySerializer<,,>).MakeGenericType(type, keyType, valueType), options);
                }
                else if (type.IsAssignableToNonGenericIDictionary())
                {
                    serializer = (IXmlSerializer)Activator.CreateInstance(typeof(DictionarySerializer<>).MakeGenericType(type), options);
                }
                else
                {
                    throw new InvalidOperationException("Can't you do anything right?!");
                }
                
                _serializerCache[key] = serializer;
            }

            return serializer;
        }
    }

    public class DictionarySerializer<TDictionary> : DictionarySerializer, IXmlSerializer<TDictionary>
        where TDictionary : IDictionary
    {
        public DictionarySerializer(IXmlSerializerOptions options)
            : base(options)
        {
        }

        public void Serialize(SerializationXmlTextWriter writer, TDictionary instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            SerializeObject(writer, instance, namespaces, alwaysEmitTypes);
        }

        public TDictionary Deserialize(XmlReader reader)
        {
            return (TDictionary)DeserializeObject(reader);
        }

        protected override Type DictionaryType
        {
            get
            {
                return typeof(TDictionary);
            }
        }

        protected override Type DefaultDictionaryType
        {
            get
            {
                return typeof(Hashtable);
            }
        }

        protected override Type KeyType
        {
            get
            {
                return typeof(object);
            }
        }

        protected override Type ValueType
        {
            get
            {
                return typeof(object);
            }
        }

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries(object dictionary)
        {
            var enumerator = ((TDictionary)dictionary).GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Entry;
            }
        }

        protected override void AddItemToDictionary(object dictionary, object key, object value)
        {
            ((TDictionary)dictionary).Add(key, value);
        }
    }

    public class DictionarySerializer<TDictionary, TKey, TValue> : DictionarySerializer, IXmlSerializer<TDictionary>
        where TDictionary : IDictionary<TKey, TValue>
    {
        public DictionarySerializer(IXmlSerializerOptions options)
            : base(options)
        {
        }

        public void Serialize(SerializationXmlTextWriter writer, TDictionary instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            SerializeObject(writer, instance, namespaces, alwaysEmitTypes);
        }

        public TDictionary Deserialize(XmlReader reader)
        {
            return (TDictionary)DeserializeObject(reader);
        }

        protected override Type DictionaryType
        {
            get
            {
                return typeof(TDictionary);
            }
        }

        protected override Type DefaultDictionaryType
        {
            get
            {
                return typeof(Dictionary<TKey, TValue>);
            }
        }

        protected override Type KeyType
        {
            get
            {
                return typeof(TKey);
            }
        }

        protected override Type ValueType
        {
            get
            {
                return typeof(TValue);
            }
        }

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries(object dictionary)
        {
            return ((TDictionary)dictionary).Select(item => new DictionaryEntry(item.Key, item.Value));
        }

        protected override void AddItemToDictionary(object dictionary, object key, object value)
        {
            ((TDictionary)dictionary).Add(
                typeof(TKey).IsValueType && key == null ? default(TKey) : (TKey)key,
                typeof(TValue).IsValueType && value == null ? default(TValue) : (TValue)value);
        }
    }
}