using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XSerializer.Encryption;

namespace XSerializer
{
    /// <summary>
    /// A factory class that creates instances of the <see cref="IXSerializer"/> interface.
    /// </summary>
    /// <remarks>
    /// An instance of the generic <see cref="XmlSerializer{T}"/> class is returned from
    /// each method in this class.
    /// </remarks>
    public static class XmlSerializer
    {
        private static readonly ConcurrentDictionary<Type, Func<XmlSerializationOptions, Type[], IXSerializer>> _createXmlSerializerFuncs = new ConcurrentDictionary<Type, Func<XmlSerializationOptions, Type[], IXSerializer>>();

        /// <summary>
        /// Create an instance of <see cref="IXSerializer"/> for the given type using a default configuration.
        /// </summary>
        /// <param name="type">The type of the object that the serializer will operate on.</param>
        /// <param name="extraTypes">Extra types that can be serialized.</param>
        /// <returns>An instance of <see cref="IXSerializer"/>.</returns>
        /// <remarks>
        /// An instance of the generic <see cref="XmlSerializer{T}"/> class of type <paramref name="type"/>
        /// is returned from this method.
        /// </remarks>
        public static IXSerializer Create(Type type, params Type[] extraTypes)
        {
            return Create(type, new XmlSerializationOptions(), extraTypes);
        }

        /// <summary>
        /// Create an instance of <see cref="IXSerializer"/> for the given type using a default configuration.
        /// </summary>
        /// <param name="type">The type of the object that the serializer will operate on.</param>
        /// <param name="setOptions">A callback for setting options.</param>
        /// <param name="extraTypes">Extra types that can be serialized.</param>
        /// <returns>An instance of <see cref="IXSerializer"/>.</returns>
        /// <remarks>
        /// An instance of the generic <see cref="XmlSerializer{T}"/> class of type <paramref name="type"/>
        /// is returned from this method.
        /// </remarks>
        public static IXSerializer Create(Type type, Action<XmlSerializationOptions> setOptions, params Type[] extraTypes)
        {
            return Create(type, GetSerializationOptions(setOptions), extraTypes);
        }

