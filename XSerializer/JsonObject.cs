using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    public sealed class JsonObject : DynamicObject, IEnumerable<KeyValuePair<string, object>>
    {
        private static readonly string[] _definedProjections =
        {
            "AsByte",
            "AsSByte",
            "AsInt16",
            "AsUInt16",
            "AsInt32",
            "AsUInt32",
            "AsInt64",
            "AsUInt64",
            "AsDouble",
            "AsSingle",
            "AsDecimal",
            "AsString",
            "AsDateTime",
            "AsDateTimeOffset",
            "AsGuid"
        };

        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        private readonly Dictionary<string, string> _numericStringValues = new Dictionary<string, string>();
        private readonly Dictionary<string, object> _projections = new Dictionary<string, object>();

        private readonly IJsonSerializeOperationInfo _info;

        public JsonObject(
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

        internal JsonObject(IJsonSerializeOperationInfo info)
        {
            _info = info;
        }

        public void Add(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            AddImpl(name, GuardValue(value));
        }

        private void AddImpl(string name, object value)
        {
            var jsonNumber = value as JsonNumber;
            if (jsonNumber != null)
            {
                _values.Add(name, jsonNumber.DoubleValue);
                _numericStringValues.Add(name, jsonNumber.StringValue);
            }
            else
            {
                _values.Add(name, value);
            }
        }

        public object this[string name]
        {
            get
            {
                object value;
                if (TryGetValue(name, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                value = GuardValue(value);
                if (!TrySetValueImpl(name, value))
                {
                    AddImpl(name, value);
                }
            }
        }

        public JsonObject Decrypt(string name)
        {
            if (_info.EncryptionMechanism != null)
            {
                object value;
                if (_values.TryGetValue(name, out value)
                    && value is string)
                {
                    var decryptedJson = _info.EncryptionMechanism.Decrypt(
                        (string)value, _info.EncryptKey, _info.SerializationState);

                    using (var stringReader = new StringReader(decryptedJson))
                    {
                        using (var reader = new JsonReader(stringReader, _info))
                        {
                            value = DynamicJsonSerializer.Get(false, JsonConcreteImplementations.Empty).DeserializeObject(reader, _info);

                            if (value == null
                                || value is bool
                                || value is string
                                || value is JsonArray
                                || value is JsonObject)
                            {
                                _values[name] = value;
                                return this;
                            }

                            var jsonNumber = value as JsonNumber;
                            if (jsonNumber != null)
                            {
                                _values[name] = jsonNumber.DoubleValue;
                                _numericStringValues[name] = jsonNumber.StringValue;
                                return this;
                            }

                            throw new NotSupportedException("Unsupported value type: " + value.GetType());
                        }
                    }
                }
            }

            return this;
        }

        public JsonObject Encrypt(string name)
        {
            if (_info.EncryptionMechanism != null)
            {
                object value;
                if (_values.TryGetValue(name, out value)
                    && value != null)
                {
                    var sb = new StringBuilder();

                    using (var stringwriter = new StringWriter(sb))
                    {
                        using (var writer = new JsonWriter(stringwriter, _info))
                        {
                            DynamicJsonSerializer.Get(false, JsonConcreteImplementations.Empty).SerializeObject(writer, value, _info);
                        }
                    }

                    value = _info.EncryptionMechanism.Encrypt(sb.ToString(), _info.EncryptKey, _info.SerializationState);
                    _values[name] = value;
                }
            }

            return this;
        }

        public bool TryGetValue(string name, out object result)
        {
            if (_values.TryGetValue(name, out result)
                || _projections.TryGetValue(name, out result))
            {
                return true;
            }

            return TryGetProjection(name, ref result);
        }

        public bool TrySetValue(string name, object value)
        {
            return TrySetValueImpl(name, GuardValue(value));
        }

        private bool TrySetValueImpl(string name, object value)
        {
            if (_values.ContainsKey(name))
            {
                var jsonNumber = value as JsonNumber;
                if (jsonNumber != null)
                {
                    _values[name] = jsonNumber.DoubleValue;
                    _numericStringValues[name] = jsonNumber.StringValue;
                }
                else
                {
                    _values[name] = value;
                    _numericStringValues.Remove(name);
                }

                RemoveProjections(name);

                return true;
            }

            return false;
        }

        private static object GuardValue(object value)
        {
            if (value == null
                || value is bool
                || value is string
                || value is JsonNumber
                || value is JsonObject
                || value is JsonArray)
            {
                return value;
            }

            if (value is int
                || value is double
                || value is byte
                || value is long
                || value is decimal
                || value is uint
                || value is ulong
                || value is short
                || value is float
                || value is ushort
                || value is sbyte)
            {
                return new JsonNumber(value.ToString());
            }

            if (value is Guid)
            {
                var guid = (Guid)value;
                return guid.ToString("D");
            }

            if (value is DateTime)
            {
                var dateTime = (DateTime)value;
                return dateTime.ToString("O");
            }

            if (value is DateTimeOffset)
            {
                var dateTimeOffset = (DateTimeOffset)value;
                return dateTimeOffset.ToString("O");
            }
            
            throw new XSerializerException("Invalid value for JsonObject member: " + value.GetType().FullName);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_values).GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            var other = obj as JsonObject;
            if (other == null)
            {
                return false;
            }

            foreach (var item in _values)
            {
                if (!other._values.ContainsKey(item.Key))
                {
                    return false;
                }

                object value;
                object otherValue;

                if (_numericStringValues.ContainsKey(item.Key))
                {
                    if (!other._numericStringValues.ContainsKey(item.Key))
                    {
                        return false;
                    }

                    value = _numericStringValues[item.Key];
                    otherValue = other._numericStringValues[item.Key];
                }
                else
                {
                    value = _values[item.Key];
                    otherValue = other._values[item.Key];
                }

                if (!Equals(value, otherValue))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = typeof(JsonObject).GetHashCode();

                foreach (var item in _values.OrderBy(x => x.Key))
                {
                    hashCode = (hashCode * 397) ^ item.Key.GetHashCode();
                    hashCode = (hashCode * 397) ^ (item.Value != null ? item.Value.GetHashCode() : 0);
                }

                return hashCode;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        private bool TryGetProjection(string name, ref object result)
        {
            string modifiedName;

            if (EndsWith(name, "AsByte", out modifiedName))
            {
                return AsByte(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsSByte", out modifiedName))
            {
                return AsSByte(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsInt16", out modifiedName))
            {
                return AsInt16(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsUInt16", out modifiedName))
            {
                return AsUInt16(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsInt32", out modifiedName))
            {
                return AsInt32(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsUInt32", out modifiedName))
            {
                return AsUInt32(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsInt64", out modifiedName))
            {
                return AsInt64(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsUInt64", out modifiedName))
            {
                return AsUInt64(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsDouble", out modifiedName))
            {
                return AsDouble(ref result, modifiedName);
            }

            if (EndsWith(name, "AsSingle", out modifiedName))
            {
                return AsSingle(ref result, modifiedName);
            }

            if (EndsWith(name, "AsDecimal", out modifiedName))
            {
                return AsDecimal(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsString", out modifiedName))
            {
                return AsString(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsDateTime", out modifiedName))
            {
                return AsDateTime(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsDateTimeOffset", out modifiedName))
            {
                return AsDateTimeOffset(ref result, modifiedName, name);
            }

            if (EndsWith(name, "AsGuid", out modifiedName))
            {
                return AsGuid(ref result, modifiedName, name);
            }

            return false;
        }

        private void RemoveProjections(string name)
        {
            var toRemove =
                from projectionName in _projections.Keys
                where projectionName.StartsWith(name) && _definedProjections.Any(projectionName.EndsWith)
                select projectionName;

            foreach (var key in toRemove)
            {
                _projections.Remove(key);
            }
        }

        private static bool EndsWith(string binderName, string suffix, out string name)
        {
            if (binderName.EndsWith(suffix, StringComparison.InvariantCulture))
            {
                name = binderName.Substring(
                    0, binderName.LastIndexOf(suffix, StringComparison.InvariantCulture));
                return true;
            }

            name = null;
            return false;
        }

        private bool AsByte(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                byte byteResult;
                if (byte.TryParse(value, out byteResult))
                {
                    result = byteResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsSByte(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                sbyte sbyteResult;
                if (sbyte.TryParse(value, out sbyteResult))
                {
                    result = sbyteResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsInt16(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                short shortResult;
                if (short.TryParse(value, out shortResult))
                {
                    result = shortResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsUInt16(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                ushort ushortResult;
                if (ushort.TryParse(value, out ushortResult))
                {
                    result = ushortResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsInt32(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                int intResult;
                if (int.TryParse(value, out intResult))
                {
                    result = intResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsUInt32(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                uint uintResult;
                if (uint.TryParse(value, out uintResult))
                {
                    result = uintResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsInt64(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                long longResult;
                if (long.TryParse(value, out longResult))
                {
                    result = longResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsUInt64(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                TruncateNumber(ref value);

                ulong ulongResult;
                if (ulong.TryParse(value, out ulongResult))
                {
                    result = ulongResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsDouble(ref object result, string name)
        {
            object value;
            if (_values.TryGetValue(name, out value)
                && value is double)
            {
                result = value;
                return true;
            }

            return false;
        }

        private bool AsSingle(ref object result, string name)
        {
            object value;
            if (_values.TryGetValue(name, out value)
                && value is double)
            {
                result = (float)((double)value);
                return true;
            }

            return false;
        }

        private bool AsDecimal(ref object result, string name, string binderName)
        {
            string value;
            if (_numericStringValues.TryGetValue(name, out value))
            {
                decimal decimalResult;
                if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimalResult))
                {
                    result = decimalResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsString(ref object result, string name, string binderName)
        {
            object value;
            if (_values.TryGetValue(name, out value))
            {
                if (value == null)
                {
                    result = null;
                    _projections.Add(binderName, result);
                    return true;
                }
                var stringValue = value as string;
                if (stringValue != null)
                {
                    result = stringValue;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private bool AsDateTime(ref object result, string name, string binderName)
        {
            object value;
            if (_values.TryGetValue(name, out value))
            {
                if (value == null)
                {
                    result = null;
                    _projections.Add(binderName, result);
                    return true;
                }
                var stringValue = value as string;
                if (stringValue != null)
                {
                    try
                    {
                        result = _info.DateTimeHandler.ParseDateTime(stringValue);
                        _projections.Add(binderName, result);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool AsDateTimeOffset(ref object result, string name, string binderName)
        {
            object value;
            if (_values.TryGetValue(name, out value))
            {
                if (value == null)
                {
                    result = null;
                    _projections.Add(binderName, result);
                    return true;
                }
                var stringValue = value as string;
                if (stringValue != null)
                {
                    try
                    {
                        result = _info.DateTimeHandler.ParseDateTimeOffset(stringValue);
                        _projections.Add(binderName, result);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool AsGuid(ref object result, string name, string binderName)
        {
            object value;
            if (_values.TryGetValue(name, out value)
                && value is string)
            {
                Guid guidResult;
                if (Guid.TryParse((string)value, out guidResult))
                {
                    result = guidResult;
                    _projections.Add(binderName, result);
                    return true;
                }
            }

            return false;
        }

        private static void TruncateNumber(ref string value)
        {
            if (value.Contains('.') || value.Contains('e') || value.Contains('E'))
            {
                var d = double.Parse(value);
                d = Math.Truncate(d);
                value = d.ToString(NumberFormatInfo.InvariantInfo);
            }
        }
    }
}