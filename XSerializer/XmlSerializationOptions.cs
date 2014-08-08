using System;
using System.Text;
using System.Xml.Serialization;

namespace XSerializer
{
    public class XmlSerializationOptions : IXmlSerializerOptions, ISerializeOptions
    {
        private readonly XmlSerializerNamespaces _namespaces;
        private string _defaultNamespace;
        private string _rootElementName;
        private bool _shouldAlwaysEmitTypes;
        private bool _shouldRedact;
        private Type[] _extraTypes;
        private bool _treatEmptyElementAsString;
        private Encoding _encoding;
        private bool _shouldIndent;
        private bool _emitNil;

        public XmlSerializationOptions(
            XmlSerializerNamespaces namespaces = null,
            Encoding encoding = null,
            string defaultNamespace = null,
            bool shouldIndent = false,
            string rootElementName = null,
            bool shouldAlwaysEmitTypes = false,
            bool shouldRedact = true,
            bool treatEmptyElementAsString = false,
            bool emitNil = false)
        {
            _namespaces = namespaces ?? new XmlSerializerNamespaces();
            _encoding = encoding ?? Encoding.UTF8;
            _defaultNamespace = defaultNamespace;
            _shouldIndent = shouldIndent;
            _rootElementName = rootElementName;
            _shouldAlwaysEmitTypes = shouldAlwaysEmitTypes;
            _shouldRedact = shouldRedact;
            _extraTypes = null;
            _treatEmptyElementAsString = treatEmptyElementAsString;
            _emitNil = emitNil;
        }

        internal Encoding Encoding { get { return _encoding; } }
        string IXmlSerializerOptions.DefaultNamespace { get { return _defaultNamespace; } }
        XmlSerializerNamespaces ISerializeOptions.Namespaces { get { return _namespaces; } }
        internal bool ShouldIndent { get { return _shouldIndent; } }
        string IXmlSerializerOptions.RootElementName { get { return _rootElementName; } }
        bool ISerializeOptions.ShouldAlwaysEmitTypes { get { return _shouldAlwaysEmitTypes; } }
        bool ISerializeOptions.ShouldRedact { get { return _shouldRedact; } }
        Type[] IXmlSerializerOptions.ExtraTypes { get { return _extraTypes; } }
        RedactAttribute IXmlSerializerOptions.RedactAttribute { get { return null; } }
        bool IXmlSerializerOptions.TreatEmptyElementAsString { get { return _treatEmptyElementAsString; } }
        bool ISerializeOptions.ShouldEmitNil { get { return _emitNil; } }

        internal void SetExtraTypes(Type[] extraTypes)
        {
            _extraTypes = extraTypes;
        }

        public XmlSerializationOptions AddNamespace(string prefix, string ns)
        {
            _namespaces.Add(prefix, ns);
            return this;
        }

        public XmlSerializationOptions WithEncoding(Encoding encoding)
        {
            _encoding = encoding;
            return this;
        }

        public XmlSerializationOptions WithDefaultNamespace(string defaultNamespace)
        {
            _defaultNamespace = defaultNamespace;
            return this;
        }

        public XmlSerializationOptions Indent()
        {
            _shouldIndent = true;
            return this;
        }

        public XmlSerializationOptions SetRootElementName(string rootElementName)
        {
            _rootElementName = rootElementName;
            return this;
        }

        public XmlSerializationOptions AlwaysEmitTypes()
        {
            _shouldAlwaysEmitTypes = true;
            return this;
        }

        public XmlSerializationOptions DisableRedact()
        {
            _shouldRedact = false;
            return this;
        }

        public XmlSerializationOptions ShouldTreatEmptyElementAsString()
        {
            _treatEmptyElementAsString = true;
            return this;
        }

        public XmlSerializationOptions EmitNil()
        {
            _emitNil = true;
            return this;
        }
    }
}