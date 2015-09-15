using System;
using System.Reflection;

namespace XSerializer
{
    internal class SerializableJsonProperty
    {
        private readonly string _name;
        private readonly Lazy<IJsonSerializerInternal> _serializer;
        private readonly Func<object, object> _getValue;
        private readonly Action<object, object> _setValue;

        public SerializableJsonProperty(PropertyInfo propertyInfo, bool encrypt)
        {
            _name = propertyInfo.Name;
            _serializer = new Lazy<IJsonSerializerInternal>(() => JsonSerializerFactory.GetSerializer(propertyInfo.PropertyType, encrypt));

            // TODO: Optimize these delegates.
            _getValue = instance => propertyInfo.GetValue(instance, null);
            _setValue = (instance, value) => propertyInfo.SetValue(instance, value, null);
        }

        public string Name
        {
            get { return _name; }
        }

        public void WriteValue(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            var value = _getValue(instance);
            writer.WriteValue(Name);
            writer.WriteNameValueSeparator();
            _serializer.Value.SerializeObject(writer, value, info);
        }

        public object ReadValue(JsonReader reader, IJsonSerializeOperationInfo info)
        {
            return _serializer.Value.DeserializeObject(reader, info);
        }

        public void SetValue(object instance, JsonReader reader, IJsonSerializeOperationInfo info)
        {
            var value = ReadValue(reader, info);
            _setValue(instance, value);
        }
    }
}