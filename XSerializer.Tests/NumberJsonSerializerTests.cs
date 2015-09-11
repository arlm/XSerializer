using NUnit.Framework;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class NumberJsonSerializerTests
    {
        [Test]
        public void CanSerialize()
        {
            var serializer = new JsonSerializer<double>();

            var json = serializer.Serialize(123.45);

            Assert.That(json, Is.EqualTo(@"123.45"));
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
    }
}