using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class SerializableProperty
    {
        protected readonly PropertyInfo _propertyInfo;
        private readonly PropertyInvoker _propertyInvoker;
        private readonly Lazy<IXmlSerializer> _serializer; 

        public SerializableProperty(PropertyInfo propertyInfo, string defaultNamespace, Type[] extraTypes)
        {
            _propertyInfo = propertyInfo;
            _propertyInvoker = new PropertyInvoker(propertyInfo);

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

        public IXmlSerializer Serializer
        {
            get { return _serializer.Value; }
        }

        public string Name { get; private set; }

        public NodeType NodeType { get; private set; }

        public object GetValue(object instance)
        {
            return _propertyInvoker.GetValue(instance);
        }

        public void SetValue(object instance, object value)
        {
            _propertyInvoker.SetValue(instance, value);
        }

        public void ReadValue(XmlReader reader, object instance)
        {
            SetValue(instance, Serializer.DeserializeObject(reader));
        }

        public void WriteValue(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces)
        {
            Serializer.SerializeObject(GetValue(instance), writer, namespaces);
        }
    }
}