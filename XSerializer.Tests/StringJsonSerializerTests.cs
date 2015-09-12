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