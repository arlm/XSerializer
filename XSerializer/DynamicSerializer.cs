using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class DynamicSerializer : IXmlSerializer<object>
    {
        private readonly string _defaultNamespace;
        private readonly Type[] _extraTypes;
        private readonly string _rootElementName;

        public static IXmlSerializer<T> GetSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            var serializer = new DynamicSerializer(defaultNamespace, extraTypes, rootElementName);

            if (typeof(T) == typeof(object))
            {
                return (IXmlSerializer<T>)serializer;
            }
            else if (typeof(T) == typeof(ExpandoObject))
            {
                return (IXmlSerializer<T>)new DynamicSerializerExpandoObjectProxy(serializer);
            }
            else
            {
                throw new InvalidOperationException("The only valid generic arguments for DynamicSerializer.GetSerializer<T> are object, dynamic, and ExpandoObject");
            }
        }

        public DynamicSerializer(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            _defaultNamespace = defaultNamespace;
            _extraTypes = extraTypes;
            _rootElementName = rootElementName;
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            Serialize(writer, instance, namespaces, alwaysEmitTypes);
        }

        public void Serialize(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            if (instance == null)
            {
                return;
            }

            var expando = instance as ExpandoObject;
            if (expando != null)
            {
                SerializeExpandoObject(writer, expando, namespaces, alwaysEmitTypes);
                return;
            }

            IXmlSerializer serializer;

            if (!alwaysEmitTypes || instance.IsAnonymous())
            {
                serializer = CustomSerializer.GetSerializer(instance.GetType(), _defaultNamespace, _extraTypes, _rootElementName);
            }
            else
            {
                serializer = CustomSerializer.GetSerializer(typeof(object), _defaultNamespace, (_extraTypes ?? new Type[0]).Concat(new[] { instance.GetType() }).Distinct().ToArray(), _rootElementName);
            }

            serializer.SerializeObject(writer, instance, namespaces, alwaysEmitTypes);
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        public object Deserialize(XmlReader reader)
        {
            var type = reader.GetXsdType<object>(_extraTypes);
            if (type != null)
            {
                var serializer = XmlSerializerFactory.Instance.GetSerializer(type, _defaultNamespace, _extraTypes, reader.Name);
                return serializer.DeserializeObject(reader);
            }

            return DeserializeExpandoObject(reader);
        }

        private void SerializeExpandoObject(SerializationXmlTextWriter writer, IDictionary<string, object> expando, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(_rootElementName);
            writer.WriteDefaultNamespaces();

            if (!string.IsNullOrWhiteSpace(_defaultNamespace))
            {
                writer.WriteAttributeString("xmlns", null, null, _defaultNamespace);
            }

            foreach (var property in expando)
            {
                if (property.Value == null)
                {
                    continue;
                }

                IXmlSerializer serializer;

                if (property.Value is ExpandoObject)
                {
                    serializer = DynamicSerializer.GetSerializer<ExpandoObject>(_defaultNamespace, _extraTypes, property.Key);
                }
                else
                {
                    serializer = CustomSerializer.GetSerializer(property.Value.GetType(), _defaultNamespace, _extraTypes, property.Key);
                }

                serializer.SerializeObject(writer, property.Value, namespaces, alwaysEmitTypes);
            }

            writer.WriteEndElement();
        }

        private ExpandoObject DeserializeExpandoObject(XmlReader reader)
        {
            // TODO: implement
            return null;
        }

        private class DynamicSerializerExpandoObjectProxy : IXmlSerializer<ExpandoObject>
        {
            private readonly DynamicSerializer _serializer;

            public DynamicSerializerExpandoObjectProxy(DynamicSerializer serializer)
            {
                _serializer = serializer;
            }

            public void Serialize(SerializationXmlTextWriter writer, ExpandoObject instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
            {
                if (instance == null)
                {
                    return;
                }

                _serializer.SerializeExpandoObject(writer, instance, namespaces, alwaysEmitTypes);
            }

            public void SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
            {
                Serialize(writer, (ExpandoObject)instance, namespaces, alwaysEmitTypes);
            }

            public ExpandoObject Deserialize(XmlReader reader)
            {
                return _serializer.DeserializeExpandoObject(reader);
            }

            public object DeserializeObject(XmlReader reader)
            {
                return Deserialize(reader);
            }
        }
    }
}
