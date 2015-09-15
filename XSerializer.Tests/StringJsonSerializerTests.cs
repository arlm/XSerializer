using System;
using NUnit.Framework;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class StringJsonSerializerTests
    {
        [Test]
        public void CanSerialize()
        {
            var serializer = new JsonSerializer<string>();

            var json = serializer.Serialize("abc");

            Assert.That(json, Is.EqualTo(@"""abc"""));
        }

        [Test]
        public void CanSerializeDateTime()
        {
            var now = DateTime.Now;

            var serializer = new JsonSerializer<DateTime>();

            var json = serializer.Serialize(now);

            var expected = @"""" + now.ToString("O") + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNullableDateTime()
        {
            var now = DateTime.Now;

            var serializer = new JsonSerializer<DateTime?>();

            var json = serializer.Serialize(now);

            var expected = @"""" + now.ToString("O") + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNullableDateTimeNullValue()
        {
            var serializer = new JsonSerializer<DateTime?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeDateTimeOffset()
        {
            var now = DateTimeOffset.Now;

            var serializer = new JsonSerializer<DateTimeOffset>();

            var json = serializer.Serialize(now);

            var expected = @"""" + now.ToString("O") + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNullableDateTimeOffset()
        {
            var now = DateTimeOffset.Now;

            var serializer = new JsonSerializer<DateTimeOffset?>();

            var json = serializer.Serialize(now);

            var expected = @"""" + now.ToString("O") + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNullableDateTimeOffsetNullValue()
        {
            var serializer = new JsonSerializer<DateTimeOffset?>();

            var json = serializer.Serialize(null);

            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void CanSerializeGuid()
        {
            var now = Guid.NewGuid();

            var serializer = new JsonSerializer<Guid>();

            var json = serializer.Serialize(now);

            var expected = @"""" + now.ToString("D") + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNullableGuid()
        {
            var now = Guid.NewGuid();

            var serializer = new JsonSerializer<Guid?>();

            var json = serializer.Serialize(now);

            var expected = @"""" + now.ToString("D") + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNullableGuidNullValue()
        {
            var serializer = new JsonSerializer<Guid?>();

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

            var serializer = new JsonSerializer<string>(configuration);

            var json = serializer.Serialize("abc");

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"""abc""")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserialize()
        {
            const string json = @"""abc""";

            var serializer = new JsonSerializer<string>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDeserializeDateTime()
        {
            var now = DateTime.Now;

            var json = @"""" + now.ToString("O") + @"""";

            var serializer = new JsonSerializer<DateTime>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(now));
        }

        [Test]
        public void CanDeserializeNullableDateTime()
        {
            var now = DateTime.Now;

            var json = @"""" + now.ToString("O") + @"""";

            var serializer = new JsonSerializer<DateTime?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(now));
        }

        [Test]
        public void CanDeserializeNullableDateTimeNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<DateTime?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeGuid()
        {
            var guid = Guid.NewGuid();

            var json = @"""" + guid.ToString("D") + @"""";

            var serializer = new JsonSerializer<Guid>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(guid));
        }

        [Test]
        public void CanDeserializeNullableGuid()
        {
            var guid = Guid.NewGuid();

            var json = @"""" + guid.ToString("D") + @"""";

            var serializer = new JsonSerializer<Guid?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo(guid));
        }

        [Test]
        public void CanDeserializeNullableGuidNullValue()
        {
            const string json = "null";

            var serializer = new JsonSerializer<Guid?>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void CanDeserializeWithWhitespace()
        {
            const string json = @"  ""abc""  ";

            var serializer = new JsonSerializer<string>();

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo("abc"));
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

            var serializer = new JsonSerializer<string>(configuration);

            var json = @""""
                + encryptionMechanism.Encrypt(@"""abc""")
                + @"""";

            var value = serializer.Deserialize(json);

            Assert.That(value, Is.EqualTo("abc"));
        }
    }
}