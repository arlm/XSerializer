using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class XSerializerXmlTextWriter : XmlTextWriter
    {
        private readonly EncryptingTextWriter _encryptingTextWriter;

        private readonly ISerializeOptions _options;
        private readonly Stack<string> _defaultNamespaceStack = new Stack<string>();

        private bool _needsClearTextFlush;
        private bool _isWritingAttribute;
        private bool _isEncryptionEnabled;

        private bool _hasWrittenStartDocument;
        private bool _hasWritternDefaultDocumentNamespaces;

        public XSerializerXmlTextWriter(TextWriter writer, ISerializeOptions options)
            : this(new EncryptingTextWriter(writer, options.GetEncryptionMechanism(), options.EncryptKey), options)
        {
        }

        private XSerializerXmlTextWriter(EncryptingTextWriter encryptingTextWriter, ISerializeOptions options)
            : base(encryptingTextWriter)
        {
            _encryptingTextWriter = encryptingTextWriter;
            _options = options;
        }

        public bool IsEncryptionEnabled
        {
            get { return _isEncryptionEnabled; }
            set
            {
                if (_isEncryptionEnabled != value)
                {
                    if (!_isEncryptionEnabled && value)
                    {
                        if (_isWritingAttribute)
                        {
                            Flush();
                        }
                        else
                        {
                            _needsClearTextFlush = true;
                        }
                    }
                    else if (_isEncryptionEnabled && !value)
                    {
                        DoClearTextFlushIfNecessary();
                        Flush();
                    }

                    _isEncryptionEnabled = value;
                    _encryptingTextWriter.IsEncryptionEnabled = value;
                }
            }
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            DoClearTextFlushIfNecessary();

            base.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            DoClearTextFlushIfNecessary();

            base.WriteString(text);
        }

        private void DoClearTextFlushIfNecessary()
        {
            if (_needsClearTextFlush && !_isWritingAttribute)
            {
                WriteWhitespace("");
                _needsClearTextFlush = false;

                _encryptingTextWriter.IsEncryptionEnabled = false;
                Flush();
                _encryptingTextWriter.IsEncryptionEnabled = true;
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            _isWritingAttribute = true;

            var wasEncryptionEnabled = _encryptingTextWriter.IsEncryptionEnabled;

            if (_needsClearTextFlush)
            {
                _encryptingTextWriter.IsEncryptionEnabled = false;
            }

            base.WriteStartAttribute(prefix, localName, ns);

            if (_needsClearTextFlush && wasEncryptionEnabled)
            {
                Flush();
                _encryptingTextWriter.IsEncryptionEnabled = wasEncryptionEnabled;
            }
        }

        public override void WriteEndAttribute()
        {
            var wasEncryptionEnabled = _encryptingTextWriter.IsEncryptionEnabled;

            if (_needsClearTextFlush)
            {
                if (wasEncryptionEnabled)
                {
                    Flush();
                }

                _encryptingTextWriter.IsEncryptionEnabled = false;
            }

            base.WriteEndAttribute();

            if (_needsClearTextFlush)
            {
                _encryptingTextWriter.IsEncryptionEnabled = wasEncryptionEnabled;
            }

            _isWritingAttribute = false;
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

        public void WriteDefaultDocumentNamespaces()
        {
            if (!_hasWritternDefaultDocumentNamespaces)
            {
                _hasWritternDefaultDocumentNamespaces = true;

                if (_options.Namespaces.Count == 0)
                {
                    WriteXmlnsAttributeString("xsd", "http://www.w3.org/2001/XMLSchema");
                    WriteXmlnsAttributeString("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                }
                else
                {
                    foreach (var name in _options.Namespaces.ToArray().Where(name => !name.IsEmpty))
                    {
                        WriteXmlnsAttributeString(name.Name, name.Namespace);
                    }
                }
            }
        }

        private void WriteXmlnsAttributeString(string localName, string ns)
        {
            WriteAttributeString("xmlns", localName, null, ns);
        }

        public IDisposable WriteDefaultNamespace(string defaultNamespace)
        {
            if (!string.IsNullOrWhiteSpace(defaultNamespace))
            {
                if (!_defaultNamespaceStack.Contains(defaultNamespace))
                {
                    WriteAttributeString("xmlns", null, null, defaultNamespace);
                }

                _defaultNamespaceStack.Push(defaultNamespace);
                
                return new StackPopper(_defaultNamespaceStack);
            }

            return new Nothing();
        }

        public bool IsEmpty
        {
            get { return !_hasWrittenStartDocument; }
        }

        private class StackPopper : IDisposable
        {
            private readonly Stack<string> _stack;

            public StackPopper(Stack<String> stack)
            {
                _stack = stack;
            }

            public void Dispose()
            {
                _stack.Pop();
            }
        }

        private class Nothing : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private class EncryptingTextWriter : TextWriter
        {
            private readonly List<char> _buffer = new List<char>();

            private readonly TextWriter _writer;
            private readonly IEncryptionMechanism _encryptionMechanism;
            private readonly object _encryptKey;

            public EncryptingTextWriter(TextWriter writer, IEncryptionMechanism encryptionMechanism, object encryptKey)
            {
                _writer = writer;
                _encryptionMechanism = encryptionMechanism;
                _encryptKey = encryptKey;
            }

            public bool IsEncryptionEnabled { get; set; }

            public override Encoding Encoding
            {
                get { return _writer.Encoding; }
            }

            public override IFormatProvider FormatProvider
            {
                get { return _writer.FormatProvider; }
            }

            public override string NewLine
            {
                get { return _writer.NewLine; }
                set { _writer.NewLine = value; }
            }

            public override void Write(char value)
            {
                _buffer.Add(value);
            }

            public override void Flush()
            {
                if (_buffer.Count == 0)
                {
                    return;
                }

                var value = new string(_buffer.ToArray());

                if (IsEncryptionEnabled)
                {
                    value = _encryptionMechanism.Encrypt(value, _encryptKey);
                }

                _writer.Write(value);
                _writer.Flush();

                _buffer.Clear();
            }

            protected override void Dispose(bool disposing)
            {
                Flush();
                _writer.Dispose();
            }
        }
    }
}