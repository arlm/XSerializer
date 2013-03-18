using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public class XmlSerializer<T>
    {
        private readonly IXmlSerializer<T> _serializer;
        private readonly Encoding _encoding;
        private readonly XmlSerializerNamespaces _namespaces;
        private readonly Formatting _formatting;
        private readonly bool _alwaysEmitTypes;

        public XmlSerializer(params Type[] extraTypes)
            : this(options => {}, extraTypes)
        {
        }

        public XmlSerializer(Action<XmlSerializationOptions> setOptions, params Type[] extraTypes)
        {
            var options = new XmlSerializationOptions();
            
            if (setOptions != null)
            {
                setOptions(options);
            }

            if (options.RootElementName == null)
            {
                options.RootElementName = typeof(T).Name;
            }

            _serializer = XmlSerializerFactory.Instance.GetSerializer<T>(options.DefaultNamespace, extraTypes, options.RootElementName);
            _encoding = options.Encoding ?? Encoding.UTF8;
            _namespaces = options.Namespaces;
            _formatting = options.ShouldIndent ? Formatting.Indented : Formatting.None;
            _alwaysEmitTypes = options.ShouldAlwaysEmitTypes;
        }

        public XmlSerializer(IXmlSerializer<T> serializer, Encoding encoding, XmlSerializerNamespaces namespaces, bool indent, bool alwaysEmitTypes)
        {
            _serializer = serializer;
            _encoding = encoding ?? Encoding.UTF8;
            _namespaces = namespaces;
            _formatting = indent ? Formatting.Indented : Formatting.None;
            _alwaysEmitTypes = alwaysEmitTypes;
        }

        public IXmlSerializer<T> Serializer
        {
            get { return _serializer; }
        }

        public string Serialize(T instance)
        {
            return _serializer.Serialize(instance, _namespaces, _encoding, _formatting, _alwaysEmitTypes);
        }

        public void Serialize(Stream stream, T instance)
        {
            _serializer.Serialize(stream, instance, _namespaces, _encoding, _formatting, _alwaysEmitTypes);
        }

        public void Serialize(TextWriter writer, T instance)
        {
            _serializer.Serialize(writer, instance, _namespaces, _formatting, _alwaysEmitTypes);
        }

        public T Deserialize(string xml)
        {
            return _serializer.Deserialize(xml);
        }

        public T Deserialize(Stream stream)
        {
            return _serializer.Deserialize(stream);
        }

        public T Deserialize(TextReader reader)
        {
            return _serializer.Deserialize(reader);
        }
    }
}
