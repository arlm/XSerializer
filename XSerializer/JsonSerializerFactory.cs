using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace XSerializer
{
    internal static class JsonSerializerFactory
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool, JsonMappings>, IJsonSerializerInternal> _cache =
            new ConcurrentDictionary<Tuple<Type, bool, JsonMappings>, IJsonSerializerInternal>();

        public static IJsonSerializerInternal GetSerializer(
            Type type,
            bool encrypt,
            IDictionary<Type, Type> mappingsByType,
            IDictionary<PropertyInfo, Type> mappingsByProperty)
        {
            return GetSerializer(
                type, encrypt,
                new JsonMappings(mappingsByType, mappingsByProperty));
        }

        public static IJsonSerializerInternal GetSerializer(
            Type type,
            bool encrypt,
            JsonMappings mappings)
        {
            return _cache.GetOrAdd(
                Tuple.Create(type, encrypt, mappings),
                tuple =>
                {
                    if (type == typeof(object))
                    {
                        return DynamicJsonSerializer.Get(encrypt, mappings);
                    }

                    if (type == typeof(string)
                        || type == typeof(DateTime)
                        || type == typeof(DateTime?)
                        || type == typeof(DateTimeOffset)
                        || type == typeof(DateTimeOffset?)
                        || type == typeof(TimeSpan)
                        || type == typeof(TimeSpan?)
                        || type == typeof(Guid)
                        || type == typeof(Guid?)
                        || type.IsEnum
                        || (type.IsNullableType() && Nullable.GetUnderlyingType(type).IsEnum)
                        || type == typeof(Type)
                        || type == typeof(Uri))
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
                        return DictionaryJsonSerializer.Get(type, encrypt, mappings);
                    }

                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        return ListJsonSerializer.Get(type, encrypt, mappings);
                    }

                    // TODO: Handle more types or possibly black-list some types or types of types.

                    return CustomJsonSerializer.Get(type, encrypt, mappings);
                });
        }
    }
}