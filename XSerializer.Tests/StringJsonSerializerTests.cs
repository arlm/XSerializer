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
    }
}