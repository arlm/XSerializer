using System;

namespace XSerializer
{
    internal class UriTypeValueConverter : IValueConverter
    {
        private static readonly Lazy<IValueConverter> _default = new Lazy<IValueConverter>(() => new UriTypeValueConverter(null));

        private readonly RedactAttribute _redactAttribute;

        public UriTypeValueConverter(RedactAttribute redactAttribute)
        {
            _redactAttribute = redactAttribute;
        }

        public static IValueConverter Default
        {
            get { return _default.Value; }
        }

        public object ParseString(string value)
        {
            if (value == null)
            {
                return null;
            }

            return new Uri(value);
        }

        public string GetString(object value, ISerializeOptions options)
        {
            var uri = value as Uri;

            if (uri == null)
            {
                return null;
            }

            var uriString =
                _redactAttribute == null
                    ? uri.ToString()
                    : _redactAttribute.Redact(uri, options.ShouldRedact);

            return uriString;
        }
    }
}