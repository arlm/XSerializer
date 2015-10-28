using System;
using System.Linq.Expressions;
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
            if (propertyInfo.DeclaringType == null)
            {
                throw new ArgumentException("The DeclaringType of the PropertyInfo must not be null.", "propertyInfo");
            }

            var jsonPropertyAttribute = (JsonPropertyAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(JsonPropertyAttribute));

            _name = jsonPropertyAttribute.GetNameOrDefaultTo(propertyInfo.Name);
            _serializer = new Lazy<IJsonSerializerInternal>(() => JsonSerializerFactory.GetSerializer(propertyInfo.PropertyType, encrypt));

            _getValue = GetGetValueFunc(propertyInfo, propertyInfo.DeclaringType);

            if (propertyInfo.CanWrite)
            {
                _setValue = GetSetValueAction(propertyInfo, propertyInfo.DeclaringType);
            }
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

        public void SetValue(object instance, object value)
        {
            _setValue(instance, value);
        }

        private static Func<object, object> GetGetValueFunc(PropertyInfo propertyInfo, Type declaringType)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");

            Expression property = Expression.Property(Expression.Convert(instanceParameter, declaringType), propertyInfo);

            if (propertyInfo.PropertyType.IsValueType) // Needs boxing.
            {
                property = Expression.Convert(property, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object, object>>(property, new[] { instanceParameter });
            return lambda.Compile();
        }

        private static Action<object, object> GetSetValueAction(PropertyInfo propertyInfo, Type declaringType)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            Expression property = Expression.Property(Expression.Convert(instanceParameter, declaringType), propertyInfo);
            var assign = Expression.Assign(property, Expression.Convert(valueParameter, propertyInfo.PropertyType));

            var lambda = Expression.Lambda<Action<object, object>>(assign, new[] { instanceParameter, valueParameter });
            return lambda.Compile();
        }
    }
}