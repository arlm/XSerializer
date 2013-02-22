using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public sealed class SerializableProperty
    {
        private readonly Func<object, object> _getValueFunc;
        private readonly Action<object, object> _setValueFunc;
        private readonly Func<object, bool> _shouldSerializeFunc;

        private readonly Lazy<IXmlSerializer> _serializer;

        public SerializableProperty(PropertyInfo propertyInfo, string defaultNamespace, Type[] extraTypes)
        {
            _getValueFunc = DynamicMethodFactory.CreateGetMethod<object>(propertyInfo.GetGetMethod());
            _setValueFunc = DynamicMethodFactory.CreateSetMethod(propertyInfo.GetSetMethod());
            _shouldSerializeFunc = GetShouldSerializeFunc(propertyInfo);

            var attributeAttribute = (XmlAttributeAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlAttributeAttribute));
            if (attributeAttribute != null)
            {
                var attributeName = !string.IsNullOrWhiteSpace(attributeAttribute.AttributeName) ? attributeAttribute.AttributeName : propertyInfo.Name;
                NodeType = NodeType.Attribute;
                Name = attributeName;
                _serializer = new Lazy<IXmlSerializer>(() => new XmlAttributeSerializer(attributeName, propertyInfo.PropertyType));
            }
            else
            {
                var textAttribute = (XmlTextAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(XmlTextAttribute));
                if (textAttribute != null)
                {
                    NodeType = NodeType.Text;
                    Name = propertyInfo.Name;
                    _serializer = new Lazy<IXmlSerializer>(() => new XmlTextSerializer(propertyInfo.PropertyType));
                }
                else
                {
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
                    _serializer =
                        new Lazy<IXmlSerializer>(
                            () => XmlSerializerFactory.Instance.GetSerializer(propertyInfo.PropertyType, defaultNamespace, extraTypes, rootElementName));
                            //() => InterfaceSerializer.GetSerializer(propertyInfo.PropertyType, defaultNamespace, extraTypes, rootElementName));
                }
            }
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

        public IXmlSerializer Serializer
        {
            get { return _serializer.Value; }
        }

        public string Name { get; private set; }

        public NodeType NodeType { get; private set; }

        public object GetValue(object instance)
        {
            return _getValueFunc(instance);
        }

        public void SetValue(object instance, object value)
        {
            _setValueFunc(instance, value);
        }

        public void ReadValue(XmlReader reader, object instance)
        {
            SetValue(instance, Serializer.DeserializeObject(reader));
        }

        public bool ShouldSerialize(object instance)
        {
            return _shouldSerializeFunc(instance);
        }

        public void WriteValue(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces)
        {
            if (ShouldSerialize(instance))
            {
                var value = GetValue(instance);
                if (value != null)
                {
                    Serializer.SerializeObject(value, writer, namespaces);
                }
            }
        }
    }
}