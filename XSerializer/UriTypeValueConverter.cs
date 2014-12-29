using System;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class UriTypeValueConverter : IValueConverter
    {
        private static readonly Lazy<IValueConverter> _default = new Lazy<IValueConverter>(() => new UriTypeValueConverter(null, null));

        private readonly RedactAttribute _redactAttribute;
        private readonly EncryptAttribute _encryptAttribute;

        public UriTypeValueConverter(RedactAttribute redactAttribute, EncryptAttribute encryptAttribute)
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
            if (value == null)
            {
                return null;
            }

            return
                new Uri(
                    _encryptAttribute != null
                        ? EncryptionMechanism.Current.Decrypt(value, options.ShouldEncrypt)
                        : value);
        }

        public string GetString(object value, ISerializeOptions options)
        {
            var uri = value as Uri;

            if (uri == null)
            {
                return null;
            }

            var uriString =
                _redactAttribute != null
                    ? _redactAttribute.Redact(uri, options.ShouldRedact)
                    : _encryptAttribute != null
                        ? EncryptionMechanism.Current.Encrypt(uri.ToString(), options.ShouldEncrypt)
                        : uri.ToString();

            return uriString;
        }
    }
}