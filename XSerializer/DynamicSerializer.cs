using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml;

namespace XSerializer
{
    internal class DynamicSerializer : IXmlSerializerInternal<object>
    {
        private readonly IXmlSerializerOptions _options;

        public static IXmlSerializerInternal<T> GetSerializer<T>(IXmlSerializerOptions options)
        {
            var serializer = new DynamicSerializer(options);

            if (typeof(T) == typeof(object))
            {
                return (IXmlSerializerInternal<T>)serializer;
            }
            
            if (typeof(T) == typeof(ExpandoObject))
            {
                return (IXmlSerializerInternal<T>)new DynamicSerializerExpandoObjectProxy(serializer);
            }
            
            throw new InvalidOperationException("The only valid generic arguments for DynamicSerializer.GetSerializer<T> are object, dynamic, and ExpandoObject");
        }

        public DynamicSerializer(IXmlSerializerOptions options)
        {
            _options = options;
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            Serialize(writer, instance, options);
        }

        public void Serialize(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            var expando = instance as ExpandoObject;
            if (expando != null || instance == null)
            {
                SerializeExpandoObject(writer, expando, options);
                return;
            }

            IXmlSerializerInternal serializer;

            if (!options.ShouldAlwaysEmitTypes || instance.IsAnonymous())
            {
                serializer = CustomSerializer.GetSerializer(instance.GetType(), _options);
            }
            else
            {
                serializer = CustomSerializer.GetSerializer(typeof(object), _options.WithAdditionalExtraTypes(instance.GetType()));
            }

            serializer.SerializeObject(writer, instance, options);
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        public object Deserialize(XmlReader reader)
        {
            var isNil = reader.IsNil();

            if (isNil && reader.IsEmptyElement)
            {
                return null;
            }

            object deserializedObject;

            var type = reader.GetXsdType<object>(_options.ExtraTypes);

            if (type != null)
            {
                var serializer = XmlSerializerFactory.Instance.GetSerializer(type, _options.WithRootElementName(reader.Name));
                deserializedObject = serializer.DeserializeObject(reader);
            }
            else
            {
                deserializedObject = DeserializeToDynamic(reader);
            }

            return
                isNil
                    ? null
                    : deserializedObject;
        }

        private void SerializeExpandoObject(SerializationXmlTextWriter writer, IDictionary<string, object> expando, ISerializeOptions options)
        {
            if (expando == null && !options.ShouldEmitNil)
            {
                return;
            }

            writer.WriteStartDocument();
            writer.WriteStartElement(_options.RootElementName);
            writer.WriteDefaultDocumentNamespaces();

            using (writer.WriteDefaultNamespace(_options.DefaultNamespace))
            {
                if (expando == null)
                {
                    writer.WriteNilAttribute();
                    writer.WriteEndElement();
                    return;
                }

                foreach (var property in expando)
                {
                    if (property.Value == null)
                    {
                        continue;
                    }

                    IXmlSerializerInternal serializer;

                    if (property.Value is ExpandoObject)
                    {
                        serializer = DynamicSerializer.GetSerializer<ExpandoObject>(_options.WithRootElementName(property.Key));
                    }
                    else
                    {
                        serializer = CustomSerializer.GetSerializer(property.Value.GetType(), _options.WithRootElementName(property.Key));
                    }

                    serializer.SerializeObject(writer, property.Value, options);
                }

                writer.WriteEndElement();
            }
        }

        private dynamic DeserializeToDynamic(XmlReader reader)
        {
            object instance = null;
            var hasInstanceBeenCreated = false;

            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == _options.RootElementName)
                        {
                            instance = new ExpandoObject();
                            hasInstanceBeenCreated = true;

                            if (reader.IsEmptyElement)
                            {
                                if (_options.TreatEmptyElementAsString)
                                {
                                    instance = "";
                                }

                                return instance;
                            }
                        }
                        else
                        {
                            SetElementPropertyValue(reader, hasInstanceBeenCreated, (ExpandoObject)instance);
                        }
                        break;
                    case XmlNodeType.Text:
                        var stringValue = (string)new XmlTextSerializer(typeof(string), _options.RedactAttribute, _options.ExtraTypes).DeserializeObject(reader);
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
                            // If this is a number with leading zeros, treat it as a string so we don't lose those leading zeros.
                            if (stringValue[0] == '0' && stringValue.Length > 1)
                            {
                                instance = stringValue;
                            }
                            else
                            {
                                instance = intValue;
                            }

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
                            if (_options.TreatEmptyElementAsString)
                            {
                                var instanceAsExpando = instance as IDictionary<string, object>;
                                if (instanceAsExpando != null && instanceAsExpando.Count == 0)
                                {
                                    instance = "";
                                }
                            }

                            return CheckAndReturn(hasInstanceBeenCreated, instance);
                        }
                        break;
                }
            } while (reader.Read());

            throw new InvalidOperationException("Deserialization error: reached the end of the document without returning a value.");
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
                throw new InvalidOperationException("Deserialization error: attempted to return a deserialized instance before it was created.");
            }

            return instance;
        }

        private class DynamicSerializerExpandoObjectProxy : IXmlSerializerInternal<ExpandoObject>
        {
            private readonly DynamicSerializer _serializer;

            public DynamicSerializerExpandoObjectProxy(DynamicSerializer serializer)
            {
                _serializer = serializer;
            }

            public void Serialize(SerializationXmlTextWriter writer, ExpandoObject instance, ISerializeOptions options)
            {
                _serializer.SerializeExpandoObject(writer, instance, options);
            }

            public void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
            {
                Serialize(writer, (ExpandoObject)instance, options);
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
