using System;

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
        /// <param name="encryptKey">Ignored.</param>
        /// <param name="serializationState">Ignored.</param>
        /// <returns>The value of <paramref name="text"/>.</returns>
        public string Encrypt(string text, object encryptKey, SerializationState serializationState)
        {
#if !BUILD
            if (serializationState == null) throw new ArgumentNullException(nameof(serializationState));
#endif
            return text;
        }

        /// <summary>
        /// Returns the value of <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Some text.</param>
        /// <param name="encryptKey">Ignored.</param>
        /// <param name="serializationState">Ignored.</param>
        /// <returns>The value of <paramref name="text"/>.</returns>
        public string Decrypt(string text, object encryptKey, SerializationState serializationState)
        {
#if !BUILD
            if (serializationState == null) throw new ArgumentNullException(nameof(serializationState));
#endif
            return text;
        }
    }
}