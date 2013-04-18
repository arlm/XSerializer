using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public abstract class ListSerializer : IXmlSerializer
    {
        private static readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();

        private readonly string _defaultNamespace;
        private readonly Type[] _extraTypes;
        private readonly string _rootElementName;
        private readonly string _itemElementName;

        private readonly IXmlSerializer _itemSerializer;

        private readonly Func<object> _createCollection;
        private readonly Func<object, object> _finalizeCollection = x => x;

        protected ListSerializer(string defaultNamespace, Type[] extraTypes, string rootElementName, string itemElementName)
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor

            _defaultNamespace = defaultNamespace;
            _extraTypes = extraTypes;
            _rootElementName = rootElementName;
            _itemElementName = itemElementName;
            _itemSerializer = XmlSerializerFactory.Instance.GetSerializer(ItemType, _defaultNamespace, _extraTypes, string.IsNullOrEmpty(itemElementName) ? DefaultItemElementName : itemElementName);

            if (CollectionType.IsArray)
            {
                _createCollection = DefaultCollectionType.CreateDefaultConstructorFunc<object>();
                _finalizeCollection = FinalizeCollectionIntoArray;
            }
            else if (CollectionType.IsInterface || CollectionType.IsAbstract)
            {
                if (CollectionType.IsAssignableFrom(DefaultCollectionType))
                {
                    _createCollection = DefaultCollectionType.CreateDefaultConstructorFunc<object>();
                }
                else
                {
                    var collectionInheritorType =
                        extraTypes.FirstOrDefault(t =>
                            !t.IsInterface
                            && !t.IsAbstract
                            && CollectionType.IsAssignableFrom(t)
                            && t.HasDefaultConstructor());
                    _createCollection = collectionInheritorType.CreateDefaultConstructorFunc<object>();
                }
            }
            else if (CollectionType.HasDefaultConstructor())
            {
                _createCollection = CollectionType.CreateDefaultConstructorFunc<object>();
            }
            else
            {
                throw new ArgumentException("Unable to find suitable collection to create.");
            }
            
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }


        protected abstract Type CollectionType { get; }
        protected abstract Type DefaultCollectionType { get; }
        protected abstract Type ItemType { get; }
        protected abstract string DefaultItemElementName { get; }

        protected abstract void AddItemToCollection(object collection, object item);
        protected abstract object FinalizeCollectionIntoArray(object collection);

        public static IXmlSerializer GetSerializer(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName, string itemElementName)
        {
            IXmlSerializer serializer;
            var key = XmlSerializerFactory.Instance.CreateKey(type, defaultNamespace, extraTypes, rootElementName + "<>" + itemElementName);

            if (!_serializerCache.TryGetValue(key, out serializer))
            {
                if (type.IsAssignableToGenericIEnumerable())
                {
                    var itemType = type.GetGenericIEnumerableType().GetGenericArguments()[0];
                    serializer = (IXmlSerializer)Activator.CreateInstance(typeof(ListSerializer<,>).MakeGenericType(type, itemType), defaultNamespace, extraTypes, rootElementName, itemElementName);
                }
                else if (type.IsAssignableToNonGenericIEnumerable())
                {
                    serializer = (IXmlSerializer)Activator.CreateInstance(typeof(ListSerializer<>).MakeGenericType(type), defaultNamespace, extraTypes, rootElementName, itemElementName);
                }
                else
                {
                    throw new InvalidOperationException("You disappoint people a lot, don't you?");
                }

                _serializerCache[key] = serializer;
            }

            return serializer;
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            if (_rootElementName != null)
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(_rootElementName);
                writer.WriteDefaultNamespaces();

                if (!string.IsNullOrWhiteSpace(_defaultNamespace))
                {
                    writer.WriteAttributeString("xmlns", null, null, _defaultNamespace);
                }
            }
            
            foreach (var item in (IEnumerable)instance)
            {
                _itemSerializer.SerializeObject(writer, item, namespaces, alwaysEmitTypes);
            }

            if (_rootElementName != null)
            {
                writer.WriteEndElement();
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            object collection = null;

            var hasInstanceBeenCreated = false;
            
            bool shouldIssueRead;

            if (_rootElementName == null)
            {
                collection = _createCollection();
                hasInstanceBeenCreated = true;
            }

            do
            {
                shouldIssueRead = true;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (_rootElementName != null)
                        {
                            if (reader.Name == _rootElementName)
                            {
                                collection = _createCollection();
                                hasInstanceBeenCreated = true;
                                break;
                            }
                        }
                        else
                        {
                            // If there's no root element, and we encounter another element, we're done - get out!
                            if (reader.Name != _itemElementName)
                            {
                                return _finalizeCollection(CheckAndReturn(hasInstanceBeenCreated, collection));
                            }
                        }

                        if (reader.Name == _itemElementName)
                        {
                            var item = DeserializeItem(reader, _itemSerializer, hasInstanceBeenCreated, out shouldIssueRead);
                            AddItemToCollection(collection, item);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (_rootElementName != null)
                        {
                            if (reader.Name == _rootElementName)
                            {
                                return _finalizeCollection(CheckAndReturn(hasInstanceBeenCreated, collection));
                            }
                        }
                        else
                        {
                            if (reader.Name != _itemElementName)
                            {
                                return _finalizeCollection(CheckAndReturn(hasInstanceBeenCreated, collection));
                            }
                        }
                        break;
                }
            } while (reader.ReadIfNeeded(shouldIssueRead));

            throw new SerializationException("What? I don't even...");
        }

        private static object DeserializeItem(XmlReader reader, IXmlSerializer serializer, bool hasInstanceBeenCreated, out bool shouldIssueRead)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("le sigh.");
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
    }

    public sealed class ListSerializer<TEnumerable> : ListSerializer, IXmlSerializer<TEnumerable>
        where TEnumerable : IEnumerable
    {
        private readonly Action<object, object> _addItemToCollection;

        public ListSerializer(string defaultNamespace, Type[] extraTypes, string rootElementName, string itemElementName)
            : base(defaultNamespace, extraTypes, rootElementName, itemElementName)
        {
            if (typeof(IList).IsAssignableFrom(typeof(TEnumerable)))
            {
                _addItemToCollection = (collection, item) => ((IList)collection).Add(item);
            }
            else
            {
                var addMethods = new[] { typeof(TEnumerable) }.Concat(typeof(TEnumerable).GetInterfaces()).SelectMany(t => t.GetMethods().Where(m => m.Name == "Add" && m.GetParameters().Length == 1));
                var addFuncs = addMethods.Select(m => DynamicMethodFactory.CreateAction(m)).ToList();

                _addItemToCollection = (collection, item) =>
                {
                    var success = false;

                    foreach (var add in addFuncs)
                    {
                        try
                        {
                            add(collection, item);
                            success = true;
                            break;
                        }
                        catch (InvalidCastException)
                        {
                        }
                    }

                    if (!success)
                    {
                        if (item != null)
                        {
                            throw new InvalidOperationException(string.Format("No suitable 'Add' method found for instance of type {0}", item.GetType()));
                        }

                        throw new InvalidOperationException("No suitable 'Add' method found for null instance.");
                    }
                };
            }
        }

        public void Serialize(SerializationXmlTextWriter writer, TEnumerable instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            SerializeObject(writer, instance, namespaces, alwaysEmitTypes);
        }

        public TEnumerable Deserialize(XmlReader reader)
        {
            return (TEnumerable)DeserializeObject(reader);
        }

        protected override Type CollectionType
        {
            get { return typeof(TEnumerable); }
        }

        protected override Type DefaultCollectionType
        {
            get { return typeof(ArrayList); }
        }

        protected override Type ItemType
        {
            get { return typeof(object); }
        }

        protected override string DefaultItemElementName
        {
            get { return "Item"; }
        }

        protected override void AddItemToCollection(object collection, object item)
        {
            _addItemToCollection(collection, item);
        }

        protected override object FinalizeCollectionIntoArray(object collection)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class ListSerializer<TEnumerable, TItem> : ListSerializer, IXmlSerializer<TEnumerable>
        where TEnumerable : IEnumerable<TItem>
    {
        private readonly Action<object, object> _addItemToCollection;

        public ListSerializer(string defaultNamespace, Type[] extraTypes, string rootElementName, string itemElementName)
            : base(defaultNamespace, extraTypes, rootElementName, itemElementName)
        {
            if (typeof(IList).IsAssignableFrom(typeof(TEnumerable)))
            {
                _addItemToCollection = (collection, item) => ((IList)collection).Add(item);
            }
            else if (typeof(ICollection<>).MakeGenericType(typeof(TItem)).IsAssignableFrom(typeof(TEnumerable)))
            {
                var addMethod = typeof(ICollection<>).MakeGenericType(typeof(TItem)).GetMethod("Add", new[] { typeof(TItem) });
                var add = DynamicMethodFactory.CreateAction(addMethod);
                _addItemToCollection = (collection, item) => add(collection, item);
            }
            else
            {
                var addMethods = new[] { typeof(TEnumerable) }.Concat(typeof(TEnumerable).GetInterfaces()).SelectMany(t => t.GetMethods().Where(m => m.Name == "Add" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(TItem)));
                var addFuncs = addMethods.Select(m => DynamicMethodFactory.CreateAction(m)).ToList();

                _addItemToCollection = (collection, item) =>
                {
                    var success = false;

                    foreach (var add in addFuncs)
                    {
                        try
                        {
                            add(collection, item);
                            success = true;
                            break;
                        }
                        catch (InvalidCastException)
                        {
                        }
                    }

                    if (!success)
                    {
                        if (item != null)
                        {
                            throw new InvalidOperationException(string.Format("No suitable 'Add' method found for instance of type {0}", item.GetType()));
                        }

                        throw new InvalidOperationException("No suitable 'Add' method found for null instance.");
                    }
                };
            }
        }

        public void Serialize(SerializationXmlTextWriter writer, TEnumerable instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            SerializeObject(writer, instance, namespaces, alwaysEmitTypes);
        }

        public TEnumerable Deserialize(XmlReader reader)
        {
            return (TEnumerable)DeserializeObject(reader);
        }

        protected override Type CollectionType
        {
            get { return typeof(TEnumerable); }
        }

        protected override Type DefaultCollectionType
        {
            get { return typeof(List<TItem>); }
        }

        protected override Type ItemType
        {
            get { return typeof(TItem); }
        }

        protected override string DefaultItemElementName
        {
            get { return typeof(TItem).Name; }
        }

        protected override void AddItemToCollection(object collection, object item)
        {
            _addItemToCollection(collection, item);
        }

        protected override object FinalizeCollectionIntoArray(object collection)
        {
            return ((List<TItem>)collection).ToArray();
        }
    }
}