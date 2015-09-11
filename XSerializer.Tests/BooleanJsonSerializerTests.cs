using NUnit.Framework;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class BooleanJsonSerializerTests
    {
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void CanSerialize(bool value, string expected)
        {
            var serializer = new JsonSerializer<bool>();

            var json = serializer.Serialize(value);

            Assert.That(json, Is.EqualTo(expected));
        }

        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void CanSerializeEncrypted(bool value, string expectedPlainText)
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<bool>(configuration);

            var json = serializer.Serialize(value);

            var expected =
                @""""
                + encryptionMechanism.Encrypt(expectedPlainText)
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }
    }
}