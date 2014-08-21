using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XSerializer
{
    internal abstract class DictionarySerializer : IXmlSerializerInternal
    {
        private static readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

        private readonly IXmlSerializerOptions _options;

        private readonly IXmlSerializerInternal _keySerializer;
        private readonly IXmlSerializerInternal _valueSerializer;

        private readonly Func<object> _createDictionary;

        protected DictionarySerializer(IXmlSerializerOptions options)
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor

            _options = options;
            _keySerializer = XmlSerializerFactory.Instance.GetSerializer(KeyType, _options.WithRootElementName("Key").WithRedactAttribute(null));
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

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            if (instance == null && !options.ShouldEmitNil)
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

            if (instance == null)
            {
                writer.WriteNilAttribute();
                writer.WriteEndElement();
                return;
            }

            foreach (var item in GetDictionaryEntries(instance))
            {
                writer.WriteStartElement("Item");

                if (item.Key != null)
                {
                    _keySerializer.SerializeObject(writer, item.Key, options);
                }

                if (item.Value != null)
                {
                    _valueSerializer.SerializeObject(writer, item.Value, options);
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
                            if (reader.IsNil())
                            {
                                if (reader.IsEmptyElement)
                                {
                                    return null;
                                }

                                dictionary = null;
                                hasInstanceBeenCreated = true;
                            }
                            else
                            {
                                dictionary = _createDictionary();
                                hasInstanceBeenCreated = true;
                            }
                        }
                        else if (reader.Name == "Item" && hasInstanceBeenCreated)
                        {
                            isInsideItemElement = true;
                        }
                        else if (isInsideItemElement)
                        {
                            if (reader.Name == "Key")
                            {
                                currentKey = DeserializeKeyOrValue(reader, _keySerializer, out shouldIssueRead);
                            }
                            else if (reader.Name == "Value")
                            {
                                currentValue = DeserializeKeyOrValue(reader, _valueSerializer, out shouldIssueRead);
                            }
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

            throw new InvalidOperationException("Deserialization error: reached the end of the document without returning a value.");
        }

        private static object DeserializeKeyOrValue(XmlReader reader, IXmlSerializerInternal serializer, out bool shouldIssueRead)
        {
            var deserialized = serializer.DeserializeObject(reader);

            shouldIssueRead = true;

            return deserialized;
        }

        private static object CheckAndReturn(bool hasInstanceBeenCreated, object instance)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new InvalidOperationException("Deserialization error: attempted to return a deserialized instance before it was created.");
            }

            return instance;
        }

        public static IXmlSerializerInternal GetSerializer(Type type, IXmlSerializerOptions options)
        {
            return _serializerCache.GetOrAdd(
                XmlSerializerFactory.Instance.CreateKey(type, options),
                _ =>
                {
                    if (type.IsAssignableToGenericIDictionary())
                    {
                        var genericArguments = type.GetGenericIDictionaryType().GetGenericArguments();
                        var keyType = genericArguments[0];
                        var valueType = genericArguments[1];
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(DictionarySerializer<,,>).MakeGenericType(type, keyType, valueType), options);
                    }

                    if (type.IsAssignableToNonGenericIDictionary())
                    {
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(DictionarySerializer<>).MakeGenericType(type), options);
                    }

                    throw new InvalidOperationException(string.Format("Cannot create a DictionarySerializer of type '{0}'.", type.FullName));
                });
        }
    }

    internal class DictionarySerializer<TDictionary> : DictionarySerializer, IXmlSerializerInternal<TDictionary>
        where TDictionary : IDictionary
    {
        public DictionarySerializer(IXmlSerializerOptions options)
            : base(options)
        {
        }

        public void Serialize(SerializationXmlTextWriter writer, TDictionary instance, ISerializeOptions options)
        {
            SerializeObject(writer, instance, options);
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
            if (dictionary != null)
            {
                ((TDictionary)dictionary).Add(key, value);
            }
        }
    }

    internal class DictionarySerializer<TDictionary, TKey, TValue> : DictionarySerializer, IXmlSerializerInternal<TDictionary>
        where TDictionary : IDictionary<TKey, TValue>
    {
        public DictionarySerializer(IXmlSerializerOptions options)
            : base(options)
        {
        }

        public void Serialize(SerializationXmlTextWriter writer, TDictionary instance, ISerializeOptions options)
        {
            SerializeObject(writer, instance, options);
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
            if (dictionary != null)
            {
                ((TDictionary)dictionary).Add(
                    typeof(TKey).IsValueType && key == null ? default(TKey) : (TKey)key,
                    typeof(TValue).IsValueType && value == null ? default(TValue) : (TValue)value);
            }
        }
    }
}