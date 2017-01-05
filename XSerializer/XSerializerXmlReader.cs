using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    public class XSerializerXmlReader : XmlReader
    {
        private const string _dummyNodeName = "___w__0_0__T___";
        private const string _dummyNodeStartElement = "<" + _dummyNodeName + ">";
        private const string _dummyNodeEndElement = "</" + _dummyNodeName + ">";

        private static readonly Regex _isStartElementRegex = new Regex(@"^\s*<", RegexOptions.Compiled);

        private readonly IEncryptionMechanism _encryptionMechanism;
        private readonly object _encryptKey;
        private readonly SerializationState _serializationState;

        private readonly XmlReader _primaryReader;

        private XmlReader _currentReader;
        private XSerializerXmlReader _surrogateReader;

        private XSerializerXmlReader(string xml, IEncryptionMechanism encryptionMechanism, object encryptKey, SerializationState serializationState)
            : this(new XmlTextReader(new StringReader(xml)), encryptionMechanism, encryptKey, serializationState)
        {
        }

        public XSerializerXmlReader(XmlReader reader, IEncryptionMechanism encryptionMechanism, object encryptKey, SerializationState serializationState)
        {
            _primaryReader = reader;
            _currentReader = reader;

            _encryptionMechanism = encryptionMechanism;
            _encryptKey = encryptKey;
            _serializationState = serializationState;
        }

        public bool IsDecryptionEnabled { get; set; }

        public override bool Read()
        {
            if (_surrogateReader != null)
            {
                if (_surrogateReader.Read() && !IsAtDummyNodeEndElement())
                {
                    // If this isn't the closing dummy node, return true.
                    return true;
                }

                // When we're done with surrogateReader, get rid of it and go back to primaryReader.
                _surrogateReader.Close();
                _surrogateReader = null;
                _currentReader = _primaryReader;
            }

            var read = _primaryReader.Read();

            if (!read)
            {
                return false;
            }

            // If we're decrypting, and our node contains text that starts with '<', then
            // assume that the text contains one or more encrypted child elements. In this
            // case, load the decrypted contents into a surrogate xml reader and use that
            // reader until it reads to its end. Then switch back to the primary reader.
            if (IsDecryptionEnabled && NodeType == XmlNodeType.Text)
            {
                var value = Value;

                if (_isStartElementRegex.IsMatch(value))
                {
                    // The xml fragment contained in the Value property may contain multiple
                    // top-level elements. To ensure valid xml, wrap the fragment in a dummy node.
                    var xml = _dummyNodeStartElement + value + _dummyNodeEndElement;

                    _surrogateReader = new XSerializerXmlReader(xml, _encryptionMechanism, _encryptKey,
                        _serializationState);
                    _currentReader = _surrogateReader;

                    _surrogateReader.Read(); // Advance to the opening dummy node
                    return _surrogateReader.Read(); // Advance to the first decrypted node.
                }
            }

            return true;
        }

        public override void Close()
        {
            _primaryReader.Close();

            if (_surrogateReader != null)
            {
                _surrogateReader.Close();
            }
        }

        public override string Value
        {
            get { return MaybeDecrypt(_currentReader.Value); }
        }

        public override string ReadString()
        {
            return MaybeDecrypt(_currentReader.ReadString());
        }

        public override string GetAttribute(string name)
        {
            return MaybeDecrypt(_currentReader.GetAttribute(name));
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return MaybeDecrypt(_currentReader.GetAttribute(name, namespaceURI));
        }

        public override string GetAttribute(int i)
        {
            return MaybeDecrypt(_currentReader.GetAttribute(i));
        }

        private string MaybeDecrypt(string value)
        {
            if (_surrogateReader != null)
            {
                return _surrogateReader.MaybeDecrypt(value);
            }

            return
                IsDecryptionEnabled && !string.IsNullOrEmpty(value)
                    ? _encryptionMechanism.Decrypt(value, _encryptKey, _serializationState)
                    : value;
        }

        private bool IsAtDummyNodeEndElement()
        {
            return _surrogateReader.NodeType == XmlNodeType.EndElement
                   && _surrogateReader.Name == _dummyNodeName;
        }

        #region Methods/Properties delegated to _currentReader

        public override bool MoveToAttribute(string name)
        {
            return _currentReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return _currentReader.MoveToAttribute(name, ns);
        }

        public override bool MoveToFirstAttribute()
        {
            return _currentReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return _currentReader.MoveToNextAttribute();
        }

        public override bool MoveToElement()
        {
            return _currentReader.MoveToElement();
        }

        public override bool ReadAttributeValue()
        {
            return _currentReader.ReadAttributeValue();
        }

        public override string LookupNamespace(string prefix)
        {
            return _currentReader.LookupNamespace(prefix);
        }

        public override void ResolveEntity()
        {
            _currentReader.ResolveEntity();
        }

        public override XmlNodeType NodeType
        {
            get { return _currentReader.NodeType; }
        }

        public override string LocalName
        {
            get { return _currentReader.LocalName; }
        }

        public override string NamespaceURI
        {
            get { return _currentReader.NamespaceURI; }
        }

        public override string Prefix
        {
            get { return _currentReader.Prefix; }
        }

        public override int Depth
        {
            get { return _currentReader.Depth; }
        }

        public override string BaseURI
        {
            get { return _currentReader.BaseURI; }
        }

        public override bool IsEmptyElement
        {
            get { return _currentReader.IsEmptyElement; }
        }

        public override int AttributeCount
        {
            get { return _currentReader.AttributeCount; }
        }

        public override bool EOF
        {
            get { return _currentReader.EOF; }
        }

        public override ReadState ReadState
        {
            get { return _currentReader.ReadState; }
        }

        public override XmlNameTable NameTable
        {
            get { return _currentReader.NameTable; }
        }

        #endregion
    }
}
