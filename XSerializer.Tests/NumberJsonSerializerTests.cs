using NUnit.Framework;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class NumberJsonSerializerTests
    {
        [Test]
        public void CanSerializeDouble()
        {
            var serializer = new JsonSerializer<double>();

            var json = serializer.Serialize(123.45);

            Assert.That(json, Is.EqualTo("123.45"));
        }

        [Test]
        public void CanSerializeNullableDouble()
        {
            var serializer = new JsonSerializer<double?>();

            var json = serializer.Serialize(123.45);

            Assert.That(json, Is.EqualTo("123.45"));
        }

        [Test]
        public void CanSerializeNullableDoubleNullValue()
        {
            var serializer = new JsonSerializer<double?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeInt()
        {
            var serializer = new JsonSerializer<int>();

            var json = serializer.Serialize(123);

            Assert.That(json, Is.EqualTo("123"));
        }

        [Test]
        public void CanSerializeNullableInt()
        {
            var serializer = new JsonSerializer<int?>();

            var json = serializer.Serialize(123);

            Assert.That(json, Is.EqualTo("123"));
        }

        [Test]
        public void CanSerializeNullableIntNullValue()
        {
            var serializer = new JsonSerializer<int?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeLong()
        {
            var serializer = new JsonSerializer<long>();

            var json = serializer.Serialize(10000000000000);

            Assert.That(json, Is.EqualTo("10000000000000"));
        }

        [Test]
        public void CanSerializeNullableLong()
        {
            var serializer = new JsonSerializer<long?>();

            var json = serializer.Serialize(10000000000000);

            Assert.That(json, Is.EqualTo("10000000000000"));
        }

        [Test]
        public void CanSerializeNullableLongNullValue()
        {
            var serializer = new JsonSerializer<long?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeUInt()
        {
            var serializer = new JsonSerializer<uint>();

            var json = serializer.Serialize(4000000000);

            Assert.That(json, Is.EqualTo("4000000000"));
        }

        [Test]
        public void CanSerializeNullableUInt()
        {
            var serializer = new JsonSerializer<uint?>();

            var json = serializer.Serialize(4000000000);

            Assert.That(json, Is.EqualTo("4000000000"));
        }

        [Test]
        public void CanSerializeNullableUIntNullValue()
        {
            var serializer = new JsonSerializer<uint?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeByte()
        {
            var serializer = new JsonSerializer<byte>();

            var json = serializer.Serialize(255);

            Assert.That(json, Is.EqualTo("255"));
        }

        [Test]
        public void CanSerializeNullableByte()
        {
            var serializer = new JsonSerializer<byte?>();

            var json = serializer.Serialize(255);

            Assert.That(json, Is.EqualTo("255"));
        }

        [Test]
        public void CanSerializeNullableByteNullValue()
        {
            var serializer = new JsonSerializer<byte?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeSByte()
        {
            var serializer = new JsonSerializer<sbyte>();

            var json = serializer.Serialize(-128);

            Assert.That(json, Is.EqualTo("-128"));
        }

        [Test]
        public void CanSerializeNullableSByte()
        {
            var serializer = new JsonSerializer<sbyte?>();

            var json = serializer.Serialize(-128);

            Assert.That(json, Is.EqualTo("-128"));
        }

        [Test]
        public void CanSerializeNullableSByteNullValue()
        {
            var serializer = new JsonSerializer<sbyte?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeShort()
        {
            var serializer = new JsonSerializer<short>();

            var json = serializer.Serialize(32000);

            Assert.That(json, Is.EqualTo("32000"));
        }

        [Test]
        public void CanSerializeNullableShort()
        {
            var serializer = new JsonSerializer<short?>();

            var json = serializer.Serialize(32000);

            Assert.That(json, Is.EqualTo("32000"));
        }

        [Test]
        public void CanSerializeNullableShortNullValue()
        {
            var serializer = new JsonSerializer<short?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeUShort()
        {
            var serializer = new JsonSerializer<ushort>();

            var json = serializer.Serialize(65000);

            Assert.That(json, Is.EqualTo("65000"));
        }

        [Test]
        public void CanSerializeNullableUShort()
        {
            var serializer = new JsonSerializer<ushort?>();

            var json = serializer.Serialize(65000);

            Assert.That(json, Is.EqualTo("65000"));
        }

        [Test]
        public void CanSerializeNullableUShortNullValue()
        {
            var serializer = new JsonSerializer<ushort?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeULong()
        {
            var serializer = new JsonSerializer<ulong>();

            var json = serializer.Serialize(10000000000000000000);

            Assert.That(json, Is.EqualTo("10000000000000000000"));
        }

        [Test]
        public void CanSerializeNullableULong()
        {
            var serializer = new JsonSerializer<ulong?>();

            var json = serializer.Serialize(10000000000000000000);

            Assert.That(json, Is.EqualTo("10000000000000000000"));
        }

        [Test]
        public void CanSerializeNullableULongNullValue()
        {
            var serializer = new JsonSerializer<ulong?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeFloat()
        {
            var serializer = new JsonSerializer<float>();

            var json = serializer.Serialize(123.45F);

            Assert.That(json, Is.EqualTo("123.45"));
        }

        [Test]
        public void CanSerializeNullableFloat()
        {
            var serializer = new JsonSerializer<float?>();

            var json = serializer.Serialize(123.45F);

            Assert.That(json, Is.EqualTo("123.45"));
        }

        [Test]
        public void CanSerializeNullableFloatNullValue()
        {
            var serializer = new JsonSerializer<float?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeDecimal()
        {
            var serializer = new JsonSerializer<decimal>();

            var json = serializer.Serialize(123.45M);

            Assert.That(json, Is.EqualTo("123.45"));
        }

        [Test]
        public void CanSerializeNullableDecimal()
        {
            var serializer = new JsonSerializer<decimal?>();

            var json = serializer.Serialize(123.45M);

            Assert.That(json, Is.EqualTo("123.45"));
        }

        [Test]
        public void CanSerializeNullableDecimalNullValue()
        {
            var serializer = new JsonSerializer<decimal?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<double>(configuration);

            var json = serializer.Serialize(123.45);

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"123.45")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeDouble()
        {
            const string json = "123.45";

            var serializer = new JsonSerializer<double>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeNullableDouble()
        {
            const string json = "123.45";

            var serializer = new JsonSerializer<double?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeNullableDoubleNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<double?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeInt()
        {
            const string json = "123";

            var serializer = new JsonSerializer<int>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeNullableInt()
        {
            const string json = "123";

            var serializer = new JsonSerializer<int?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeNullableIntNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<int?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeLong()
        {
            const string json = "10000000000000";

            var serializer = new JsonSerializer<long>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(10000000000000));
        }

        [Test]
        public void CanDeserializeNullableLong()
        {
            const string json = "10000000000000";

            var serializer = new JsonSerializer<long?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(10000000000000));
        }

        [Test]
        public void CanDeserializeNullableLongNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<long?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeUInt()
        {
            const string json = "4000000000";

            var serializer = new JsonSerializer<uint>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(4000000000));
        }

        [Test]
        public void CanDeserializeNullableUInt()
        {
            const string json = "4000000000";

            var serializer = new JsonSerializer<uint?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(4000000000));
        }

        [Test]
        public void CanDeserializeNullableUIntNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<uint?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeByte()
        {
            const string json = "255";

            var serializer = new JsonSerializer<byte>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(255));
        }

        [Test]
        public void CanDeserializeNullableByte()
        {
            const string json = "255";

            var serializer = new JsonSerializer<byte?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(255));
        }

        [Test]
        public void CanDeserializeNullableByteNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<byte?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeSByte()
        {
            const string json = "-128";

            var serializer = new JsonSerializer<sbyte>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(-128));
        }

        [Test]
        public void CanDeserializeNullableSByte()
        {
            const string json = "-128";

            var serializer = new JsonSerializer<sbyte?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(-128));
        }

        [Test]
        public void CanDeserializeNullableSByteNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<sbyte?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeShort()
        {
            const string json = "32000";

            var serializer = new JsonSerializer<short>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(32000));
        }

        [Test]
        public void CanDeserializeNullableShort()
        {
            const string json = "32000";

            var serializer = new JsonSerializer<short?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(32000));
        }

        [Test]
        public void CanDeserializeNullableShortNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<short?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeUShort()
        {
            const string json = "65000";

            var serializer = new JsonSerializer<ushort>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(65000));
        }

        [Test]
        public void CanDeserializeNullableUShort()
        {
            const string json = "65000";

            var serializer = new JsonSerializer<ushort?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(65000));
        }

        [Test]
        public void CanDeserializeNullableUShortNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<ushort?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeULong()
        {
            const string json = "10000000000000000000";

            var serializer = new JsonSerializer<ulong>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(10000000000000000000));
        }

        [Test]
        public void CanDeserializeNullableULong()
        {
            const string json = "10000000000000000000";

            var serializer = new JsonSerializer<ulong?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(10000000000000000000));
        }

        [Test]
        public void CanDeserializeNullableULongNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<ulong?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeFloat()
        {
            const string json = "123.45";

            var serializer = new JsonSerializer<float>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45F));
        }

        [Test]
        public void CanDeserializeNullableFloat()
        {
            const string json = "123.45";

            var serializer = new JsonSerializer<float?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45F));
        }

        [Test]
        public void CanDeserializeNullableFloatNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<float?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeDecimal()
        {
            const string json = "123.45";

            var serializer = new JsonSerializer<decimal>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45M));
        }

        [Test]
        public void CanDeserializeNullableDecimal()
        {
            const string json = "123.45";

            var serializer = new JsonSerializer<decimal?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45M));
        }

        [Test]
        public void CanDeserializeNullableDecimalNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<decimal?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeWithWhitespace()
        {
            const string json = "  123.45  ";

            var serializer = new JsonSerializer<double>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<double>(configuration);

            var json = @""""
                + encryptionMechanism.Encrypt("123.45")
                + @"""";

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(123.45));
        }
    }
}