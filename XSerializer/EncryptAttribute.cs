using System;

namespace XSerializer
{
    public class EncryptAttribute : Attribute
    {
        public string Encrypt(
            string plaintext,
            Type declaringType,
            bool encryptionEnabled,
            IEncryptionAlgorithmFactory encryptionAlgorithmFactory)
        {
            if (plaintext == null)
            {
                return null;
            }

            return
                encryptionEnabled
                    ? (encryptionAlgorithmFactory ?? EncryptionAlgorithmFactory.Current).GetAlgorithm(declaringType).Encrypt(plaintext)
                    : plaintext;
        }

        public string Decrypt(
            string ciphertext,
            Type declaringType,
            bool encryptionEnabled,
            IEncryptionAlgorithmFactory encryptionAlgorithmFactory)
        {
            if (ciphertext == null)
            {
                return null;
            }

            return
                encryptionEnabled
                    ? (encryptionAlgorithmFactory ?? EncryptionAlgorithmFactory.Current).GetAlgorithm(declaringType).Decrypt(ciphertext)
                    : ciphertext;
        }
    }
}