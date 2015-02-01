using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using XSerializer.Encryption;

namespace XSerializer
{
    internal sealed class SerializableProperty
    {
        private static readonly Func<object, IEnumerator> _getEnumeratorFunc = DynamicMethodFactory.CreateFunc<IEnumerator>(typeof(IEnumerable).GetMethod("GetEnumerator"));

        private readonly Lazy<IXmlSerializerInternal> _serializer;

        private readonly Func<object, object> _getValueFunc;
        private readonly Action<object, object> _setValueFunc;
        private readonly Func<object, bool> _shouldSerializeFunc;

        private Func<bool> _readsPastLastElement;

        public SerializableProperty(PropertyInfo propertyInfo, IXmlSerializerOptions options)
        {
            _getValueFunc = DynamicMethodFactory.CreateFunc<object>(propertyInfo.GetGetMethod());

            if (!propertyInfo.DeclaringType.IsAnonymous()
                && !(propertyInfo.IsReadOnlyProperty() && propertyInfo.PropertyType.IsGenericIEnumerable()))
            {
                if (propertyInfo.IsSerializableReadOnlyProperty())
                {
                    _setValueFunc = GetSerializableReadonlyPropertySetValueFunc(propertyInfo);
                }
                else
                {
                    if (IsListProperty(propertyInfo))
                    {
                        var setValue = DynamicMethodFactory.CreateAction(propertyInfo.GetSetMethod());
                        var addValues = GetAddEnumerableValuesAction(propertyInfo.PropertyType);

                        var targetType = propertyInfo.PropertyType;

                        _setValueFunc = (destinationInstance, sourceCollection) =>
                        {
                            sourceCollection = sourceCollection.ConvertIfNecessary(targetType);

                            if (_getValueFunc(destinationInstance) == null)
                            {
                                setValue(destinationInstance, sourceCollection);
                            }
                            else
                            {
                                addValues(destinationInstance, sourceCollection);
                            }
                        };
                    }
                    else
                    {
                        var setMethod = propertyInfo.GetSetMethod();
                        if (setMethod != null)
                        {
                            _setValueFunc = DynamicMethodFactory.CreateAction(setMethod);
                        }
                        else
                        {
                            _setValueFunc = null;
                        }
                    }
                }
            }

            _shouldSerializeFunc = GetShouldSerializeFunc(propertyInfo);
            _readsPastLastElement = () => false;
            _serializer = new Lazy<IXmlSerializerInternal>(GetCreateSerializerFunc(propertyInfo, options));
        }

        public string Name { get; private set; }

        public NodeType NodeType { get; private set; }

        public bool ReadsPastLastElement { get { return _readsPastLastElement(); } }

        public void ReadValue(XSerializerXmlReader reader, object instance, ISerializeOptions options)
        {
            SetValue(instance, ReadValue(reader, options));
        }

        public object ReadValue(XSerializerXmlReader reader, ISerializeOptions options)
        {
            return _serializer.Value.DeserializeObject(reader, options);
        }

        public void SetValue(object instance, object value)
        {
            if (_setValueFunc == null)
            {
                throw new InvalidOperationException("Cannot set the value of a read-only property.");
            }

            _setValueFunc(instance, value);
        }

        public void WriteValue(XSerializerXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            if (_shouldSerializeFunc(instance))
            {
                var value = _getValueFunc(instance);
                _serializer.Value.SerializeObject(writer, value, options);
            }
        }

        private Func<IXmlSerializerInternal> GetCreateSerializerFunc(PropertyInfo propertyInfo, IXmlSerializerOptions options)
        {
            var redactAttribute = (RedactAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(RedactAttribute));
            if (redactAttribute == null)
            {
                redactAttribute = options.RedactAttribute;
            }

            var encryptAttribute = (EncryptAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(EncryptAttribute));
            if (encryptAttribute == null)
            {
                encryptAttribute = options.EncryptAttribute;
            }

            var attributeAttribute = (XmlAttributeAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlAttributeAttribute));
            if (attributeAttribute != null)
            {
                var attributeName = !string.IsNullOrWhiteSpace(attributeAttribute.AttributeName) ? attributeAttribute.AttributeName : propertyInfo.Name;
                NodeType = NodeType.Attribute;
                Name = attributeName;
                return () => new XmlAttributeSerializer(propertyInfo.PropertyType, attributeName, redactAttribute, encryptAttribute, options);
            }

            var textAttribute = (XmlTextAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlTextAttribute));
            if (textAttribute != null)
            {
                NodeType = NodeType.Text;
                Name = propertyInfo.Name;
                return () => new XmlTextSerializer(propertyInfo.PropertyType, redactAttribute, encryptAttribute, options.ExtraTypes);
            }

            if (redactAttribute != null)
            {
                options = options.WithRedactAttribute(redactAttribute);
            }
            
            if (encryptAttribute != null)
            {
                options = options.WithEncryptAttribute(encryptAttribute);
            }
            
