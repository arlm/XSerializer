namespace XSerializer.Encryption
{
    /// <summary>
    /// Defines an interface for encrypting and decrypting text.
    /// </summary>
    public interface IEncryptionMechanism
    {
        /// <summary>
        /// Encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="encryptKey">
        /// An object to used to look up invokation-specific encryption parameters.
        /// </param>
        /// <param name="serializationState">
        /// An object that holds an arbitrary value that is passed to one or more
        /// encrypt operations within a single serialization operation.
        /// </param>
        /// <returns>The encrypted text.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Encrypt"/> method should be implemented
        /// in such a way that it treates the value of <paramref name="encryptKey"/> as
        /// if it were a key to a dictionary. The value of that dictionary should
        /// contain encryption parameters specific to a particular invocation of the
        /// <see cref="Encrypt"/> method. If the value of <paramref name="encryptKey"/>
        /// is null or unknown, then a default set of encryption parameters should be
        /// used.
        /// </para>
        /// <para>
        /// Implementations of <see cref="IEncryptionMechanism"/> may choose to ignore
        /// the <paramref name="encryptKey"/> parameter altogether.
        /// </para>
        /// </remarks>
        string Encrypt(string plainText, object encryptKey, SerializationState serializationState);

        /// <summary>
        /// Decrypts the specified cipher text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="encryptKey">
        /// An object to used to look up invokation-specific encryption parameters.
        /// </param>
        /// <param name="serializationState">
        /// An object that holds an arbitrary value that is passed to one or more
        /// decrypt operations within a single serialization operation.
        /// </param>
        /// <returns>The decrypted text.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Decrypt"/> method should be implemented
        /// in such a way that it treates the value of <see cref="encryptKey"/> as
        /// if it were a key to a dictionary. The value of that dictionary should
        /// contain encryption parameters specific to a particular invocation of the
        /// <see cref="Decrypt"/> method. If the value of <paramref name="encryptKey"/>
        /// is null or unknown, then a default set of encryption parameters should be
        /// used.
        /// </para>
        /// <para>
        /// Implementations of <see cref="IEncryptionMechanism"/> may choose to ignore
        /// the <paramref name="encryptKey"/> parameter altogether.
        /// </para>
        /// </remarks>
        string Decrypt(string cipherText, object encryptKey, SerializationState serializationState);
    }
}
