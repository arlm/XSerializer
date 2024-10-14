using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    /// <summary>
    /// A factory class that creates instances of the <see cref="IXSerializer"/> interface.
    /// </summary>
    /// <remarks>
    /// An instance of the generic <see cref="JsonSerializer{T}"/> class is returned from
    /// each method in this class.
    /// </remarks>
    public static class JsonSerializer
    {
        private static readonly ConcurrentDictionary<Type, Func<IJsonSerializerConfiguration, IXSerializer>> _createXmlSerializerFuncs = new ConcurrentDictionary<Type, Func<IJsonSerializerConfiguration, IXSerializer>>();

        /// <summary>
        /// Create an instance of <see cref="IXSerializer"/> for the given type using a default configuration.
        /// </summary>
        /// <param name="type">The type of the object that the serializer will operate on.</param>
        /// <returns>An instance of <see cref="IXSerializer"/>.</returns>
        /// <remarks>
        /// An instance of the generic <see cref="JsonSerializer{T}"/> class of type <paramref name="type"/>
        /// is returned from this method.
        /// </remarks>
        public static IXSerializer Create(Type type)
        {
            return Create(type, null);
        }

        /// <summary>
        /// Create an instance of <see cref="IXSerializer"/> for the given type using the specified configuration.
        /// </summary>
        /// <param name="type">The type of the object that the serializer will operate on.</param>
        /// <param name="configuration">The configuration for the serializer.</param>
        /// <returns>An instance of <see cref="IXSerializer"/>.</returns>
        /// <remarks>
        /// An instance of the generic <see cref="JsonSerializer{T}"/> class of type <paramref name="type"/>
        /// is returned from this method.
        /// </remarks>
        public static IXSerializer Create(Type type, IJsonSerializerConfiguration configuration)
        {
            var createJsonSerializer = _createXmlSerializerFuncs.GetOrAdd(
                type, t =>
                {
                    var jsonSerializerType = typeof(JsonSerializer<>).MakeGenericType(t);
                    var ctor = jsonSerializerType.GetConstructor(new[] { typeof(IJsonSerializerConfiguration) });

                    if (ctor == null) throw new InvalidOperationException("A source code change has resulted in broken reflection. typeof(JsonSerializer<>).MakeGenericType(type).GetConstructor(new[] { typeof(IJsonSerializerConfiguration) })");

                    var parameter = Expression.Parameter(typeof(IJsonSerializerConfiguration), "configuration");

                    var lambda = Expression.Lambda<Func<IJsonSerializerConfiguration, IXSerializer>>(
                        Expression.New(ctor, parameter),
                        parameter);

                    return lambda.Compile();
                });

            return createJsonSerializer(configuration ?? new JsonSerializerConfiguration());
        }
    }

    /// <summary>
    /// An object used for JSON serializing and deserializing objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize and deserialize.</typeparam>
    public class JsonSerializer<T> : IXSerializer
    {
        private readonly IJsonSerializerConfiguration _configuration;
        private readonly IJsonSerializerInternal _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer{T}"/> class using a
        /// default configuration.
        /// </summary>
        public JsonSerializer()
            : this(new JsonSerializerConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer{T}"/> class using the 
        /// specified configuration.
        /// </summary>
        /// <param name="configuration"></param>
        public JsonSerializer(IJsonSerializerConfiguration configuration)
        {
            _configuration = configuration;

            var encrypt =
                typeof(T).GetCustomAttribute<EncryptAttribute>() != null
                || configuration.EncryptRootObject;

            _serializer = JsonSerializerFactory.GetSerializer(typeof(T), encrypt, _configuration.MappingsByType, _configuration.MappingsByProperty, _configuration.ShouldUseAttributeDefinedInInterface);
        }

        /// <summary>
        /// Serialize the given object to a string.
        /// </summary>
        /// <param name="instance">The object to serialize.</param>
        /// <param name="useBOM">When true, do not skip BOM bytes, else skip those bytes.</param>
        /// <returns>A string representation of the object.</returns>
        string IXSerializer.Serialize(object instance, bool useBOM = true)
        {
            var sb = new StringBuilder();

            using (var writer = new StringWriterWithEncoding(sb, _configuration.Encoding))
            {
                ((IXSerializer)this).Serialize(writer, instance);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Serialize the given object to a JSON string.
        /// </summary>
        /// <param name="instance">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public string Serialize(T instance)
        {
            return ((IXSerializer)this).Serialize(instance);
        }

        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/> as JSON.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        /// <param name="useBOM">When true, do not skip BOM bytes, else skip those bytes.</param>
        void IXSerializer.Serialize(Stream stream, object instance, bool useBOM = true)
        {
            using (var writer = new StreamWriter(stream, _configuration.Encoding))
            {
                ((IXSerializer)this).Serialize(writer, instance);
            }
        }

        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/> as JSON.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        public void Serialize(Stream stream, T instance)
        {
            ((IXSerializer)this).Serialize(stream, instance);
        }

        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/> as JSON.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        void IXSerializer.Serialize(TextWriter textWriter, object instance)
        {
            var info = GetJsonSerializeOperationInfo();

            using (var writer = new JsonWriter(textWriter, info))
            {
                _serializer.SerializeObject(writer, instance, info);
            }
        }

        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/> as JSON.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        public void Serialize(TextWriter textWriter, T instance)
        {
            ((IXSerializer)this).Serialize(textWriter, instance);
        }

        /// <summary>
        /// Deserialize an object from a JSON string.
        /// </summary>
        /// <param name="json">A JSON representation of an object.</param>
        /// <returns>An object created from the JSON string.</returns>
        object IXSerializer.Deserialize(string json)
        {
            using (var reader = new StringReader(json))
            {
                return ((IXSerializer)this).Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserialize an object from a JSON representation of that object.
        /// </summary>
        /// <param name="json">A JSON representation of an object.</param>
        /// <returns>An object created from the JSON string.</returns>
        public T Deserialize(string json)
        {
            return (T)((IXSerializer)this).Deserialize(json);
        }

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/> containing a JSON representation
        /// of that object.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="Stream"/>.</returns>
        object IXSerializer.Deserialize(Stream stream)
        {
            using (var reader = new StreamReader(stream, _configuration.Encoding))
            {
                return ((IXSerializer)this).Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/> containing a JSON representation
        /// of that object.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="Stream"/>.</returns>
        public T Deserialize(Stream stream)
        {
            return (T)((IXSerializer)this).Deserialize(stream);
        }

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/> containing a JSON representation
        /// of that object.
        /// </summary>
        /// <param name="textReader">
        /// A <see cref="TextReader"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="TextReader"/>.</returns>
        object IXSerializer.Deserialize(TextReader textReader)
        {
            var info = GetJsonSerializeOperationInfo();

            using (var reader = new JsonReader(textReader, info))
            {
                var returnObject = _serializer.DeserializeObject(reader, info, "");

                if (reader.ReadContent("") || reader.NodeType == JsonNodeType.Invalid)
                {
                    throw new MalformedDocumentException(MalformedDocumentError.ExpectedEndOfString,
                        "", reader.Value, reader.Line, reader.Position, null, reader.NodeType);
                }

                return returnObject;
            }
        }

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/> containing a JSON representation
        /// of that object.
        /// </summary>
        /// <param name="textReader">
        /// A <see cref="TextReader"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="TextReader"/>.</returns>
        public T Deserialize(TextReader textReader)
        {
            return (T)((IXSerializer)this).Deserialize(textReader);
        }

        private IJsonSerializeOperationInfo GetJsonSerializeOperationInfo()
        {
            return new JsonSerializeOperationInfo
            {
                EncryptionMechanism = _configuration.EncryptionMechanism,
                EncryptKey = _configuration.EncryptKey,
                SerializationState = new SerializationState(),
                DateTimeHandler = _configuration.DateTimeHandler ?? DateTimeHandler.Default
            };
        }
    }
}