            var elementAttribute = (XmlElementAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlElementAttribute), false);
            string rootElementName;

            if (IsListProperty(propertyInfo))
            {
                var arrayAttribute = (XmlArrayAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlArrayAttribute), false);
                var arrayItemAttribute = (XmlArrayItemAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlArrayItemAttribute), false);

                if (elementAttribute != null && (arrayAttribute != null || arrayItemAttribute != null))
                {
                    throw new InvalidOperationException("On list types, XmlElementAttribute may not be present with either XmlArrayAttribute or XmlArrayItemAttribute are present.");
                }

                string itemElementName;

                if (elementAttribute != null)
                {
                    rootElementName = null;
                    itemElementName = GetElementName(elementAttribute, x => x.ElementName, propertyInfo.Name);

                    Name = itemElementName;

                    _readsPastLastElement = () => true;
                }
                else
                {
                    rootElementName = GetElementName(arrayAttribute, x => x.ElementName, propertyInfo.Name);

                    var itemElementNameFallback =
                        propertyInfo.PropertyType.IsAssignableToGenericIEnumerable()
                            ? propertyInfo.PropertyType.GetGenericIEnumerableType().GetGenericArguments()[0].GetElementName()
                            : "Item";

                    itemElementName = GetElementName(arrayItemAttribute, x => x.ElementName, itemElementNameFallback);

                    Name = rootElementName;
                }

                NodeType = NodeType.Element;
                return () => ListSerializer.GetSerializer(propertyInfo.PropertyType, options.WithRootElementName(rootElementName), itemElementName);
            }

            rootElementName = GetElementName(elementAttribute, x => x.ElementName, propertyInfo.Name);

            NodeType = NodeType.Element;
            Name = rootElementName;
            return () => XmlSerializerFactory.Instance.GetSerializer(propertyInfo.PropertyType, options.WithRootElementName(rootElementName));
        }

        private static bool IsListProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType != typeof(string)
                   && !propertyInfo.PropertyType.IsAssignableToNonGenericIDictionary()
                   && !propertyInfo.PropertyType.IsAssignableToGenericIDictionary()
                   && !propertyInfo.PropertyType.IsReadOnlyDictionary()
                   && (propertyInfo.PropertyType.IsAssignableToNonGenericIEnumerable()
                       || propertyInfo.PropertyType.IsAssignableToGenericIEnumerable());
        }

        private static string GetElementName<TAttribute>(TAttribute attribute, Func<TAttribute, string> getNameFromAttribute, string fallbackName)
            where TAttribute : Attribute
        {
            if (attribute != null && !string.IsNullOrWhiteSpace(getNameFromAttribute(attribute)))
            {
                return getNameFromAttribute(attribute);
            }

            return fallbackName;
        }

        private Action<object, object> GetSerializableReadonlyPropertySetValueFunc(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            if (propertyType.IsAssignableToNonGenericIDictionary())
            {
                return (destinationInstance, sourceDictionary) =>
                {
                    var destinationDictionary = (IDictionary)_getValueFunc(destinationInstance);
                    foreach (DictionaryEntry sourceDictionaryEntry in (IDictionary)sourceDictionary)
                    {
                        destinationDictionary.Add(sourceDictionaryEntry.Key, sourceDictionaryEntry.Value);
                    }
                };
            }

            if (propertyType.IsAssignableToGenericIDictionary())
            {
                var genericIDictionaryType = propertyType.GetGenericIDictionaryType();

                var addMethod = genericIDictionaryType.GetMethod("Add", genericIDictionaryType.GetGenericArguments());
                var addToDictionary = DynamicMethodFactory.CreateTwoArgAction(addMethod);

                var keyValuePairType = typeof(KeyValuePair<,>).MakeGenericType(genericIDictionaryType.GetGenericArguments());
                var getKeyFunc = DynamicMethodFactory.CreateGetPropertyValueFunc(keyValuePairType, "Key");
                var getValueFunc = DynamicMethodFactory.CreateGetPropertyValueFunc(keyValuePairType, "Value");

                return (destinationInstance, sourceDictionary) =>
                {
                    var destinationDictionary = _getValueFunc(destinationInstance);
                    var sourceEnumerator = _getEnumeratorFunc(sourceDictionary);
                    while (sourceEnumerator.MoveNext())
                    {
                        addToDictionary(destinationDictionary, getKeyFunc(sourceEnumerator.Current), getValueFunc(sourceEnumerator.Current));
                    }
                };
            }

            var addEnumerableValuesAction = GetAddEnumerableValuesAction(propertyType);

            if (addEnumerableValuesAction == null)
            {
                throw new InvalidOperationException("Unknown property type - cannot determine the 'SetValueFunc'.");
            }

            return addEnumerableValuesAction;
        }

        private Action<object, object> GetAddEnumerableValuesAction(Type propertyType)
        {
            if (typeof(IList).IsAssignableFrom(propertyType))
            {
                return (destinationInstance, sourceCollection) =>
                {
                    var destinationCollection = (IList)_getValueFunc(destinationInstance);
                    foreach (var sourceItem in (IList)sourceCollection)
                    {
                        destinationCollection.Add(sourceItem);
                    }
                };
            }

            if (propertyType.IsAssignableToGenericIEnumerable())
            {
                var itemType = propertyType.GetGenericIEnumerableType().GetGenericArguments()[0];

                if (typeof(ICollection<>).MakeGenericType(itemType).IsAssignableFrom(propertyType))
                {
                    var addMethod = typeof(ICollection<>).MakeGenericType(itemType).GetMethod("Add", new[] { itemType });
                    var addToCollection = DynamicMethodFactory.CreateAction(addMethod);

                    return (destinationInstance, sourceCollection) =>
                    {
                        var destinationCollection = _getValueFunc(destinationInstance);
                        var sourceEnumerator = _getEnumeratorFunc(sourceCollection);

                        while (sourceEnumerator.MoveNext())
                        {
                            addToCollection(destinationCollection, sourceEnumerator.Current);
                        }
                    };
                }

                if (propertyType.HasAddMethodOfType(itemType))
                {
                    var addMethods = new[] { propertyType }.Concat(propertyType.GetInterfaces()).SelectMany(t => t.GetMethods().Where(m => m.Name == "Add" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == itemType));
                    var addToCollectionFuncs = addMethods.Select(m => DynamicMethodFactory.CreateAction(m)).ToList();

                    return (destinationInstance, sourceCollection) =>
                    {
                        var destinationCollection = _getValueFunc(destinationInstance);
                        var sourceEnumerator = _getEnumeratorFunc(sourceCollection);

                        while (sourceEnumerator.MoveNext())
                        {
                            var success = false;

                            foreach (var addToCollection in addToCollectionFuncs)
                            {
                                try
                                {
                                    addToCollection(destinationCollection, sourceEnumerator.Current);
                                    success = true;
                                    break;
                                }
                                catch (InvalidCastException)
                                {
                                }
                            }

                            if (!success)
                            {
                                if (sourceEnumerator.Current != null)
                                {
                                    throw new InvalidOperationException(string.Format("No suitable 'Add' method found for instance of type {0}", sourceEnumerator.Current.GetType()));
                                }

                                throw new InvalidOperationException("No suitable 'Add' method found for null instance.");
                            }
                        }
                    };
                }
            }

            if (propertyType.IsAssignableToNonGenericIEnumerable())
            {
                var addMethods = new[] { propertyType }.Concat(propertyType.GetInterfaces()).SelectMany(t => t.GetMethods().Where(m => m.Name == "Add" && m.GetParameters().Length == 1));
                var addToCollectionFuncs = addMethods.Select(m => DynamicMethodFactory.CreateAction(m)).ToList();

                return (destinationInstance, sourceCollection) =>
                {
                    var destinationCollection = _getValueFunc(destinationInstance);
                    var sourceEnumerator = _getEnumeratorFunc(sourceCollection);

                    while (sourceEnumerator.MoveNext())
                    {
                        var success = false;

                        foreach (var addToCollection in addToCollectionFuncs)
                        {
                            try
                            {
                                addToCollection(destinationCollection, sourceEnumerator.Current);
                                success = true;
                                break;
                            }
                            catch (InvalidCastException)
                            {
                            }
                        }

                        if (!success)
                        {
                            if (sourceEnumerator.Current != null)
                            {
                                throw new InvalidOperationException(string.Format("No suitable 'Add' method found for instance of type {0}", sourceEnumerator.Current.GetType()));
                            }

                            throw new InvalidOperationException("No suitable 'Add' method found for null instance.");
                        }
                    }
                };
            }

            return null;
        }

        private Func<object, bool> GetShouldSerializeFunc(PropertyInfo propertyInfo)
        {
            Func<object, bool> specifiedFunc = null;
            var specifiedProperty = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name + "Specified");
            if (specifiedProperty != null && specifiedProperty.CanRead)
            {
                specifiedFunc = DynamicMethodFactory.CreateFunc<bool>(specifiedProperty.GetGetMethod());
            }

            Func<object, bool> shouldSerializeFunc = null;
            var shouldSerializeMethod = propertyInfo.DeclaringType.GetMethod("ShouldSerialize" + propertyInfo.Name, Type.EmptyTypes);
            if (shouldSerializeMethod != null)
            {
                shouldSerializeFunc = DynamicMethodFactory.CreateFunc<bool>(shouldSerializeMethod);
            }

            if (specifiedFunc == null && shouldSerializeFunc == null)
            {
                return instance => true;
            }

            if (specifiedFunc != null && shouldSerializeFunc == null)
            {
                return specifiedFunc;
            }

            if (specifiedFunc == null)
            {
                return shouldSerializeFunc;
            }

            return instance => specifiedFunc(instance) && shouldSerializeFunc(instance);
        }
    }
}