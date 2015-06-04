namespace XSerializer.Encryption
{
    /// <summary>
    /// Provides a means for an application to specify a default instance of
    /// <see cref="IEncryptionMechanism"/> to be used by XSerializer when encrypting
    /// or decrypting data.
    /// </summary>
    public static class EncryptionMechanism
    {
        private static IEncryptionMechanism _current = GetDefaultEncryptionMechanism();

        /// <summary>
        /// Get or sets the current instance of <see cref="IEncryptionMechanism"/>.
        /// This value is used by XSerializer when encrypting or decrypting data and 
        /// an <see cref="IEncryptionMechanism"/> is not otherwise specified. The
        /// default value is an instance of <see cref="ClearTextEncryptionMechanism"/>.
        /// If set to null, an instance of <see cref="ClearTextEncryptionMechanism"/>
        /// is set instead.
        /// </summary>
        /// <remarks>
        /// Do not change the value of this property while an application is running.
        /// Set it once at the "beginning" of the application and never again.
        /// </remarks>
        public static IEncryptionMechanism Current
        {
            get { return _current; }
            set { _current = value ?? GetDefaultEncryptionMechanism(); }
        }

        /// <summary>
        /// Encrypts the specified plain text using the <see cref="IEncryptionMechanism"/>
        /// specified by the <see cref="Current"/> property.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>The encrypted text.</returns>
        public static string Encrypt(string plainText)
        {
            return Encrypt(plainText, null, null);
        }

        /// <summary>
        /// Encrypts the specified plain text using the <see cref="IEncryptionMechanism"/>
        /// specified by the <see cref="Current"/> property.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="encryptKey">
        /// An object to used to look up invokation-specific encryption parameters.
        /// </param>
        /// <returns>The encrypted text.</returns>
        public static string Encrypt(string plainText, object encryptKey)
        {
            return Encrypt(plainText, encryptKey, null);
        }

        /// <summary>
        /// Encrypts the specified plain text using the <see cref="IEncryptionMechanism"/>
        /// specified by the <see cref="Current"/> property.
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
        public static string Encrypt(string plainText, object encryptKey, SerializationState serializationState)
        {
            return Current.Encrypt(plainText, encryptKey, serializationState);
        }

        /// <summary>
        /// Decrypts the specified cipher text using the <see cref="IEncryptionMechanism"/>
        /// specified by the <see cref="Current"/> property.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string cipherText)
        {
            return Decrypt(cipherText, null, null);
        }

        /// <summary>
        /// Decrypts the specified cipher text using the <see cref="IEncryptionMechanism"/>
        /// specified by the <see cref="Current"/> property.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="encryptKey">
        /// An object to used to look up invokation-specific encryption parameters.
        /// </param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string cipherText, object encryptKey)
        {
            return Decrypt(cipherText, encryptKey, null);
        }

        /// <summary>
        /// Decrypts the specified cipher text using the <see cref="IEncryptionMechanism"/>
        /// specified by the <see cref="Current"/> property.
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
        public static string Decrypt(string cipherText, object encryptKey, SerializationState serializationState)
        {
            return Current.Decrypt(cipherText, encryptKey, serializationState);
        }

        /// <summary>
        /// Gets the <see cref="IEncryptionMechanism"/> specified by the
        /// <see cref="ISerializeOptions.EncryptionMechanism"/> property of the
        /// <paramref name="options"/> parameter. If that value is null, then the value specified
        /// by the static <see cref="Current"/> property of the <see cref="EncryptionMechanism"/>
        /// class is returned.
        /// </summary>
        internal static IEncryptionMechanism GetEncryptionMechanism(this ISerializeOptions options)
        {
            return options.EncryptionMechanism ?? Current;
        }

        private static ClearTextEncryptionMechanism GetDefaultEncryptionMechanism()
        {
            return new ClearTextEncryptionMechanism();
        }
    }
}