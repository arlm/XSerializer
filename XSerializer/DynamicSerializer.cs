using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class DynamicSerializer : IXmlSerializer<object>
    {
        private readonly IOptions _options;

        public static IXmlSerializer<T> GetSerializer<T>(IOptions options)
        {
            var serializer = new DynamicSerializer(options);

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

        public DynamicSerializer(IOptions options)
        {
            _options = options;
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
                serializer = CustomSerializer.GetSerializer(instance.GetType(), _options);
            }
            else
            {
                serializer = CustomSerializer.GetSerializer(typeof(object), _options.WithAdditionalExtraTypes(instance.GetType()));
            }

            serializer.SerializeObject(writer, instance, namespaces, alwaysEmitTypes);
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        public object Deserialize(XmlReader reader)
        {
            var type = reader.GetXsdType<object>(_options.ExtraTypes);
            if (type != null)
            {
                var serializer = XmlSerializerFactory.Instance.GetSerializer(type, _options.WithRootElementName(reader.Name));
                return serializer.DeserializeObject(reader);
            }

            return DeserializeToDynamic(reader);
        }

        private void SerializeExpandoObject(SerializationXmlTextWriter writer, IDictionary<string, object> expando, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(_options.RootElementName);
            writer.WriteDefaultNamespaces();

            if (!string.IsNullOrWhiteSpace(_options.DefaultNamespace))
            {
                writer.WriteAttributeString("xmlns", null, null, _options.DefaultNamespace);
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
                    serializer = DynamicSerializer.GetSerializer<ExpandoObject>(_options.WithRootElementName(property.Key));
                }
                else
                {
                    serializer = CustomSerializer.GetSerializer(property.Value.GetType(), _options.WithRootElementName(property.Key));
                }

                serializer.SerializeObject(writer, property.Value, namespaces, alwaysEmitTypes);
            }

            writer.WriteEndElement();
        }

        private dynamic DeserializeToDynamic(XmlReader reader)
        {
            object instance = null;
            var hasInstanceBeenCreated = false;

            var attributes = new Dictionary<string, string>();

            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _options.RootElementName)
                        {
                            instance = new ExpandoObject();
                            hasInstanceBeenCreated = true;
                        }
                        else
                        {
                            SetElementPropertyValue(reader, hasInstanceBeenCreated, (ExpandoObject)instance);
                        }
                        break;
                    case XmlNodeType.Text:
                        var stringValue = (string)XmlTextSerializer.GetSerializer(typeof(string)).DeserializeObject(reader);
                        hasInstanceBeenCreated = true;

                        bool boolValue;
                        if (bool.TryParse(stringValue, out boolValue))
                        {
                            instance = boolValue;
                            break;
                        }

                        int intValue;
                        if (int.TryParse(stringValue, out intValue))
                        {
                            instance = intValue;
                            break;
                        }

                        decimal decimalValue;
                        if (decimal.TryParse(stringValue, out decimalValue))
                        {
                            instance = decimalValue;
                            break;
                        }

                        DateTime dateTimeValue;
                        if (DateTime.TryParse(stringValue, out dateTimeValue))
                        {
                            instance = dateTimeValue.ToUniversalTime();
                            break;
                        }

                        // TODO: add more types to check?

                        instance = stringValue;
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == _options.RootElementName)
                        {
                            return CheckAndReturn(hasInstanceBeenCreated, instance);
                        }
                        break;
                }
            } while (reader.Read());

            throw new SerializationException("Couldn't serialize... for some reason. (You know, I should put a better exception message here...)");
        }

        private void SetElementPropertyValue(XmlReader reader, bool hasInstanceBeenCreated, IDictionary<string, object> expando)
        {
            var propertyName = reader.Name;
            var serializer = GetSerializer<object>(_options.WithRootElementName(reader.Name));
            var value = serializer.Deserialize(reader);
            expando[propertyName] = value;
        }

        private static object CheckAndReturn(bool hasInstanceBeenCreated, object instance)
        {
            if (!hasInstanceBeenCreated)
            {
                throw new SerializationException("Awwww, crap.");
            }

            return instance;
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
                return _serializer.DeserializeToDynamic(reader);
            }

            public object DeserializeObject(XmlReader reader)
            {
                return Deserialize(reader);
            }
        }
    }
}
