using System.IO;
using System.Text;
using System.Xml;

namespace XSerializer.Tests
{
    public static class BclXmlSerializerExtensions
    {
        public static string SerializeObject(
            this System.Xml.Serialization.XmlSerializer serializer,
            object instance,
            Encoding encoding,
            Formatting formatting,
            ISerializeOptions options)
        {
            
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriterWithEncoding(sb, encoding ?? Encoding.UTF8))
            {
                using (var xmlWriter = new SerializationXmlTextWriter(stringWriter, options))
                {
                    xmlWriter.Formatting = formatting;
                    serializer.Serialize(xmlWriter, instance, options.Namespaces);
                }
            }

            return sb.ToString();
        }

        public static object DeserializeObject(this System.Xml.Serialization.XmlSerializer serializer, string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return serializer.Deserialize(xmlReader);
                }
            }
        }

        private class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding _encoding;

            public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
                : base(sb)
            {
                _encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return _encoding; }
            }
        }
    }
}