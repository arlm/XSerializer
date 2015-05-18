using System;
using System.Collections.Generic;
using System.Linq;

namespace XSerializer
{
    internal class EnumTypeValueConverter : IValueConverter
    {
        private static readonly Lazy<IValueConverter> _default = new Lazy<IValueConverter>(() => new EnumTypeValueConverter(null, Enumerable.Empty<Type>()));

        private readonly RedactAttribute _redactAttribute;
        private readonly IEnumerable<Type> _enumExtraTypes;

        public EnumTypeValueConverter(RedactAttribute redactAttribute, IEnumerable<Type> extraTypes)
        {
            _redactAttribute = redactAttribute;
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

            var enumStringValue =
                _redactAttribute != null
                    ? _redactAttribute.Redact(enumValue, options.ShouldRedact)
                    : value.ToString();

            var combinedValue = value.GetType().Name + "." + enumStringValue;

            return combinedValue;
        }
    }
}