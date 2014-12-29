namespace XSerializer.Encryption
{
    /// <summary>
    /// Provides a mechanism for obtaining an instance of <see cref="IEncryptionMechanism"/>.
    /// </summary>
    public interface IEncryptionMechanismFactory
    {
        /// <summary>
        /// Gets an instance of <see cref="IEncryptionMechanism"/>.
        /// </summary>
        /// <returns>An instance of <see cref="IEncryptionMechanism"/>.</returns>
        IEncryptionMechanism GetEncryptionMechanism();
    }
}