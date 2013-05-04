namespace XSerializer
{
    using System.Xml;

    public interface IXmlSerializer
    {
        void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options);
        object DeserializeObject(XmlReader reader);
    }

    public interface IXmlSerializer<T> : IXmlSerializer
    {
        void Serialize(SerializationXmlTextWriter writer, T instance, ISerializeOptions options);
        T Deserialize(XmlReader reader);
    }
}