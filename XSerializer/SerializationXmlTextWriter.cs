using System.IO;
using System.Text;
using System.Xml;

namespace XSerializer
{
    internal class SerializationXmlTextWriter : XmlTextWriter
    {
        private bool _hasWrittenStartDocument;
        private bool _hasWritternDefaultNamespaces;

        public SerializationXmlTextWriter(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
        }

        public SerializationXmlTextWriter(TextWriter writer)
            : base(writer)
        {
        }

        public override void WriteStartDocument()
        {
            if (!_hasWrittenStartDocument)
            {
                _hasWrittenStartDocument = true;
                base.WriteStartDocument();
            }
        }

        public override void WriteStartDocument(bool standalone)
        {
            if (!_hasWrittenStartDocument)
            {
                _hasWrittenStartDocument = true;
                base.WriteStartDocument(standalone);
            }
        }

        public void WriteDefaultNamespaces()
        {
            if (!_hasWritternDefaultNamespaces)
            {
                _hasWritternDefaultNamespaces = true;
                WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            }
        }
    }
}