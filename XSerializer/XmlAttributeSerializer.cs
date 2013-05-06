using System;
using System.Globalization;
using System.Xml;

namespace XSerializer
{
    public class XmlAttributeSerializer : IXmlSerializer
    {
        private readonly Type _type;
        private readonly string _attributeName;
        private readonly Func<string, object> _parseString;
        private readonly Func<object, ISerializeOptions, string> _getString; 

        public XmlAttributeSerializer(Type type, string attributeName, RedactAttribute redactAttribute)
        {
            _attributeName = attributeName;
            _type = type;

            if (redactAttribute != null)
            {
                _parseString = GetRedactedGetParseStringFunc(_type);
                _getString = GetRedactedGetStringFunc(_type, redactAttribute);
            }
            else
            {
                _parseString = GetNonRedactedGetParseStringFunc(_type);
                _getString = GetNonRedactedGetStringFunc(_type);
            }
        }

        public void SerializeObject(SerializationXmlTextWriter writer, object value, ISerializeOptions options)
        {
            if (value != null)
            {
                writer.WriteAttributeString(_attributeName, _getString(value, options));
            }
        }

        public object DeserializeObject(XmlReader reader)
        {
            if (reader.MoveToAttribute(_attributeName))
            {
                var value = _parseString(reader.Value);
                reader.MoveToElement();
                return value;
            }

            return null;
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
                return value =>
                {
                    if (value == "XXXXXX")
                    {
                        return defaultValue;
                    }

                    return Enum.Parse(type, value);
                };
            }
            
            if (type == typeof(bool) || type == typeof(bool?))
            {
                return value =>
                {
                    if (value == "XXXXXX")
                    {
                        return defaultValue;
                    }

                    return Convert.ChangeType(value, type);
                };
            }
            
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return value =>
                    DateTime.ParseExact(
                        value,
                        "O",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind);
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
                return value => Enum.Parse(type, value);
            }

            return value => Convert.ChangeType(value, type);
        }

        private static Func<object, ISerializeOptions, string> GetNonRedactedGetStringFunc(Type type)
        {
            if (type == typeof(bool))
            {
                return (value, options) => value.ToString().ToLower();
            }

            return (value, options) => value.ToString();
        }
    }
}