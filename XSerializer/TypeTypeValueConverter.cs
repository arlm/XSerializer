using System;

namespace XSerializer
{
    internal class TypeTypeValueConverter : IValueConverter
    {
        private readonly RedactAttribute _redactAttribute;

        public TypeTypeValueConverter(RedactAttribute redactAttribute)
        {
            _redactAttribute = redactAttribute;
        }

        public object ParseString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return Type.GetType(value);
        }

        public string GetString(object value, ISerializeOptions options)
        {
            var type = value as Type;

            if (type == null)
            {
                return null;
            }

            var typeString =
                _redactAttribute == null
                    ? GetStringValue(type)
                    : _redactAttribute.Redact(type, options.ShouldRedact);

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