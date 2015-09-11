using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    public static class JsonSerializer
    {
        private static readonly ConcurrentDictionary<Type, Func<IXSerializer>> _createXmlSerializerFuncs = new ConcurrentDictionary<Type, Func<IXSerializer>>();

        public static IXSerializer Create(Type type)
        {
            var createJsonSerializer = _createXmlSerializerFuncs.GetOrAdd(
                type, t =>
                {
                    var jsonSerializerType = typeof(JsonSerializer<>).MakeGenericType(t);
                    var ctor = jsonSerializerType.GetConstructor(new Type[0]);

                    Debug.Assert(ctor != null);

                    var lambda = Expression.Lambda<Func<IXSerializer>>(Expression.New(ctor));

                    return lambda.Compile();
                });

            return createJsonSerializer();
        }
    }

    public class JsonSerializer<T> : IXSerializer
    {
        private readonly IJsonSerializerConfiguration _configuration;
        private readonly IJsonSerializerInternal _serializer;

        public JsonSerializer()
            : this(new JsonSerializerConfiguration())
        {
        }

        public JsonSerializer(IJsonSerializerConfiguration configuration)
        {
            _configuration = configuration;

            EncryptAttribute encryptAttributeOrNull =
                ((EncryptAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(EncryptAttribute)))
                ?? (configuration.EncryptRootObject
                    ? new EncryptAttribute()
                    : null);

            _serializer = JsonSerializerFactory.GetSerializer(typeof(T), encryptAttributeOrNull);
        }

        string IXSerializer.Serialize(object instance)
        {
            var sb = new StringBuilder();

            using (var writer = new StringWriterWithEncoding(sb, _configuration.Encoding))
            {
                ((IXSerializer)this).Serialize(writer, instance);
            }

            return sb.ToString();
        }

        public string Serialize(T instance)
        {
            return ((IXSerializer)this).Serialize(instance);
        }

        void IXSerializer.Serialize(Stream stream, object instance)
        {
            using (var writer = new StreamWriter(stream, _configuration.Encoding))
            {
                ((IXSerializer)this).Serialize(writer, instance);
            }
        }

        public void Serialize(Stream stream, T instance)
        {
            ((IXSerializer)this).Serialize(stream, instance);
        }

        void IXSerializer.Serialize(TextWriter textWriter, object instance)
        {
            var info = GetJsonSerializeOperationInfo();

            using (var writer = new JsonWriter(textWriter, info))
            {
                _serializer.SerializeObject(writer, instance, info);
            }
        }

        public void Serialize(TextWriter textWriter, T instance)
        {
            ((IXSerializer)this).Serialize(textWriter, instance);
        }

        object IXSerializer.Deserialize(string json)
        {
            using (var reader = new StringReader(json))
            {
                return ((IXSerializer)this).Deserialize(reader);
            }
        }

        public T Deserialize(string json)
        {
            return (T)((IXSerializer)this).Deserialize(json);
        }

        object IXSerializer.Deserialize(Stream stream)
        {
            using (var reader = new StreamReader(stream, _configuration.Encoding))
            {
                return ((IXSerializer)this).Deserialize(reader);
            }
        }

        public T Deserialize(Stream stream)
        {
            return (T)((IXSerializer)this).Deserialize(stream);
        }

        object IXSerializer.Deserialize(TextReader textReader)
        {
            var info = GetJsonSerializeOperationInfo();

            using (var reader = new JsonReader(textReader, info))
            {
                return _serializer.DeserializeObject(reader, info);
            }
        }

        public T Deserialize(TextReader textReader)
        {
            return (T)((IXSerializer)this).Deserialize(textReader);
        }

        private IJsonSerializeOperationInfo GetJsonSerializeOperationInfo()
        {
            return new JsonSerializeOperationInfo
            {
                RedactEnabled = _configuration.RedactEnabled,
                EncryptionEnabled = _configuration.EncryptionEnabled,
                EncryptionMechanism = _configuration.EncryptionMechanism,
                EncryptKey = _configuration.EncryptKey,
                SerializationState = new SerializationState()
            };
        }
    }
}