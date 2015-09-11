using System;
using System.Reflection;

namespace XSerializer
{
    internal class SerializableJsonProperty
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly Lazy<IJsonSerializerInternal> _serializer;

        public SerializableJsonProperty(PropertyInfo propertyInfo, bool encrypt)
        {
            _propertyInfo = propertyInfo;
            _serializer = new Lazy<IJsonSerializerInternal>(() => JsonSerializerFactory.GetSerializer(_propertyInfo.PropertyType, encrypt));
        }

        public void WriteValue(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            var value = _propertyInfo.GetValue(instance, null);
            writer.WriteValue(_propertyInfo.Name);
            writer.WriteNameValueSeparator();
            _serializer.Value.SerializeObject(writer, value, info);
        }
    }
}