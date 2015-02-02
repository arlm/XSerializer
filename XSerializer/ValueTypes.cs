using System;
using System.Collections.Generic;
using System.Linq;

namespace XSerializer
{
    internal static class ValueTypes
    {
        private static readonly Dictionary<Type, Func<RedactAttribute, Type[], IValueConverter>> _factories = new Dictionary<Type, Func<RedactAttribute, Type[], IValueConverter>>();
        private static readonly Dictionary<Type, Func<bool, object, string>> _redactFuncs = new Dictionary<Type, Func<bool, object, string>>();

        static ValueTypes()
        {
            _factories.Add(typeof(Enum), (redactAttribute, extraTypes) => new EnumTypeValueConverter(redactAttribute, extraTypes));
            _factories.Add(typeof(Type), (redactAttribute, extraTypes) => new TypeTypeValueConverter(redactAttribute));
            _factories.Add(typeof(Uri), (redactAttribute, extraTypes) => new UriTypeValueConverter(redactAttribute));

            _redactFuncs.Add(typeof(Enum), GetRedactFunc(EnumTypeValueConverter.Default));
            _redactFuncs.Add(typeof(Type), GetRedactFunc(TypeTypeValueConverter.Default));
            _redactFuncs.Add(typeof(Uri), GetRedactFunc(UriTypeValueConverter.Default));

            if (_factories.Count != _redactFuncs.Count || _factories.Keys.Any(key => !_redactFuncs.ContainsKey(key)))
            {
                throw new InvalidOperationException("Both dictionaries in the ValueTypes type must have identical keys.");
            }
        }

        public static bool TryGetValueConverter(Type type, RedactAttribute redactAttribute, Type[] extraTypes, out IValueConverter valueConverter)
        {
            Func<RedactAttribute, Type[], IValueConverter> factory;

            if (_factories.TryGetValue(type, out factory))
            {
                valueConverter = factory(redactAttribute, extraTypes);
                return true;
            }

            valueConverter = null;
            return false;
        }

        public static bool IsRegistered(Type type)
        {
            return _factories.ContainsKey(type);
        }

        public static bool TryRedact(RedactAttribute redactAttribute, object value, bool redactEnabled, out string redactedValue)
        {
            if (value == null)
            {
                redactedValue = null;
                return true;
            }

            Func<bool, object, string> redactFunc;
            if (_redactFuncs.TryGetValue(GetRedactFuncKey(value.GetType()), out redactFunc))
            {
                redactedValue = redactFunc(redactEnabled, value);
                return true;
            }

            redactedValue = null;
            return false;
        }

        private static Type GetRedactFuncKey(Type type)
        {
            return type.IsEnum ? typeof(Enum) : type;
        }

        private static Func<bool, object, string> GetRedactFunc(IValueConverter valueConverter)
        {
            return
                (redactEnabled, value) =>
                    redactEnabled
                        ? "XXXXXXXXXX"
                        : valueConverter.GetString(value, null);
        }
    }
}