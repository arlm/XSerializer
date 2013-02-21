using System.Text;
using System.Xml.Serialization;

namespace XSerializer
{
    public class XmlSerializationOptions
    {
        internal XmlSerializationOptions()
        {
            Encoding = Encoding.UTF8;
            Namespaces = new XmlSerializerNamespaces();
        }

        internal Encoding Encoding { get; set; }
        internal string DefaultNamespace { get; set; }
        internal XmlSerializerNamespaces Namespaces { get; set; }
        internal bool ShouldIndent { get; set; }
        internal string RootElementName { get; set; }

        public XmlSerializationOptions WithEncoding(Encoding encoding)
        {
            Encoding = encoding;
            return this;
        }

        public XmlSerializationOptions WithDefaultNamespace(string defaultNamespace)
        {
            DefaultNamespace = defaultNamespace;
            return this;
        }

        public XmlSerializationOptions AddNamespace(string prefix, string ns)
        {
            Namespaces.Add(prefix, ns);
            return this;
        }

        public XmlSerializationOptions Indent()
        {
            ShouldIndent = true;
            return this;
        }

        public XmlSerializationOptions SetRootElementName(string rootElementName)
        {
            RootElementName = rootElementName;
            return this;
        }
    }
}