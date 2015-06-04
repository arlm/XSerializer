using System;
using System.Linq.Expressions;
using System.Reflection;

namespace XSerializer.Tests.Encryption
{
    internal static class SerializationStateExtensions
    {
        private static readonly Func<SerializationState, object> _getRawValue;

        static SerializationStateExtensions()
        {
            var parameter = Expression.Parameter(typeof(SerializationState), "serializationState");

            var fieldInfo = typeof(SerializationState).GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);

            var lambda = Expression.Lambda<Func<SerializationState, object>>(
                Expression.Field(parameter, fieldInfo),
                new[] { parameter });

            _getRawValue = lambda.Compile();
        }

        public static object GetRawValue(this SerializationState serializationState)
        {
            return _getRawValue(serializationState);
        }
    }
}