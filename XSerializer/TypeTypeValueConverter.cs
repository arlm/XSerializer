using System;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class TypeTypeValueConverter : IValueConverter
    {
        private static readonly Lazy<IValueConverter> _default = new Lazy<IValueConverter>(() => new TypeTypeValueConverter(null, null));

        private readonly RedactAttribute _redactAttribute;
        private readonly EncryptAttribute _encryptAttribute;

        public TypeTypeValueConverter(RedactAttribute redactAttribute, EncryptAttribute encryptAttribute)
        {
            _redactAttribute = redactAttribute;
            _encryptAttribute = encryptAttribute;
        }

        public static IValueConverter Default
        {
            get { return _default.Value; }
        }

        public object ParseString(string value, ISerializeOptions options)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return
                Type.GetType(
                    _encryptAttribute != null
                        ? EncryptionMechanism.Current.Decrypt(value, options.ShouldEncrypt)
                        : value);
        }

        public string GetString(object value, ISerializeOptions options)
        {
            var type = value as Type;

            if (type == null)
            {
                return null;
            }

            var typeString =
                _redactAttribute != null
                    ? _redactAttribute.Redact(type, options.ShouldRedact)
                    : _encryptAttribute != null
                        ? EncryptionMechanism.Current.Encrypt(GetStringValue(type), options.ShouldEncrypt)
                        : GetStringValue(type);

            return typeString;
        }

        private static string GetStringValue(Type type)
        {
            return
                type.Assembly.GetName().Name == "mscorlib"
                    ? type.FullName
                    : type.AssemblyQualifiedName;
        }
    }
}