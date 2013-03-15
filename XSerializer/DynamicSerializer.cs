using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class DynamicSerializer : IXmlSerializer<object>
    {
        public static DynamicSerializer Instance = new DynamicSerializer();

        private DynamicSerializer()
        {
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces)
        {
            Serialize(writer, instance, namespaces);
        }

        public void Serialize(SerializationXmlTextWriter writer, object instance, XmlSerializerNamespaces namespaces)
        {
            // TODO: Implement
        }

        public object DeserializeObject(XmlReader reader)
        {
            return Deserialize(reader);
        }

        public object Deserialize(XmlReader reader)
        {
            // TODO: Implement
            return null;
        }
    }
}
