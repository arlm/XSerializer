using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace XSerializer
{
    internal class SerializationXmlTextWriter : XmlTextWriter
    {
        private readonly ISerializeOptions _options;
        private readonly Stack<string> _defaultNamespaceStack = new Stack<string>(); 

        private bool _hasWrittenStartDocument;
        private bool _hasWritternDefaultDocumentNamespaces;

        public SerializationXmlTextWriter(TextWriter writer, ISerializeOptions options)
            : base(writer)
        {
            _options = options;
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
    }
}