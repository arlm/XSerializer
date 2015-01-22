using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public static class XmlSerializer
    {
        private static readonly ConcurrentDictionary<Type, Func<XmlSerializationOptions, Type[], IXmlSerializer>> _createXmlSerializerFuncs = new ConcurrentDictionary<Type, Func<XmlSerializationOptions, Type[], IXmlSerializer>>(); 

        public static IXmlSerializer Create(Type type, params Type[] extraTypes)
        {
            return Create(type, new XmlSerializationOptions(), extraTypes);
        }

        public static IXmlSerializer Create(Type type, Action<XmlSerializationOptions> setOptions, params Type[] extraTypes)
        {
            return Create(type, GetSerializationOptions(setOptions), extraTypes);
        }

        public static IXmlSerializer Create(Type type, XmlSerializationOptions options, params Type[] extraTypes)
        {
            var createXmlSerializer = _createXmlSerializerFuncs.GetOrAdd(
                type,
                t =>
                {
                    var xmlSerializerType = typeof(XmlSerializer<>).MakeGenericType(t);
                    var ctor = xmlSerializerType.GetConstructor(new[] { typeof(XmlSerializationOptions), typeof(Type[]) });

                    Debug.Assert(ctor != null);

                    var optionsParameter = Expression.Parameter(typeof(XmlSerializationOptions), "options");
                    var extraTypesParameter = Expression.Parameter(typeof(Type[]), "extraTypes");

                    var lambda =
                        Expression.Lambda<Func<XmlSerializationOptions, Type[], IXmlSerializer>>(
                            Expression.New(ctor, optionsParameter, extraTypesParameter),
                            optionsParameter,
                            extraTypesParameter);

                    return lambda.Compile();
                });

            return createXmlSerializer(options, extraTypes);
        }

        internal static XmlSerializationOptions GetSerializationOptions(Action<XmlSerializationOptions> setOptions)
        {
            var options = new XmlSerializationOptions();

            if (setOptions != null)
            {
                setOptions(options);
            }

            return options;
        }
    }

    public class XmlSerializer<T> : IXmlSerializer
    {
        private readonly IXmlSerializerInternal _serializer;
        private readonly Encoding _encoding;
        private readonly Formatting _formatting;
        private readonly ISerializeOptions _serializeOptions;

        public XmlSerializer(params Type[] extraTypes)
            : this(new XmlSerializationOptions(), extraTypes)
        {
        }

        public XmlSerializer(Action<XmlSerializationOptions> setOptions, params Type[] extraTypes)
            : this(XmlSerializer.GetSerializationOptions(setOptions), extraTypes)
        {
        }

        public XmlSerializer(XmlSerializationOptions options, params Type[] extraTypes)
        {
            if (((IXmlSerializerOptions)options).RootElementName == null)
            {
                var xmlRootAttribute = typeof(T).GetCustomAttributes(typeof(XmlRootAttribute), false).FirstOrDefault() as XmlRootAttribute;

                var rootElementName =
                    xmlRootAttribute != null && !string.IsNullOrWhiteSpace(xmlRootAttribute.ElementName)
                        ? xmlRootAttribute.ElementName
                        : typeof(T).GetElementName();

                options.SetRootElementName(rootElementName);
            }

            options.SetExtraTypes(extraTypes);

            _serializer = XmlSerializerFactory.Instance.GetSerializer<T>(options);
            _encoding = options.Encoding ?? Encoding.UTF8;
            _formatting = options.ShouldIndent ? Formatting.Indented : Formatting.None;
            _serializeOptions = options;
        }

        internal IXmlSerializerInternal Serializer
        {
            get { return _serializer; }
        }

        public string Serialize(T instance)
        {
            return _serializer.SerializeObject(instance, _encoding, _formatting, _serializeOptions);
        }

        string IXmlSerializer.Serialize(object instance)
        {
            return Serialize((T)instance);
        }

        public void Serialize(Stream stream, T instance)
        {
            _serializer.SerializeObject(stream, instance, _encoding, _formatting, _serializeOptions);
        }

        void IXmlSerializer.Serialize(Stream stream, object instance)
        {
            Serialize(stream, (T)instance);
        }

        public void Serialize(TextWriter writer, T instance)
        {
            _serializer.SerializeObject(writer, instance, _formatting, _serializeOptions);
        }

        void IXmlSerializer.Serialize(TextWriter writer, object instance)
        {
            Serialize(writer, (T)instance);
        }

        public T Deserialize(string xml)
        {
            return (T)_serializer.DeserializeObject(xml).ConvertIfNecessary(typeof(T));
        }

        object IXmlSerializer.Deserialize(string xml)
        {
            return Deserialize(xml);
        }

        public T Deserialize(Stream stream)
        {
            return (T)_serializer.DeserializeObject(stream).ConvertIfNecessary(typeof(T));
        }

        object IXmlSerializer.Deserialize(Stream stream)
        {
            return Deserialize(stream);
        }

        public T Deserialize(TextReader reader)
        {
            return (T)_serializer.DeserializeObject(reader).ConvertIfNecessary(typeof(T));
        }

        object IXmlSerializer.Deserialize(TextReader reader)
        {
            return Deserialize(reader);
        }
    }
}
