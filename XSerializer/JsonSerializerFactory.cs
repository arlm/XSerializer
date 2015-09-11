using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Xml;
using XSerializer.Encryption;

namespace XSerializer
{
    internal static class JsonSerializerFactory
    {
        private static readonly ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal> _cache = new ConcurrentDictionary<Tuple<Type, bool>, IJsonSerializerInternal>();

        public static IJsonSerializerInternal GetSerializer(Type type, EncryptAttribute encryptAttribute)
        {
            return _cache.GetOrAdd(
                Tuple.Create(type, encryptAttribute != null),
                _ =>
                {
                    // TODO: Implement
                    return null;
                });
        }
    }
}