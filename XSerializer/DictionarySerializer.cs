using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal abstract class DictionarySerializer : IXmlSerializerInternal
    {
        private static readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

        protected readonly EncryptAttribute _encryptAttribute;
        private readonly IXmlSerializerOptions _options;

        private readonly IXmlSerializerInternal _keySerializer;
        private readonly IXmlSerializerInternal _valueSerializer;

        private readonly Func<object> _createDictionary;
        private readonly Func<object, object> _finalizeDictionary = x => x;

        protected DictionarySerializer(EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor

            _encryptAttribute = encryptAttribute;
            _options = options;
            _keySerializer = XmlSerializerFactory.Instance.GetSerializer(KeyType, null, _options.WithRootElementName("Key").WithRedactAttribute(null));
            _valueSerializer = XmlSerializerFactory.Instance.GetSerializer(ValueType, null, _options.WithRootElementName("Value"));

            if (DictionaryType.IsReadOnlyDictionary())
            {
                _createDictionary = DefaultDictionaryType.CreateDefaultConstructorFunc<object>();
                _finalizeDictionary = FinalizeCollectionIntoReadOnlyDictionary;
            }
            else if (DictionaryType.IsInterface || DictionaryType.IsAbstract)
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

        protected abstract object FinalizeCollectionIntoReadOnlyDictionary(object collection);

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

                var setIsEncryptionEnabledBackToFalse = writer.MaybeSetIsEncryptionEnabledToTrue(_encryptAttribute, options);

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

                if (setIsEncryptionEnabledBackToFalse)
                {
                    writer.IsEncryptionEnabled = false;
                }

                writer.WriteEndElement();
            }
        }

        public object DeserializeObject(XSerializerXmlReader reader, ISerializeOptions options)
        {
            object dictionary = null;

            var hasInstanceBeenCreated = false;
            var isInsideItemElement = false;

            object currentKey = null;
            object currentValue = null;
            bool shouldIssueRead;

            var setIsDecryptionEnabledBackToFalse = false;

            Func<bool> isAtRootElement;
            {
                var hasOpenedRootElement = false;
                
                isAtRootElement = () =>
                {
                    if (!hasOpenedRootElement && reader.Name == _options.RootElementName)
                    {
                        hasOpenedRootElement = true;
                        return true;
                    }

                    return false;
                };
            }

            do
            {
                shouldIssueRead = true;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (isAtRootElement())
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
                                setIsDecryptionEnabledBackToFalse = reader.MaybeSetIsDecryptionEnabledToTrue(_encryptAttribute, options);

                                dictionary = _createDictionary();
                                hasInstanceBeenCreated = true;

                                if (reader.IsEmptyElement)
                                {
                                    if (setIsDecryptionEnabledBackToFalse)
                                    {
                                        reader.IsDecryptionEnabled = false;
                                    }

                                    return _finalizeDictionary(dictionary);
                                }
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
                                currentKey = DeserializeKeyOrValue(reader, _keySerializer, options, out shouldIssueRead);
                            }
                            else if (reader.Name == "Value")
                            {
                                currentValue = DeserializeKeyOrValue(reader, _valueSerializer, options, out shouldIssueRead);
                            }
                        }

                        break;
                    case XmlNodeType.EndElement:
                        if (isInsideItemElement && reader.Name == "Item")
                        {
                            AddItemToDictionary(dictionary, currentKey, currentValue);
                            currentKey = null;
                            currentValue = null;
                            isInsideItemElement = false;
                        }
                        else if (reader.Name == _options.RootElementName)
                        {
                            if (setIsDecryptionEnabledBackToFalse)
                            {
                                reader.IsDecryptionEnabled = false;
                            }

                            return CheckAndReturn(hasInstanceBeenCreated, _finalizeDictionary(dictionary));
                        }

                        break;
                }
            } while (reader.ReadIfNeeded(shouldIssueRead));

            throw new InvalidOperationException("Deserialization error: reached the end of the document without returning a value.");
        }

        private static object DeserializeKeyOrValue(XSerializerXmlReader reader, IXmlSerializerInternal serializer, ISerializeOptions options, out bool shouldIssueRead)
        {
            var deserialized = serializer.DeserializeObject(reader, options);

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

        public static IXmlSerializerInternal GetSerializer(Type type, EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
        {
            return _serializerCache.GetOrAdd(
                XmlSerializerFactory.Instance.CreateKey(type, encryptAttribute, options),
                _ =>
                {
                    Func<Type, IXmlSerializerInternal> createDictionarySerializer =
                        genericDictionaryInterface =>
                        {
                            var genericArguments = genericDictionaryInterface.GetGenericArguments();
                            var keyType = genericArguments[0];
                            var valueType = genericArguments[1];
                            return (IXmlSerializerInternal)Activator.CreateInstance(typeof(DictionarySerializer<,,>).MakeGenericType(type, keyType, valueType), encryptAttribute, options);
                        };

                    if (type.IsAssignableToGenericIDictionary())
                    {
                        return createDictionarySerializer(type.GetGenericIDictionaryType());
                    }

                    if (type.IsAssignableToNonGenericIDictionary())
                    {
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(DictionarySerializer<>).MakeGenericType(type), encryptAttribute, options);
                    }

                    if (type.IsReadOnlyDictionary())
                    {
                        return createDictionarySerializer(type.GetIReadOnlyDictionaryInterface());
                    }

                    throw new InvalidOperationException(string.Format("Cannot create a DictionarySerializer of type '{0}'.", type.FullName));
                });
        }
    }

    internal class DictionarySerializer<TDictionary> : DictionarySerializer
        where TDictionary : IDictionary
    {
        public DictionarySerializer(EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
            : base(encryptAttribute, options)
        {
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

        protected override object FinalizeCollectionIntoReadOnlyDictionary(object collection)
        {
            throw new NotSupportedException();
        }
    }

    internal class DictionarySerializer<TDictionary, TKey, TValue> : DictionarySerializer
        where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Lazy<Func<object, object>> _finalizeCollectionIntoReadOnlyDictionary;
        
        public DictionarySerializer(EncryptAttribute encryptAttribute, IXmlSerializerOptions options)
            : base(encryptAttribute, options)
        {
            _finalizeCollectionIntoReadOnlyDictionary = new Lazy<Func<object, object>>(
                () =>
                {
                    var ctor =
                        Type.GetType(SerializationExtensions.ReadOnlyDictionary)
                            .MakeGenericType(KeyType, ValueType)
                            .GetConstructors()
                            .Single(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.IsGenericType && c.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                    var dictionaryParameter = Expression.Parameter(typeof(object), "dictionary");

                    var lambda =
                        Expression.Lambda<Func<object, object>>(
                            Expression.New(
                                ctor,
                                Expression.Convert(dictionaryParameter, typeof (IDictionary<TKey, TValue>))),
                            dictionaryParameter);

                    return lambda.Compile();
                });
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
            var iDictionary = dictionary as IDictionary<TKey, TValue>;

            if (iDictionary != null)
            {
                iDictionary.Add(
                    typeof(TKey).IsValueType && key == null ? default(TKey) : (TKey)key,
                    typeof(TValue).IsValueType && value == null ? default(TValue) : (TValue)value);
            }

        }

        protected override object FinalizeCollectionIntoReadOnlyDictionary(object dictionary)
        {
            return _finalizeCollectionIntoReadOnlyDictionary.Value(dictionary);
        }
    }
}