using System;
using System.Collections.Generic;
using System.Linq;
using XSerializer.Encryption;

namespace XSerializer
{
    internal class EnumTypeValueConverter : IValueConverter
    {
        private static readonly Lazy<IValueConverter> _default = new Lazy<IValueConverter>(() => new EnumTypeValueConverter(null, null, Enumerable.Empty<Type>()));

        private readonly RedactAttribute _redactAttribute;
        private readonly EncryptAttribute _encryptAttribute;
        private readonly IEnumerable<Type> _enumExtraTypes;

        public EnumTypeValueConverter(RedactAttribute redactAttribute, EncryptAttribute encryptAttribute, IEnumerable<Type> extraTypes)
        {
            _redactAttribute = redactAttribute;
            _encryptAttribute = encryptAttribute;
            _enumExtraTypes = extraTypes.Where(t => t.IsEnum).ToList();
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

            var enumTypeName = value.Substring(0, value.LastIndexOf('.'));
            var enumValue = value.Substring(value.LastIndexOf('.') + 1);

            if (_encryptAttribute != null)
            {
                enumValue = EncryptionProvider.Current.Decrypt(enumValue, options.ShouldEncrypt);
            }

            var enumType = _enumExtraTypes.Single(t => t.Name == enumTypeName);
            return Enum.Parse(enumType, enumValue);
        }

        public string GetString(object value, ISerializeOptions options)
        {
            var enumValue = value as Enum;

            if (enumValue == null)
            {
                return null;
            }

            var enumString =
                _redactAttribute != null
                    ? _redactAttribute.Redact(enumValue, options.ShouldRedact)
                    : _encryptAttribute != null
                        ? EncryptionProvider.Current.Encrypt(value.ToString(), options.ShouldEncrypt)
                        : value.ToString();

            return value.GetType().Name + "." + enumString;
        }
    }
}