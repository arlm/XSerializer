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

            IXmlSerializer serializer;

            var expando = instance as ExpandoObject;
            if (expando != null)
            {
                SerializeExpandoObject(writer, expando, namespaces);
                return;
            }

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

        private void SerializeExpandoObject(SerializationXmlTextWriter writer, ExpandoObject instance, XmlSerializerNamespaces namespaces)
        {

        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        public object Deserialize(XmlReader reader)
        {
            return DeserializeExpandoObject(reader);
        }

        private ExpandoObject DeserializeExpandoObject(XmlReader reader)
        {
            // TODO: implement
            return null;
        }

        private class DynamicSerializerExpandoObjectProxy : IXmlSerializer<ExpandoObject>
        {
            private readonly DynamicSerializer _serializer;
            private readonly string _defaultNamespace;
            private readonly Type[] _extraTypes;
            private readonly string _rootElementName;

            public DynamicSerializerExpandoObjectProxy(DynamicSerializer serializer)
            {
                _serializer = serializer;
            }

            public void Serialize(SerializationXmlTextWriter writer, ExpandoObject instance, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
            {
                _serializer.Serialize(writer, instance, namespaces, alwaysEmitTypes);
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
