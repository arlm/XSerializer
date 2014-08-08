using System;
using System.Collections.Generic;
using System.Linq;

namespace XSerializer
{
    internal class EnumTypeValueConverter : IValueConverter
    {
        private readonly RedactAttribute _redactAttribute;
        private readonly IEnumerable<Type> _enumExtraTypes;

        public EnumTypeValueConverter(RedactAttribute redactAttribute, IEnumerable<Type> extraTypes)
        {
            _redactAttribute = redactAttribute;
            _enumExtraTypes = extraTypes.Where(t => t.IsEnum).ToList();
        }

        public object ParseString(string value)
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
            if (value == null)
            {
                return null;
            }

            var enumValue =
                _redactAttribute != null
                    ? _redactAttribute.Redact((Enum)value, options.ShouldRedact)
                    : value.ToString();

            return value.GetType().Name + "." + enumValue;
        }
    }
}