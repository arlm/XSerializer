using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace XSerializer
{
    internal static class JsonSerializerFactory
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool, JsonConcreteImplementations>, IJsonSerializerInternal> _cache =
            new ConcurrentDictionary<Tuple<Type, bool, JsonConcreteImplementations>, IJsonSerializerInternal>();

        public static IJsonSerializerInternal GetSerializer(
            Type type,
            bool encrypt,
            IDictionary<Type, Type> concreteImplementationsByType,
            IDictionary<PropertyInfo, Type> concreteImplementationsByProperty)
        {
            return GetSerializer(
                type, encrypt,
                new JsonConcreteImplementations(concreteImplementationsByType, concreteImplementationsByProperty));
        }

        public static IJsonSerializerInternal GetSerializer(
            Type type,
            bool encrypt,
            JsonConcreteImplementations concreteImplementations)
        {
            return _cache.GetOrAdd(
                Tuple.Create(type, encrypt, concreteImplementations),
                tuple =>
                {
                    if (type == typeof(object))
                    {
                        return DynamicJsonSerializer.Get(encrypt, concreteImplementations);
                    }

                    if (type == typeof(string)
                        || type == typeof(DateTime)
                        || type == typeof(DateTime?)
                        || type == typeof(DateTimeOffset)
                        || type == typeof(DateTimeOffset?)
                        || type == typeof(Guid)
                        || type == typeof(Guid?))
                    {
                        return StringJsonSerializer.Get(type, encrypt);
                    }

                    if (type == typeof(double)
                        || type == typeof(double?)
                        || type == typeof(int)
                        || type == typeof(int?)
                        || type == typeof(float)
                        || type == typeof(float?)
                        || type == typeof(long)
                        || type == typeof(long?)
                        || type == typeof(decimal)
                        || type == typeof(decimal?)
                        || type == typeof(byte)
                        || type == typeof(byte?)
                        || type == typeof(sbyte)
                        || type == typeof(sbyte?)
                        || type == typeof(short)
                        || type == typeof(short?)
                        || type == typeof(ushort)
                        || type == typeof(ushort?)
                        || type == typeof(uint)
                        || type == typeof(uint?)
                        || type == typeof(ulong)
                        || type == typeof(ulong?)) // TODO: handle more number types.
                    {
                        return NumberJsonSerializer.Get(type, encrypt);
                    }

                    if (type == typeof(bool)
                        || type == typeof(bool?))
                    {
                        return BooleanJsonSerializer.Get(encrypt, type == typeof(bool?));
                    }

                    if (type.IsAssignableToGenericIDictionary()
                        || typeof(IDictionary).IsAssignableFrom(type))
                    {
                        return DictionaryJsonSerializer.Get(type, encrypt, concreteImplementations);
                    }

                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        return ListJsonSerializer.Get(type, encrypt, concreteImplementations);
                    }

                    // TODO: Handle more types or possibly black-list some types or types of types.

                    return CustomJsonSerializer.Get(type, encrypt, concreteImplementations);
                });
        }
    }
}