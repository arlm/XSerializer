namespace XSerializer.Encryption
{
    /// <summary>
    /// Provides a mechanism for obtaining an instance of <see cref="IEncryptionProvider"/>.
    /// </summary>
    public interface IEncryptionProviderProvider
    {
        /// <summary>
        /// Gets an instance of <see cref="IEncryptionProvider"/>.
        /// </summary>
        /// <returns>An instance of <see cref="IEncryptionProvider"/>.</returns>
        IEncryptionProvider GetEncryptionProvider();
    }
}