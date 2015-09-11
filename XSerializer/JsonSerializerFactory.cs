using System;
using System.Collections;
using System.Collections.Concurrent;

namespace XSerializer
{
    internal static class JsonSerializerFactory
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal> _cache = new ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal>();

        public static IJsonSerializerInternal GetSerializer(Type type, bool encrypt)
        {
            return _cache.GetOrAdd(
                Tuple.Create(type, encrypt),
                _ =>
                {
                    if (type == typeof(object))
                    {
                        return DynamicJsonSerializer.Get(encrypt);
                    }

                    if (type == typeof(string))
                    {
                        return StringJsonSerializer.Get(encrypt);
                    }

                    if (type == typeof(double)) // TODO: handler more number types.
                    {
                        return NumberJsonSerializer.Get(encrypt);
                    }

                    if (type == typeof(bool))
                    {
                        return BooleanJsonSerializer.Get(encrypt);
                    }

                    if (type.IsAssignableToGenericIDictionaryOfStringToAnything())
                    {
                        return DictionaryJsonSerializer.Get(type, encrypt);
                    }

                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        return ListJsonSerializer.Get(type, encrypt);
                    }

                    // TODO: Handle more types or possibly black-list some types or types of types.

                    return CustomJsonSerializer.Get(type, encrypt);
                });
        }
    }
}