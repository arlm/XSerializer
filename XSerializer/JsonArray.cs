using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace XSerializer
{
    public sealed class JsonArray : DynamicObject, IEnumerable<object>
    {
        private delegate bool TryFunc(out object result);

        private static readonly ConcurrentDictionary<Type, TryFunc> _convertFuncs = new ConcurrentDictionary<Type, TryFunc>();

        private readonly IDateTimeHandler _dateTimeHandler;
        private readonly List<object> _values = new List<object>();
        private readonly List<object> _transformableValues = new List<object>();
        private readonly List<Type> _transformedTypes = new List<Type>();

        public JsonArray()
            : this(DateTimeHandler.Default)
        {
        }

        public JsonArray(IDateTimeHandler dateTimeHandler)
        {
            _dateTimeHandler = dateTimeHandler;
        }

        public void Add(object value)
        {
            if (value == null
                || value is bool
                || value is JsonArray
                || value is JsonObject)
            {
                _values.Add(value);
                _transformableValues.Add(null);
            }
            else if (value is string)
            {
                _values.Add(value);
                _transformableValues.Add(value);
            }
            else
            {
                var jsonNumber = value as JsonNumber;
                if (jsonNumber != null)
                {
                    _values.Add(jsonNumber.DoubleValue);
                    _transformableValues.Add(jsonNumber);
                }
                else
                {
                    throw new ArgumentException("Invalid value type: " + value.GetType(), "value");
                }
            }
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var convertFunc = _convertFuncs.GetOrAdd(
                binder.Type,
                t =>
                {
                    if (t.IsInterface && t.IsGenericType)
                    {
                        var genericTypeDefinition = t.GetGenericTypeDefinition();

                        if (genericTypeDefinition == typeof(IEnumerable<>)
                            || genericTypeDefinition == typeof(ICollection<>)
                            || genericTypeDefinition == typeof(IList<>))
                        {
                            var collectionType = t.GetGenericArguments()[0];

                            if (collectionType == typeof(object))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<object>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(JsonObject))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<JsonObject>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(JsonArray))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<JsonArray>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(bool))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<bool>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(bool?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<bool?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(byte))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<byte>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(byte?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<byte?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(sbyte))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<sbyte>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(sbyte?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<sbyte?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(short))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<short>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(short?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<short?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(ushort))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<ushort>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(ushort?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<ushort?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(int))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<int>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(int?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<int?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(uint))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<uint>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(uint?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<uint?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(long))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<long>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(long?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<long?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(ulong))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<ulong>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(ulong?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<ulong?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(float))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<float>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(float?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<float?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(double))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<double>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(double?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<double?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(decimal))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<decimal>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(decimal?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<decimal?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(string))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<string>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(DateTime))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<DateTime>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(DateTime?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<DateTime?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(DateTimeOffset))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<DateTimeOffset>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(DateTimeOffset?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<DateTimeOffset?>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(Guid))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<Guid>)this;
                                    return true;
                                });
                            }

                            if (collectionType == typeof(Guid?))
                            {
                                return ((out object r) =>
                                {
                                    r = (List<Guid?>)this;
                                    return true;
                                });
                            }
                        }
                    }

                    return null;
                });

            return convertFunc != null
                ? convertFunc(out result)
                : base.TryConvert(binder, out result);
        }

        public object this[int index]
        {
            get { return _values[index]; }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public JsonArray TransformItems<T>()
        {
            return TransformItems(typeof(T));
        }

        public JsonArray TransformItems(Type type)
        {
            if (_transformedTypes.Contains(type))
            {
                return this;
            }

            if (type.IsNullableType())
            {
                var nullableOfType = type.GetGenericArguments()[0];
                if (_transformedTypes.Contains(nullableOfType))
                {
                    return this;
                }

                _transformedTypes.Add(nullableOfType);
            }
            else
            {
                _transformedTypes.Add(type);
            }

            var transform = GetTransform(type);

            // TODO: Add additional transform functions.

            for (int i = 0; i < _values.Count; i++)
            {
                if (_transformableValues[i] != null)
                {
                    _values[i] = transform(_values[i], _transformableValues[i]);
                }
            }

            return this;
        }

        private Func<object, object, object> GetTransform(Type type)
        {
            Func<object, object, object> transform = (currentItem, transformableValue) => currentItem;

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                transform = TransformDateTime;
            }

            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                transform = TransformDateTimeOffset;
            }

            if (type == typeof(Guid) || type == typeof(Guid?))
            {
                transform = TransformGuid;
            }

            if (type == typeof(byte) || type == typeof(byte?))
            {
                transform = TransformByte;
            }

            if (type == typeof(sbyte) || type == typeof(sbyte?))
            {
                transform = TransformSByte;
            }

            if (type == typeof(short) || type == typeof(short?))
            {
                transform = TransformInt16;
            }

            if (type == typeof(ushort) || type == typeof(ushort?))
            {
                transform = TransformUInt16;
            }

            if (type == typeof(int) || type == typeof(int?))
            {
                transform = TransformInt32;
            }

            if (type == typeof(uint) || type == typeof(uint?))
            {
                transform = TransformUInt32;
            }

            if (type == typeof(long) || type == typeof(long?))
            {
                transform = TransformInt64;
            }

            if (type == typeof(ulong) || type == typeof(ulong?))
            {
                transform = TransformUInt64;
            }

            if (type == typeof(float) || type == typeof(float?))
            {
                transform = TransformSingle;
            }

            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                transform = TransformDecimal;
            }

            return transform;
        }

        private object TransformDateTime(object currentItem, object transformableValue)
        {
            if (transformableValue is JsonNumber)
            {
                return currentItem;
            }

            try
            {
                return _dateTimeHandler.ParseDateTime((string)transformableValue);
            }
            catch
            {
                return currentItem;
            }
        }

        private object TransformDateTimeOffset(object currentItem, object transformableValue)
        {
            if (transformableValue is JsonNumber)
            {
                return currentItem;
            }

            try
            {
                return _dateTimeHandler.ParseDateTimeOffset((string)transformableValue);
            }
            catch
            {
                return currentItem;
            }
        }

        private static object TransformGuid(object currentItem, object transformableValue)
        {
            if (transformableValue is JsonNumber)
            {
                return currentItem;
            }

            Guid value;
            return Guid.TryParse((string)transformableValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformByte(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            byte value;
            return byte.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformSByte(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            sbyte value;
            return sbyte.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformInt16(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            short value;
            return short.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformUInt16(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            ushort value;
            return ushort.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformInt32(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            int value;
            return int.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformUInt32(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            uint value;
            return uint.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformInt64(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            long value;
            return long.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformUInt64(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            ulong value;
            return ulong.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformSingle(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            float value;
            return float.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        private static object TransformDecimal(object currentItem, object transformableValue)
        {
            if (transformableValue is string)
            {
                return currentItem;
            }

            decimal value;
            return decimal.TryParse(((JsonNumber)transformableValue).StringValue, out value)
                ? value
                : currentItem;
        }

        public static implicit operator List<object>(JsonArray jsonArray)
        {
            return jsonArray._values;
        }

        public static implicit operator object[](JsonArray jsonArray)
        {
            return jsonArray._values.ToArray();
        }

        public static implicit operator List<JsonObject>(JsonArray jsonArray)
        {
            return jsonArray._values.Cast<JsonObject>().ToList();
        }

        public static implicit operator JsonObject[](JsonArray jsonArray)
        {
            return jsonArray._values.Cast<JsonObject>().ToArray();
        }

        public static implicit operator List<JsonArray>(JsonArray jsonArray)
        {
            return jsonArray._values.Cast<JsonArray>().ToList();
        }

        public static implicit operator JsonArray[](JsonArray jsonArray)
        {
            return jsonArray._values.Cast<JsonArray>().ToArray();
        }

        public static implicit operator List<string>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<string>()._values.Cast<string>().ToList();
        }

        public static implicit operator string[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<string>()._values.Cast<string>().ToArray();
        }

        public static implicit operator List<DateTime>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTime>()._values.Cast<DateTime>().ToList();
        }

        public static implicit operator DateTime[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTime>()._values.Cast<DateTime>().ToArray();
        }

        public static implicit operator List<DateTime?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTime?>()._values.Cast<DateTime?>().ToList();
        }

        public static implicit operator DateTime?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTime?>()._values.Cast<DateTime?>().ToArray();
        }

        public static implicit operator List<DateTimeOffset>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTimeOffset>()._values.Cast<DateTimeOffset>().ToList();
        }

        public static implicit operator DateTimeOffset[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTimeOffset>()._values.Cast<DateTimeOffset>().ToArray();
        }

        public static implicit operator List<DateTimeOffset?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTimeOffset?>()._values.Cast<DateTimeOffset?>().ToList();
        }

        public static implicit operator DateTimeOffset?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTimeOffset?>()._values.Cast<DateTimeOffset?>().ToArray();
        }

        public static implicit operator List<Guid>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<Guid>()._values.Cast<Guid>().ToList();
        }

        public static implicit operator Guid[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<Guid>()._values.Cast<Guid>().ToArray();
        }

        public static implicit operator List<Guid?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<Guid?>()._values.Cast<Guid?>().ToList();
        }

        public static implicit operator Guid?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<Guid?>()._values.Cast<Guid?>().ToArray();
        }

        public static implicit operator List<bool>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<bool>()._values.Cast<bool>().ToList();
        }

        public static implicit operator bool[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<bool>()._values.Cast<bool>().ToArray();
        }

        public static implicit operator List<bool?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<bool?>()._values.Cast<bool?>().ToList();
        }

        public static implicit operator bool?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<bool?>()._values.Cast<bool?>().ToArray();
        }

        public static implicit operator List<byte>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<byte>()._values.Cast<byte>().ToList();
        }

        public static implicit operator byte[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<byte>()._values.Cast<byte>().ToArray();
        }

        public static implicit operator List<byte?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<byte?>()._values.Cast<byte?>().ToList();
        }

        public static implicit operator byte?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<byte?>()._values.Cast<byte?>().ToArray();
        }

        public static implicit operator List<sbyte>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<sbyte>()._values.Cast<sbyte>().ToList();
        }

        public static implicit operator sbyte[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<sbyte>()._values.Cast<sbyte>().ToArray();
        }

        public static implicit operator List<sbyte?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<sbyte?>()._values.Cast<sbyte?>().ToList();
        }

        public static implicit operator sbyte?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<sbyte?>()._values.Cast<sbyte?>().ToArray();
        }

        public static implicit operator List<short>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<short>()._values.Cast<short>().ToList();
        }

        public static implicit operator short[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<short>()._values.Cast<short>().ToArray();
        }

        public static implicit operator List<short?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<short?>()._values.Cast<short?>().ToList();
        }

        public static implicit operator short?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<short?>()._values.Cast<short?>().ToArray();
        }

        public static implicit operator List<ushort>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ushort>()._values.Cast<ushort>().ToList();
        }

        public static implicit operator ushort[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ushort>()._values.Cast<ushort>().ToArray();
        }

        public static implicit operator List<ushort?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ushort?>()._values.Cast<ushort?>().ToList();
        }

        public static implicit operator ushort?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ushort?>()._values.Cast<ushort?>().ToArray();
        }

        public static implicit operator List<int>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<int>()._values.Cast<int>().ToList();
        }

        public static implicit operator int[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<int>()._values.Cast<int>().ToArray();
        }

        public static implicit operator List<int?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<int?>()._values.Cast<int?>().ToList();
        }

        public static implicit operator int?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<int?>()._values.Cast<int?>().ToArray();
        }

        public static implicit operator List<uint>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<uint>()._values.Cast<uint>().ToList();
        }

        public static implicit operator uint[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<uint>()._values.Cast<uint>().ToArray();
        }

        public static implicit operator List<uint?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<uint?>()._values.Cast<uint?>().ToList();
        }

        public static implicit operator uint?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<uint?>()._values.Cast<uint?>().ToArray();
        }

        public static implicit operator List<long>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<long>()._values.Cast<long>().ToList();
        }

        public static implicit operator long[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<long>()._values.Cast<long>().ToArray();
        }

        public static implicit operator List<long?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<long?>()._values.Cast<long?>().ToList();
        }

        public static implicit operator long?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<long?>()._values.Cast<long?>().ToArray();
        }

        public static implicit operator List<ulong>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ulong>()._values.Cast<ulong>().ToList();
        }

        public static implicit operator ulong[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ulong>()._values.Cast<ulong>().ToArray();
        }

        public static implicit operator List<ulong?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ulong?>()._values.Cast<ulong?>().ToList();
        }

        public static implicit operator ulong?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ulong?>()._values.Cast<ulong?>().ToArray();
        }

        public static implicit operator List<float>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<float>()._values.Cast<float>().ToList();
        }

        public static implicit operator float[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<float>()._values.Cast<float>().ToArray();
        }

        public static implicit operator List<float?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<float?>()._values.Cast<float?>().ToList();
        }

        public static implicit operator float?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<float?>()._values.Cast<float?>().ToArray();
        }

        public static implicit operator List<double>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<double>()._values.Cast<double>().ToList();
        }

        public static implicit operator double[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<double>()._values.Cast<double>().ToArray();
        }

        public static implicit operator List<double?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<double?>()._values.Cast<double?>().ToList();
        }

        public static implicit operator double?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<double?>()._values.Cast<double?>().ToArray();
        }

        public static implicit operator List<decimal>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<decimal>()._values.Cast<decimal>().ToList();
        }

        public static implicit operator decimal[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<decimal>()._values.Cast<decimal>().ToArray();
        }

        public static implicit operator List<decimal?>(JsonArray jsonArray)
        {
            return jsonArray.TransformItems<decimal?>()._values.Cast<decimal?>().ToList();
        }

        public static implicit operator decimal?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<decimal?>()._values.Cast<decimal?>().ToArray();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}