namespace XSerializer
{
    using System.Xml;

    internal interface IXmlSerializerInternal
    {
        void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options);
        object DeserializeObject(XmlReader reader);
    }
}