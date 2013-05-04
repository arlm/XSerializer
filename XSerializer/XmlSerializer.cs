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
        private readonly Formatting _formatting;
        private readonly ISerializeOptions _serializeOptions;

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
                options.SetRootElementName(typeof(T).GetElementName());
            }

            options.ExtraTypes = extraTypes;

            _serializer = XmlSerializerFactory.Instance.GetSerializer<T>(options);
            _encoding = options.Encoding ?? Encoding.UTF8;
            _formatting = options.ShouldIndent ? Formatting.Indented : Formatting.None;
            _serializeOptions = options;
        }

        public XmlSerializer(IXmlSerializer<T> serializer, Encoding encoding, XmlSerializerNamespaces namespaces, bool indent, bool alwaysEmitTypes)
        {
            _serializer = serializer;
            _encoding = encoding ?? Encoding.UTF8;
            _formatting = indent ? Formatting.Indented : Formatting.None;
            
            _serializeOptions = new XmlSerializationOptions();
            
            if (alwaysEmitTypes)
            {
                ((XmlSerializationOptions)_serializeOptions).AlwaysEmitTypes();
            }

            if (namespaces != null)
            {
                foreach (var @namespace in namespaces.ToArray())
                {
                    ((XmlSerializationOptions)_serializeOptions).AddNamespace(@namespace.Name, @namespace.Namespace);
                }
            }
        }

        public IXmlSerializer<T> Serializer
        {
            get { return _serializer; }
        }

        public string Serialize(T instance)
        {
            return _serializer.Serialize(instance, _encoding, _formatting, _serializeOptions);
        }

        public void Serialize(Stream stream, T instance)
        {
            _serializer.Serialize(stream, instance, _encoding, _formatting, _serializeOptions);
        }

        public void Serialize(TextWriter writer, T instance)
        {
            _serializer.Serialize(writer, instance, _formatting, _serializeOptions);
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
