using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests
{
    public class EncryptionBugs
    {
        private static readonly IEncryptionMechanism _encryptionMechanism = new EncryptionMarker();

        public class Foo
        {
            [Encrypt]
            public string Bar { get; set; }
        }

        [Test]
        public void EmptyElementWithPropertyMarkedWithEncryptAttributeDoesNotThrow()
        {
            const string xml = @"<Foo><Bar></Bar></Foo>";

            var serializer = new XmlSerializer<Foo>(x => x
                .WithEncryptionMechanism(_encryptionMechanism)
                .WithEncryptKey(typeof(Foo)));

            Assert.That(() => serializer.Deserialize(xml), Throws.Nothing);
        }

        public class EncryptionMarker : IEncryptionMechanism
        {
            public string Encrypt(string plainText)
            {
                return "ENCRYPTED(" + plainText
                    .Replace("[", @"\[").Replace('<', '[')
                    .Replace("]", @"\]").Replace('>', ']') + ")";
            }

            string IEncryptionMechanism.Encrypt(string plainText, object encryptKey, SerializationState serializationState)
            {
                return Encrypt(plainText);
            }

            public string Decrypt(string cipherText)
            {
                return cipherText.Substring(10, cipherText.Length - 11)
                    .Replace(@"\[", "!!11@@22##33").Replace('[', '<').Replace("!!11@@22##33", "[")
                    .Replace(@"\]", "!!11@@22##33").Replace(']', '>').Replace("!!11@@22##33", "]");
            }

            string IEncryptionMechanism.Decrypt(string cipherText, object encryptKey, SerializationState serializationState)
            {
                return Decrypt(cipherText);
            }
        }
    }
}
