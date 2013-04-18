using System;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class EnumSerializer<T> : IXmlSerializer<T>
    {
        private readonly string _elementName;

        public EnumSerializer(string elementName)
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("Generic argument of EnumSerializer<T> must be an Enum");
            }

            _elementName = elementName;
        }

        public void Serialize(SerializationXmlTextWriter writer, T value, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            SerializeObject(writer, value, namespaces, alwaysEmitTypes);
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, XmlSerializerNamespaces namespaces, bool alwaysEmitTypes)
        {
            if (value != null)
            {
                writer.WriteElementString(_elementName, value.ToString());
            }
        }

        public T Deserialize(XmlReader reader)
        {
            return (T)Enum.Parse(typeof(T), reader.ReadString());
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }
    }
}