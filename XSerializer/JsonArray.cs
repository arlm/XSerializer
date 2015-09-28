using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    public sealed class JsonArray : DynamicObject, IEnumerable<object>
    {
        private delegate bool TryFunc(out object result);

        private readonly ConcurrentDictionary<Type, TryFunc> _convertFuncs = new ConcurrentDictionary<Type, TryFunc>();

        private readonly IJsonSerializeOperationInfo _info;

        private readonly List<object> _values = new List<object>();
        private readonly List<object> _transformableValues = new List<object>();
        private readonly List<Type> _transformedTypes = new List<Type>();

        public JsonArray(
            IDateTimeHandler dateTimeHandler = null,
            IEncryptionMechanism encryptionMechanism = null,
            object encryptKey = null,
            SerializationState serializationState = null)
            : this(new JsonSerializeOperationInfo
                {
                    DateTimeHandler = dateTimeHandler ?? DateTimeHandler.Default,
                    EncryptionMechanism = encryptionMechanism,
                    EncryptKey = encryptKey,
                    SerializationState = serializationState
                })
        {
        }

        internal JsonArray(IJsonSerializeOperationInfo info)
        {
            _info = info;
        }

        public void Add(object value)
        {
            var jsonNumber = value as JsonNumber;
            if (jsonNumber != null)
            {
                _values.Add(jsonNumber.DoubleValue);
                _transformableValues.Add(jsonNumber);
            }
            else if (value is string)
            {
                _values.Add(value);
                _transformableValues.Add(value);
            }
            else
            {
                _values.Add(value);
                _transformableValues.Add(null);
            }
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var convertFunc = _convertFuncs.GetOrAdd(binder.Type, GetConvertFunc);

            return convertFunc != null
                ? convertFunc(out result)
                : base.TryConvert(binder, out result);
        }

        public object this[int index]
        {
            get { return _values[index]; }
            set
            {
                _values[index] = value;
                _transformableValues[index] = null;
            }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public JsonArray Decrypt(int index)
        {
            if (_info.EncryptionMechanism != null)
            {
                var value = _transformableValues[index];

                if (value is string)
                {
                    var decryptedJson = _info.EncryptionMechanism.Decrypt(
                        (string)value, _info.EncryptKey, _info.SerializationState);

                    using (var stringReader = new StringReader(decryptedJson))
                    {
                        using (var reader = new JsonReader(stringReader, _info))
                        {
                            value = DynamicJsonSerializer.Get(false).DeserializeObject(reader, _info);

                            if (value == null
                                || value is bool
                                || value is JsonArray
                                || value is JsonObject)
                            {
                                _values[index] = value;
                                _transformableValues[index] = null;
                            }
                            else if (value is string)
                            {
                                _values[index] = value;
                                _transformableValues[index] = value;
                            }
                            else
                            {
                                var jsonNumber = value as JsonNumber;
                                if (jsonNumber != null)
                                {
                                    _values[index] = jsonNumber.DoubleValue;
                                    _transformableValues[index] = jsonNumber;
                                }
                                else
                                {
                                    throw new NotSupportedException("Unsupported value type: " + value.GetType());
                                }
                            }
                        }
                    }
                }
            }

            return this;
        }

        public JsonArray Encrypt(int index)
        {
            if (_info.EncryptionMechanism != null)
            {
                var value = _values[index];

                var sb = new StringBuilder();

                using (var stringwriter = new StringWriter(sb))
                {
                    using (var writer = new JsonWriter(stringwriter, _info))
                    {
                        DynamicJsonSerializer.Get(false).SerializeObject(writer, value, _info);
                    }
                }

                value = _info.EncryptionMechanism.Encrypt(sb.ToString(), _info.EncryptKey, _info.SerializationState);
                _values[index] = value;
            }

            return this;
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

            for (int i = 0; i < _values.Count; i++)
            {
                if (_transformableValues[i] != null)
                {
                    _values[i] = transform(_values[i], _transformableValues[i]);
                }
            }

            return this;
        }

        private TryFunc GetConvertFunc(Type type)
        {
            if (type.IsInterface && type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(IEnumerable<>)
                    || genericTypeDefinition == typeof(ICollection<>)
                    || genericTypeDefinition == typeof(IList<>))
                {
                    var collectionType = type.GetGenericArguments()[0];

                    if (collectionType == typeof(object))
                    {
                        return ((out object r) =>
                        {
                            r = _values;
                            return true;
                        });
                    }

                    if (collectionType == typeof(JsonObject))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<JsonObject>(_values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(JsonArray))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<JsonArray>(_values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(bool))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<bool>(TransformItems<bool>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(bool?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<bool?>(TransformItems<bool?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(byte))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<byte>(TransformItems<byte>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(byte?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<byte?>(TransformItems<byte?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(sbyte))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<sbyte>(TransformItems<sbyte>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(sbyte?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<sbyte?>(TransformItems<sbyte?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(short))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<short>(TransformItems<short>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(short?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<short?>(TransformItems<short?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(ushort))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<ushort>(TransformItems<ushort>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(ushort?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<ushort?>(TransformItems<ushort?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(int))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<int>(TransformItems<int>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(int?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<int?>(TransformItems<int?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(uint))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<uint>(TransformItems<uint>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(uint?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<uint?>(TransformItems<uint?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(long))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<long>(TransformItems<long>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(long?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<long?>(TransformItems<long?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(ulong))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<ulong>(TransformItems<ulong>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(ulong?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<ulong?>(TransformItems<ulong?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(float))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<float>(TransformItems<float>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(float?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<float?>(TransformItems<float?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(double))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<double>(_values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(double?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<double?>(_values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(decimal))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<decimal>(TransformItems<decimal>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(decimal?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<decimal?>(TransformItems<decimal?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(string))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<string>(_values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(DateTime))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<DateTime>(TransformItems<DateTime>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(DateTime?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<DateTime?>(TransformItems<DateTime?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(DateTimeOffset))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<DateTimeOffset>(TransformItems<DateTimeOffset>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(DateTimeOffset?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<DateTimeOffset?>(TransformItems<DateTimeOffset?>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(Guid))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<Guid>(TransformItems<Guid>()._values);
                            return true;
                        });
                    }

                    if (collectionType == typeof(Guid?))
                    {
                        return ((out object r) =>
                        {
                            r = new ConversionList<Guid?>(TransformItems<Guid?>()._values);
                            return true;
                        });
                    }
                }
            }

            return null;
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
                return _info.DateTimeHandler.ParseDateTime((string)transformableValue);
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
                return _info.DateTimeHandler.ParseDateTimeOffset((string)transformableValue);
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

        public static implicit operator JsonObject[](JsonArray jsonArray)
        {
            return jsonArray._values.Cast<JsonObject>().ToArray();
        }

        public static implicit operator JsonArray[](JsonArray jsonArray)
        {
            return jsonArray._values.Cast<JsonArray>().ToArray();
        }

        public static implicit operator string[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<string>()._values.Cast<string>().ToArray();
        }

        public static implicit operator DateTime[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTime>()._values.Cast<DateTime>().ToArray();
        }

        public static implicit operator DateTime?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTime?>()._values.Cast<DateTime?>().ToArray();
        }

        public static implicit operator DateTimeOffset[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTimeOffset>()._values.Cast<DateTimeOffset>().ToArray();
        }

        public static implicit operator DateTimeOffset?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<DateTimeOffset?>()._values.Cast<DateTimeOffset?>().ToArray();
        }

        public static implicit operator Guid[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<Guid>()._values.Cast<Guid>().ToArray();
        }

        public static implicit operator Guid?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<Guid?>()._values.Cast<Guid?>().ToArray();
        }

        public static implicit operator bool[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<bool>()._values.Cast<bool>().ToArray();
        }

        public static implicit operator bool?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<bool?>()._values.Cast<bool?>().ToArray();
        }

        public static implicit operator byte[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<byte>()._values.Cast<byte>().ToArray();
        }

        public static implicit operator byte?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<byte?>()._values.Cast<byte?>().ToArray();
        }

        public static implicit operator sbyte[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<sbyte>()._values.Cast<sbyte>().ToArray();
        }

        public static implicit operator sbyte?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<sbyte?>()._values.Cast<sbyte?>().ToArray();
        }

        public static implicit operator short[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<short>()._values.Cast<short>().ToArray();
        }

        public static implicit operator short?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<short?>()._values.Cast<short?>().ToArray();
        }

        public static implicit operator ushort[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ushort>()._values.Cast<ushort>().ToArray();
        }

        public static implicit operator ushort?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ushort?>()._values.Cast<ushort?>().ToArray();
        }

        public static implicit operator int[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<int>()._values.Cast<int>().ToArray();
        }

        public static implicit operator int?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<int?>()._values.Cast<int?>().ToArray();
        }

        public static implicit operator uint[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<uint>()._values.Cast<uint>().ToArray();
        }

        public static implicit operator uint?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<uint?>()._values.Cast<uint?>().ToArray();
        }

        public static implicit operator long[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<long>()._values.Cast<long>().ToArray();
        }

        public static implicit operator long?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<long?>()._values.Cast<long?>().ToArray();
        }

        public static implicit operator ulong[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ulong>()._values.Cast<ulong>().ToArray();
        }

        public static implicit operator ulong?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<ulong?>()._values.Cast<ulong?>().ToArray();
        }

        public static implicit operator float[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<float>()._values.Cast<float>().ToArray();
        }

        public static implicit operator float?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<float?>()._values.Cast<float?>().ToArray();
        }

        public static implicit operator double[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<double>()._values.Cast<double>().ToArray();
        }

        public static implicit operator double?[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<double?>()._values.Cast<double?>().ToArray();
        }

        public static implicit operator decimal[](JsonArray jsonArray)
        {
            return jsonArray.TransformItems<decimal>()._values.Cast<decimal>().ToArray();
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

        private class ConversionList<T> : IList<T>
        {
            private readonly List<T> _list = new List<T>();
            private readonly List<object> _backingList;

            public ConversionList(List<object> backingList)
            {
                _backingList = backingList;

                foreach (var item in backingList)
                {
                    _list.Add((T)item);
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_list).GetEnumerator();
            }

            public void Add(T item)
            {
                _list.Add(item);
                _backingList.Add(item);
            }

            public void Clear()
            {
                _list.Clear();
                _backingList.Clear();
            }

            public bool Contains(T item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                return _list.Remove(item)
                    && _backingList.Remove(item);
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public int IndexOf(T item)
            {
                return _list.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                _list.Insert(index, item);
                _backingList.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
                _backingList.RemoveAt(index);
            }

            public T this[int index]
            {
                get { return _list[index]; }
                set
                {
                    _list[index] = value;
                    _backingList[index] = value;
                }
            }
        }
    }
}