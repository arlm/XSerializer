using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace XSerializer
{
    internal abstract class ListSerializer : IXmlSerializerInternal
    {
        private static readonly ConcurrentDictionary<int, IXmlSerializerInternal> _serializerCache = new ConcurrentDictionary<int, IXmlSerializerInternal>();

        private readonly IXmlSerializerOptions _options;
        private readonly string _itemElementName;

        private readonly IXmlSerializerInternal _itemSerializer;

        private readonly Func<object> _createCollection;

        protected ListSerializer(IXmlSerializerOptions options, string itemElementName)                                                             // ReSharper disable DoNotCallOverridableMethodsInConstructor
        {
            _options = options;
            _itemElementName = string.IsNullOrEmpty(itemElementName) ? DefaultItemElementName : itemElementName;
            _itemSerializer = XmlSerializerFactory.Instance.GetSerializer(ItemType, _options.WithRootElementName(_itemElementName).AlwaysEmitNil());

            if (CollectionType.IsArray)
            {
                _createCollection = DefaultCollectionType.CreateDefaultConstructorFunc<object>();
            }
            else if (CollectionType.IsReadOnlyCollection())
            {
                _createCollection = DefaultCollectionType.CreateDefaultConstructorFunc<object>();
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
                        _options.ExtraTypes.FirstOrDefault(t =>
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
        }                                                                                                                                           // ReSharper restore DoNotCallOverridableMethodsInConstructor

        protected abstract Type CollectionType { get; }
        protected abstract Type DefaultCollectionType { get; }
        protected abstract Type ItemType { get; }
        protected abstract string DefaultItemElementName { get; }

        protected abstract void AddItemToCollection(object collection, object item);

        public static IXmlSerializerInternal GetSerializer(Type type, IXmlSerializerOptions options, string itemElementName)
        {
            return _serializerCache.GetOrAdd(
                XmlSerializerFactory.Instance.CreateKey(type, options.WithRootElementName(options.RootElementName + "<>" + itemElementName)),
                _ =>
                {
                    if (type.IsAssignableToGenericIEnumerable())
                    {
                        var itemType = type.GetGenericIEnumerableType().GetGenericArguments()[0];
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(ListSerializer<,>).MakeGenericType(type, itemType), options, itemElementName);
                    }
                        
                    if (type.IsAssignableToNonGenericIEnumerable())
                    {
                        return (IXmlSerializerInternal)Activator.CreateInstance(typeof(ListSerializer<>).MakeGenericType(type), options, itemElementName);
                    }

                    throw new InvalidOperationException(string.Format("Cannot create a ListSerializer of type '{0}'.", type.FullName));
                });
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            if (instance == null && !options.ShouldEmitNil)
            {
                return;
            }

            if (_options.RootElementName != null)
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(_options.RootElementName);
                writer.WriteDefaultDocumentNamespaces();
                writer.WriteDefaultNamespace(_options.DefaultNamespace).Dispose();
            }

            if (instance == null)
            {
                writer.WriteNilAttribute();
            }
            else
            {
                foreach (var item in (IEnumerable)instance)
                {
                    _itemSerializer.SerializeObject(writer, item, options);
                }
            }

            if (_options.RootElementName != null)
            {
                writer.WriteEndElement();
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            object collection = null;

            var hasInstanceBeenCreated = false;

            bool shouldIssueRead;

            if (_options.RootElementName == null)
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
                        if (_options.RootElementName != null)
                        {
                            if (reader.Name == _options.RootElementName)
                            {
                                if (reader.IsNil())
                                {
                                    if (reader.IsEmptyElement)
                                    {
                                        return null;
                                    }

                                    collection = null;
                                    hasInstanceBeenCreated = true;
                                }
                                else
                                {
                                    collection = _createCollection();
                                    hasInstanceBeenCreated = true;

                                    if (reader.IsEmptyElement)
                                    {
                                        return collection;
                                    }
                                }

                                break;
                            }
                        }
                        else
                        {
                            // If there's no root element, and we encounter another element, we're done - get out!
                            if (reader.Name != _itemElementName)
                            {
                                return
                                    collection == null
                                        ? null
                                        : CheckAndReturn(hasInstanceBeenCreated, collection);
                            }
                        }

                        if (reader.Name == _itemElementName)
                        {
                            var item = DeserializeItem(reader, _itemSerializer, hasInstanceBeenCreated, out shouldIssueRead);

                            if (collection != null)
                            {
                                AddItemToCollection(collection, item);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (_options.RootElementName != null)
                        {
                            if (reader.Name == _options.RootElementName)
                            {
                                return
                                    collection == null
                                        ? null
                                        : CheckAndReturn(hasInstanceBeenCreated, collection);
                            }
                        }
                        else
                        {
                            if (reader.Name != _itemElementName)
                            {
                                return
                                    collection == null
                                        ? null
                                        : CheckAndReturn(hasInstanceBeenCreated, collection);
                            }
                        }
                        break;
                }
            } while (reader.ReadIfNeeded(shouldIssueRead));

            throw new InvalidOperationException("Deserialization error: attempted to return a deserialized instance before it was created.");
        }

        private static object DeserializeItem(XmlReader reader, IXmlSerializerInternal serializer, bool hasInstanceBeenCreated, out bool shouldIssueRead)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new InvalidOperationException("Deserialization error: attempted to deserialize an item before creating its list.");
            }

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
    }

    internal sealed class ListSerializer<TEnumerable> : ListSerializer
        where TEnumerable : IEnumerable
    {
        private readonly Action<object, object> _addItemToCollection;

        public ListSerializer(IXmlSerializerOptions options, string itemElementName)
            : base(options, itemElementName)
        {
            if (typeof(IList).IsAssignableFrom(typeof(TEnumerable)))
            {
                _addItemToCollection = (collection, item) => ((IList)collection).Add(item);
            }
            else
            {
                var addMethods =
                    typeof(TEnumerable).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Concat(typeof(TEnumerable).GetInterfaces().SelectMany(i => i.GetMethods()))
                    .Where(m => m.Name == "Add" && m.GetParameters().Length == 1);

                var addFuncs = addMethods.Select(DynamicMethodFactory.CreateAction).ToList();

                if (addFuncs.Count == 0)
                {
                    throw new InvalidOperationException(string.Format("No suitable 'Add' method found for type '{0}'.", typeof(TEnumerable).FullName));
                }

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
                            throw new InvalidOperationException(string.Format("No suitable 'Add' method found for instance of type {0}.", item.GetType()));
                        }

                        throw new InvalidOperationException("No suitable 'Add' method found for null instance.");
                    }
                };
            }
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
    }

    internal sealed class ListSerializer<TEnumerable, TItem> : ListSerializer
        where TEnumerable : IEnumerable<TItem>
    {
        private readonly Action<object, object> _addItemToCollection;

        public ListSerializer(IXmlSerializerOptions options, string itemElementName)
            : base(options, itemElementName)
        {
            if (typeof(IList).IsAssignableFrom(typeof(TEnumerable)))
            {
                _addItemToCollection = (collection, item) => ((IList)collection).Add(item);
            }
            else
            {
                var addMethods =
                    typeof(TEnumerable).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Concat(typeof(TEnumerable).GetInterfaces().SelectMany(i => i.GetMethods()))
                    .Where(m => m.Name == "Add"
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(TItem)));

                var addFuncs = addMethods.Select(DynamicMethodFactory.CreateAction).ToList();

                if (addFuncs.Count == 0)
                {
                    if (typeof(TEnumerable) == typeof(IEnumerable<>).MakeGenericType(typeof(TItem)))
                    {
                        addFuncs.Add((collection, item) => ((IList)collection).Add(item));
                    }
                    else if (typeof(TEnumerable).IsReadOnlyCollection())
                    {
                        addFuncs.Add((collection, item) => ((IList)collection).Add(item));
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("No suitable 'Add' method found for type '{0}'.", typeof(TEnumerable).FullName));
                    }
                }

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
                            throw new InvalidOperationException(string.Format("No suitable 'Add' method found for instance of type {0}.", item.GetType()));
                        }

                        throw new InvalidOperationException("No suitable 'Add' method found for null instance.");
                    }
                };
            }
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
    }
}