using System;
using System.Collections.Generic;
using System.Globalization;

namespace XSerializer
{
    public class SimpleTypeValueConverter
    {
        private static readonly Dictionary<int, SimpleTypeValueConverter> Map = new Dictionary<int, SimpleTypeValueConverter>();

        private readonly Func<string, object> _parseString;
        private readonly Func<object, ISerializeOptions, string> _getString; 

        private SimpleTypeValueConverter(Type type, RedactAttribute redactAttribute)
        {
            if (redactAttribute != null)
            {
                _parseString = GetRedactedGetParseStringFunc(type);
                _getString = GetRedactedGetStringFunc(type, redactAttribute);
            }
            else
            {
                _parseString = GetNonRedactedGetParseStringFunc(type);
                _getString = GetNonRedactedGetStringFunc(type);
            }
        }

        public static SimpleTypeValueConverter Create(Type type, RedactAttribute redactAttribute)
        {
            SimpleTypeValueConverter converter;

            var key = CreateKey(type, redactAttribute);

            if (!Map.TryGetValue(key, out converter))
            {
                converter = new SimpleTypeValueConverter(type, redactAttribute);
                Map[key] = converter;
            }

            return converter;
        }

        public object ParseString(string value)
        {
            return _parseString(value);
        }

        public string GetString(object value, ISerializeOptions options)
        {
            return _getString(value, options);
        }

        private static Func<string, object> GetRedactedGetParseStringFunc(Type type)
        {
            var defaultValue =
                type.IsValueType
                ? Activator.CreateInstance(type)
                : null;

            if (type.IsEnum ||
                (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments()[0].IsEnum))
            {
                return value => value == null || value == "XXXXXX" ? defaultValue : Enum.Parse(type, value);
            }

            if (type == typeof(bool) || type == typeof(bool?))
            {
                return value => value == null || value == "XXXXXX" ? defaultValue : Convert.ChangeType(value, type);
            }

            if (type == typeof(DateTime))
            {
                return ParseStringForDateTime;
            }

            if (type == typeof(DateTime?))
            {
                return ParseStringForNullableDateTime;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return value => Convert.ChangeType(value, type.GetGenericArguments()[0]);
            }

            return value => Convert.ChangeType(value, type);
        }

        private static Func<object, ISerializeOptions, string> GetRedactedGetStringFunc(Type type, RedactAttribute redactAttribute)
        {
            if (type == typeof(string))
            {
                return (value, options) => redactAttribute.Redact((string)value, options.ShouldRedact);
            }

            if (type == typeof(bool) || type == typeof(bool?))
            {
                return (value, options) => redactAttribute.Redact((bool?)value, options.ShouldRedact);
            }

            if (type.IsEnum ||
                (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments()[0].IsEnum))
            {
                return (value, options) => redactAttribute.Redact((Enum)value, options.ShouldRedact);
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return (value, options) => redactAttribute.Redact((DateTime?)value, options.ShouldRedact);
            }

            return (value, options) => redactAttribute.Redact(value, options.ShouldRedact);
        }

        private static Func<string, object> GetNonRedactedGetParseStringFunc(Type type)
        {
            if (type.IsEnum)
            {
                var defaultValue = Activator.CreateInstance(type);
                return value => value == null ? defaultValue : Enum.Parse(type, value);
            }

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && type.GetGenericArguments()[0].IsEnum)
            {
                return value => value == null ? null : Enum.Parse(type, value);
            }

            if (type == typeof(DateTime))
            {
                return ParseStringForDateTime;
            }

            if (type == typeof(DateTime?))
            {
                return ParseStringForNullableDateTime;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return value => Convert.ChangeType(value, type.GetGenericArguments()[0]);
            }

            return value => Convert.ChangeType(value, type);
        }

        private static Func<object, ISerializeOptions, string> GetNonRedactedGetStringFunc(Type type)
        {
            if (type == typeof(bool))
            {
                return GetStringFromBool;
            }

            if (type == typeof(bool?))
            {
                return GetStringFromNullableBool;
            }

            if (type == typeof(DateTime))
            {
                return GetStringFromDateTime;
            }

            if (type == typeof(DateTime?))
            {
                return GetStringFromNullableDateTime;
            }

            return (value, options) => value.ToString();
        }

        private static object ParseStringForDateTime(string value)
        {
            if (value == null)
            {
                return new DateTime();
            }

            return DateTime.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        private static object ParseStringForNullableDateTime(string value)
        {
            if (value == null)
            {
                return null;
            }

            return DateTime.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        private static string GetStringFromBool(object value, ISerializeOptions options)
        {
            return value.ToString().ToLower();
        }

        private static string GetStringFromNullableBool(object value, ISerializeOptions options)
        {
            return value == null ? null : GetStringFromBool(value, options);
        }

        private static string GetStringFromDateTime(object value, ISerializeOptions options)
        {
            var dateTime = (DateTime)value;
            return dateTime.ToString("O");
        }

        private static string GetStringFromNullableDateTime(object value, ISerializeOptions options)
        {
            return value == null ? null : GetStringFromDateTime(value, options);
        }

        private static int CreateKey(Type type, RedactAttribute redactAttribute)
        {
            unchecked
            {
                var key = type.GetHashCode();

                if (redactAttribute != null)
                {
                    key = (key * 397) ^ redactAttribute.GetHashCode();
                }

                return key;
            }
        }
    }
}