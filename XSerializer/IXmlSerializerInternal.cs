namespace XSerializer
{
    internal interface IXmlSerializerInternal
    {
        void SerializeObject(XSerializerXmlTextWriter writer, object instance, ISerializeOptions options);
        object DeserializeObject(XSerializerXmlReader reader, ISerializeOptions options);
    }
}