using System;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer.Tests.Encryption
{
    public class Base64EncryptionMechanism : IEncryptionMechanism
    {
        public string Encrypt(string plainText, object encryptKey, SerializationState serializationState)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        public string Decrypt(string cipherText, object encryptKey, SerializationState serializationState)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
        }
    }
}