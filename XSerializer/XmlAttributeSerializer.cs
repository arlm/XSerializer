using System;
using System.Xml;

namespace XSerializer
{
    public class XmlAttributeSerializer : IXmlSerializer
    {
        private readonly Type _type;
        private readonly string _attributeName;
        private readonly RedactAttribute _redactAttribute;
        private readonly Func<string, object> _parseString;
        private readonly Func<object, ISerializeOptions, string> _getString; 

        public XmlAttributeSerializer(Type type, string attributeName, RedactAttribute redactAttribute)
        {
            _attributeName = attributeName;
            _redactAttribute = redactAttribute;
            _type = type;

            if (redactAttribute == null)
            {
                if (_type.IsEnum)
                {
                    _parseString = value => Enum.Parse(_type, value);
                }
                else
                {
                    _parseString = value => Convert.ChangeType(value, _type);
                }

                if (_type == typeof(bool))
                {
                    _getString = (value, options) => value.ToString().ToLower();
                }
                else
                {
                    _getString = (value, options) => value.ToString();
                }
            }
            else
            {
                object defaultValue;
                if (_type.IsValueType)
                {
                    defaultValue = Activator.CreateInstance(_type);
                }
                else
                {
                    defaultValue = null;
                }

                if (_type.IsEnum)
                {
                    _parseString = value =>
                    {
                        if (value == "XXXXXX")
                        {
                            return defaultValue;
                        }

                        return Enum.Parse(_type, value);
                    };
                }
                else if (_type == typeof(bool))
                {
                    _parseString = value =>
                        {
                            if (value == "XXXXXX")
                            {
                                return defaultValue;
                            }

                            return Convert.ChangeType(value, _type);
                        };
                }
                else
                {
                    _parseString = value => Convert.ChangeType(value, _type);
                }

                if (_type == typeof(string))
                {
                    _getString = (value, options) => _redactAttribute.Redact((string)value, options.ShouldRedact);
                }
                else if (_type == typeof(bool) || type == typeof(bool?))
                {
                    _getString = (value, options) => _redactAttribute.Redact((bool?)value, options.ShouldRedact);
                }
                else if (_type.IsEnum ||
                    (_type.IsGenericType
                        && _type.GetGenericTypeDefinition() == typeof(Nullable<>)
                        && _type.GetGenericArguments()[0].IsEnum))
                {
                    _getString = (value, options) => _redactAttribute.Redact((Enum)value, options.ShouldRedact);
                }
                else if (_type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    _getString = (value, options) => _redactAttribute.Redact((DateTime?)value, options.ShouldRedact);
                }
                else
                {
                    _getString = (value, options) => _redactAttribute.Redact(value, options.ShouldRedact);
                }
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
    }
}