using System;
using System.Xml;

namespace XSerializer
{
    internal class EnumSerializer : IXmlSerializerInternal<Enum>
    {
        private readonly IXmlSerializerOptions _options;

        public EnumSerializer(IXmlSerializerOptions options)
        {
            _options = options;
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object instance, ISerializeOptions options)
        {
            throw new NotImplementedException();
        }

        public object DeserializeObject(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void Serialize(SerializationXmlTextWriter writer, Enum instance, ISerializeOptions options)
        {
            throw new NotImplementedException();
        }

        public Enum Deserialize(XmlReader reader)
        {
            throw new NotImplementedException();
        }
    }
}