namespace XSerializer
{
    using System.Xml;

    internal interface IXmlSerializerInternal
    {
        void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options);
        object DeserializeObject(XmlReader reader);
    }

    internal interface IXmlSerializerInternal<T> : IXmlSerializerInternal
    {
        void Serialize(SerializationXmlTextWriter writer, T instance, ISerializeOptions options);
        T Deserialize(XmlReader reader);
    }
}