        /// <summary>
        /// Create an instance of <see cref="IXSerializer"/> for the given type using a default configuration.
        /// </summary>
        /// <param name="type">The type of the object that the serializer will operate on.</param>
        /// <param name="options">Options.</param>
        /// <param name="extraTypes">Extra types that can be serialized.</param>
        /// <returns>An instance of <see cref="IXSerializer"/>.</returns>
        /// <remarks>
        /// An instance of the generic <see cref="XmlSerializer{T}"/> class of type <paramref name="type"/>
        /// is returned from this method.
        /// </remarks>
        public static IXSerializer Create(Type type, XmlSerializationOptions options, params Type[] extraTypes)
        {
            var createXmlSerializer = _createXmlSerializerFuncs.GetOrAdd(
                type,
                t =>
                {
                    var xmlSerializerType = typeof(XmlSerializer<>).MakeGenericType(t);
                    var ctor = xmlSerializerType.GetConstructor(new[] { typeof(XmlSerializationOptions), typeof(Type[]) });

                    if (ctor == null) throw new InvalidOperationException("A source code change has resulted in broken reflection. typeof(XmlSerializer<>).MakeGenericType(type).GetConstructor(new[] { typeof(XmlSerializationOptions), typeof(Type[]) })");
                    var optionsParameter = Expression.Parameter(typeof(XmlSerializationOptions), "options");
                    var extraTypesParameter = Expression.Parameter(typeof(Type[]), "extraTypes");

                    var lambda =
                        Expression.Lambda<Func<XmlSerializationOptions, Type[], IXSerializer>>(
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

    /// <summary>
    /// An object used for XML serializing and deserializing objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize and deserialize.</typeparam>
    public class XmlSerializer<T> : IXSerializer
    {
        private readonly IXmlSerializerInternal _serializer;
        private readonly Encoding _encoding;
        private readonly Formatting _formatting;
        private readonly ISerializeOptions _serializeOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializer{T}"/> class using a
        /// default configuration.
        /// </summary>
        /// <param name="extraTypes">Extra types that can be serialized.</param>
        public XmlSerializer(params Type[] extraTypes)
            : this(new XmlSerializationOptions(), extraTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializer{T}"/> class using a
        /// default configuration.
        /// </summary>
        /// <param name="setOptions">A callback for setting options.</param>
        /// <param name="extraTypes">Extra types that can be serialized.</param>
        public XmlSerializer(Action<XmlSerializationOptions> setOptions, params Type[] extraTypes)
            : this(XmlSerializer.GetSerializationOptions(setOptions), extraTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializer{T}"/> class using a
        /// default configuration.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="extraTypes">Extra types that can be serialized.</param>
        public XmlSerializer(XmlSerializationOptions options, params Type[] extraTypes)
        {
            if (((IXmlSerializerOptions)options).RootElementName == null)
            {
                var xmlRootAttribute = typeof(T).GetCustomAttribute<XmlRootAttribute>();

                var rootElementName =
                    xmlRootAttribute != null && !string.IsNullOrWhiteSpace(xmlRootAttribute.ElementName)
                        ? xmlRootAttribute.ElementName
                        : typeof(T).GetElementName();

                var rootNamespace =
                    xmlRootAttribute != null && !string.IsNullOrWhiteSpace (xmlRootAttribute.Namespace)
                        ? xmlRootAttribute.Namespace
                        : string.Empty;

                options.WithDefaultNamespace (rootNamespace);
                options.SetRootElementName(rootElementName);
            }

            options.SetExtraTypes(extraTypes);

            EncryptAttribute encryptAttributeOrNull =
                typeof(T).GetCustomAttribute<EncryptAttribute>()
                ?? (options.ShouldEncryptRootObject
                    ? new EncryptAttribute()
                    : null);

            _serializer = XmlSerializerFactory.Instance.GetSerializer<T>(encryptAttributeOrNull, options);
            _encoding = options.Encoding ?? Encoding.UTF8;
            _formatting = options.ShouldIndent ? Formatting.Indented : Formatting.None;
            _serializeOptions = options;
        }

        internal IXmlSerializerInternal Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        /// Serialize the given object to an XML string.
        /// </summary>
        /// <param name="instance">The object to serialize.</param>
        /// <returns>An XML string representation of the object.</returns>
        public string Serialize(T instance)
        {
            return _serializer.SerializeObject(instance, _encoding, _formatting, _serializeOptions);
        }

        string IXSerializer.Serialize(object instance, bool useBOM = true)
        {
            return Serialize((T)instance);
        }

        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/> as XML.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        /// <param name="useBOM">When true, do not skip BOM bytes, else skip those bytes.</param>
        public void Serialize(Stream stream, T instance, bool useBOM = true)
        {
            _serializer.SerializeObject(stream, instance, _encoding, _formatting, _serializeOptions);

            if (!useBOM)
            {
                SkipBOM(stream);
            }
        }

        void IXSerializer.Serialize(Stream stream, object instance, bool useBOM = true)
        {
            Serialize(stream, (T)instance, useBOM);
        }

        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/> as JSON.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        public void Serialize(TextWriter writer, T instance)
        {
            _serializer.SerializeObject(writer, instance, _formatting, _serializeOptions);
        }

        void IXSerializer.Serialize(TextWriter writer, object instance)
        {
            Serialize(writer, (T)instance);
        }

        /// <summary>
        /// Deserialize an object from a XML representation of that object.
        /// </summary>
        /// <param name="xml">A XML representation of an object.</param>
        /// <returns>An object created from the XML string.</returns>
        public T Deserialize(string xml)
        {
            return (T)_serializer.DeserializeObject(xml, _serializeOptions).ConvertIfNecessary(typeof(T));
        }

        object IXSerializer.Deserialize(string xml)
        {
            return Deserialize(xml);
        }

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/> containing a XML representation
        /// of that object.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="Stream"/>.</returns>
        public T Deserialize(Stream stream)
        {
            return (T)_serializer.DeserializeObject(stream, _serializeOptions).ConvertIfNecessary(typeof(T));
        }

        object IXSerializer.Deserialize(Stream stream)
        {
            return Deserialize(stream);
        }

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/> containing a XML representation
        /// of that object.
        /// </summary>
        /// <param name="reader">
        /// A <see cref="TextReader"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="TextReader"/>.</returns>
        public T Deserialize(TextReader reader)
        {
            return (T)_serializer.DeserializeObject(reader, _serializeOptions).ConvertIfNecessary(typeof(T));
        }

        object IXSerializer.Deserialize(TextReader reader)
        {
            return Deserialize(reader);
        }

        static void SkipBOM(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            int cursor = 0;

            // UTF-32,
            if (IsMatch(stream, new byte[] { 0x00, 0x00, 0xFE, 0xFF }) || IsMatch(stream, new byte[] { 0xFF, 0xFE, 0x00, 0x00 }))
                cursor = 4;
            // UTF-16
            if (IsMatch(stream, new byte[] { 0xFE, 0xFF }) || IsMatch(stream, new byte[] { 0xFF, 0xFE }))
                cursor = 2;
            // UTF-8
            if (IsMatch(stream, new byte[] { 0xEF, 0xBB, 0xBF }))
                cursor = 3;

            stream.Seek(cursor, SeekOrigin.Begin);

            static bool IsMatch(Stream stream, byte[] match)
            {
                stream.Position = 0;
                var buffer = new byte[match.Length];
                stream.Read(buffer, 0, buffer.Length);

                return !buffer.Where((readByte, index) => readByte != match[index]).Any();
            }
        }
    }
}
