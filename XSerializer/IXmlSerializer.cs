using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public interface IXmlSerializer
    {
        void SerializeObject(object instance, SerializationXmlTextWriter writer, XmlSerializerNamespaces namespaces);
        object DeserializeObject(XmlReader reader);
    }

    public interface IXmlSerializer<T> : IXmlSerializer
    {
        void Serialize(T instance, SerializationXmlTextWriter writer, XmlSerializerNamespaces namespaces);
        T Deserialize(XmlReader reader);
    }
}