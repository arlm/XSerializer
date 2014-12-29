namespace XSerializer.Encryption
{
    /// <summary>
    /// An implementation of <see cref="IEncryptionMechanism"/> that does
    /// nothing. The <see cref="Encrypt"/> and <see cref="Decrypt"/>
    /// methods both return the value of their parameter.
    /// </summary>
    public class ClearTextEncryptionMechanism : IEncryptionMechanism
    {
        /// <summary>
        /// Returns the value of <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Some text.</param>
        /// <returns>The value of <paramref name="text"/>.</returns>
        public string Encrypt(string text)
        {
            return text;
        }

        /// <summary>
        /// Returns the value of <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Some text.</param>
        /// <returns>The value of <paramref name="text"/>.</returns>
        public string Decrypt(string text)
        {
            return text;
        }
    }
}