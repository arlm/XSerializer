using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    using System.Collections;

    public sealed class SerializableProperty
    {
        private readonly Lazy<IXmlSerializer> _serializer;

        private readonly Func<object, object> _getValueFunc;
        private readonly Action<object, object> _setValueFunc;
        private readonly Func<object, bool> _shouldSerializeFunc;

        public SerializableProperty(PropertyInfo propertyInfo, string defaultNamespace, Type[] extraTypes)
        {
            _getValueFunc = DynamicMethodFactory.CreateGetMethod<object>(propertyInfo.GetGetMethod());
            _setValueFunc =
                propertyInfo.IsSerializableReadOnlyProperty()
                    ? GetSerializableReadonlyPropertySetValueFunc(propertyInfo)
                    : DynamicMethodFactory.CreateSetMethod(propertyInfo.GetSetMethod());
            _shouldSerializeFunc = GetShouldSerializeFunc(propertyInfo);
            _serializer = new Lazy<IXmlSerializer>(GetCreateSerializerFunc(propertyInfo, defaultNamespace, extraTypes));
        }

        public string Name { get; private set; }

        public NodeType NodeType { get; private set; }

        public void ReadValue(XmlReader reader, object instance)
        {
            _setValueFunc(instance, _serializer.Value.DeserializeObject(reader));
        }

        public void WriteValue(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces)
        {
            if (_shouldSerializeFunc(instance))
            {
                var value = _getValueFunc(instance);
                if (value != null)
                {
                    _serializer.Value.SerializeObject(value, writer, namespaces);
                }
            }
        }

        private Func<IXmlSerializer> GetCreateSerializerFunc(PropertyInfo propertyInfo, string defaultNamespace, Type[] extraTypes)
        {
            var attributeAttribute = (XmlAttributeAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlAttributeAttribute));
            if (attributeAttribute != null)
            {
                var attributeName = !string.IsNullOrWhiteSpace(attributeAttribute.AttributeName) ? attributeAttribute.AttributeName : propertyInfo.Name;
                NodeType = NodeType.Attribute;
                Name = attributeName;
                return () => new XmlAttributeSerializer(attributeName, propertyInfo.PropertyType);
            }

            var textAttribute = (XmlTextAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlTextAttribute));
            if (textAttribute != null)
            {
                NodeType = NodeType.Text;
                Name = propertyInfo.Name;
                return () => new XmlTextSerializer(propertyInfo.PropertyType);
            }

            var elementAttribute = (XmlElementAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlElementAttribute));

            string rootElementName;
            if (elementAttribute != null && !string.IsNullOrWhiteSpace(elementAttribute.ElementName))
            {
                rootElementName = elementAttribute.ElementName;
            }
            else
            {
                rootElementName = propertyInfo.Name;
            }

            NodeType = NodeType.Element;
            Name = rootElementName;
            return () => XmlSerializerFactory.Instance.GetSerializer(propertyInfo.PropertyType, defaultNamespace, extraTypes, rootElementName);
        }

        private Action<object, object> GetSerializableReadonlyPropertySetValueFunc(PropertyInfo propertyInfo)
        {
            if (typeof(IDictionary).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return (instance, value) =>
                    {
                        var instanceDictionary = (IDictionary)_getValueFunc(instance);
                        var valueEnumerator = ((IDictionary)value).GetEnumerator();
                        while (valueEnumerator.MoveNext())
                        {
                            instanceDictionary.Add(valueEnumerator.Key, valueEnumerator.Value);
                        }
                    };
            }

            return (instance, value) => { };
        }

        private Func<object, bool> GetShouldSerializeFunc(PropertyInfo propertyInfo)
        {
            var xmlIgnoreAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(XmlIgnoreAttribute));
            if (xmlIgnoreAttribute != null)
            {
                return instance => false;
            }

            Func<object, bool> specifiedFunc = null;
            var specifiedProperty = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name + "Specified");
            if (specifiedProperty != null && specifiedProperty.CanRead)
            {
                specifiedFunc = DynamicMethodFactory.CreateGetMethod<bool>(specifiedProperty.GetGetMethod());
            }

            Func<object, bool> shouldSerializeFunc = null;
            var shouldSerializeMethod = propertyInfo.DeclaringType.GetMethod("ShouldSerialize" + propertyInfo.Name, Type.EmptyTypes);
            if (shouldSerializeMethod != null)
            {
                shouldSerializeFunc = DynamicMethodFactory.CreateGetMethod<bool>(shouldSerializeMethod);
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