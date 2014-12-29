namespace XSerializer.Encryption
{
    public static class EncryptionMechanismExtensions
    {
        public static string Encrypt(this IEncryptionMechanism source, string plainText, bool encryptionEnabled)
        {
            return
                encryptionEnabled
                    ? source.Encrypt(plainText)
                    : plainText;
        }

        public static string Decrypt(this IEncryptionMechanism source, string cipherText, bool encryptionEnabled)
        {
            return
                encryptionEnabled
                    ? source.Decrypt(cipherText)
                    : cipherText;
        }
    }
}