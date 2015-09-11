using System.IO;

namespace XSerializer
{
    internal interface IJsonSerializerInternal
    {
        void SerializeObject(TextWriter writer, object instance, IJsonSerializeOperationInfo info);
        object DeserializeObject(JsonReader reader, IJsonSerializeOperationInfo info);
    }
}