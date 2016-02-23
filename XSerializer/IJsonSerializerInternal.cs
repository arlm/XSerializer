namespace XSerializer
{
    internal interface IJsonSerializerInternal
    {
        void SerializeObject(JsonWriter writer, object instance, IJsonSerializeOperationInfo info);
        object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info, string path);
    }
}