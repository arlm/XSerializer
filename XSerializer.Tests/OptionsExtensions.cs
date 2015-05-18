namespace XSerializer.Tests
{
    internal static class OptionsExtensions
    {
        public static object DeserializeObject(this IXmlSerializerInternal serializer, string xml)
        {
            return serializer.DeserializeObject(xml, new TestSerializeOptions());
        }
    }
}