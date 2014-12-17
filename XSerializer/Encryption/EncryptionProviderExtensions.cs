namespace XSerializer.Encryption
{
    public static class EncryptionProviderExtensions
    {
        public static string Encrypt(this IEncryptionProvider source, string plainText, bool encryptionEnabled)
        {
            return
                encryptionEnabled
                    ? source.Encrypt(plainText)
                    : plainText;
        }

        public static string Decrypt(this IEncryptionProvider source, string cipherText, bool encryptionEnabled)
        {
            return
                encryptionEnabled
                    ? source.Decrypt(cipherText)
                    : cipherText;
        }
    }
}