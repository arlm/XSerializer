namespace XSerializer.Encryption
{
    /// <summary>
    /// Defines an interface for encrypting and decrypting text.
    /// </summary>
    public interface IEncryptionProvider
    {
        /// <summary>
        /// Encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>The encrypted text.</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts the specified cyper text.
        /// </summary>
        /// <param name="cyperText">The cyper text.</param>
        /// <returns>The decrypted text.</returns>
        string Decrypt(string cyperText);
    }
}
