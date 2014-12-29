namespace XSerializer.Encryption
{
    /// <summary>
    /// Provides a mechanism for an application to specify an instance of
    /// <see cref="IEncryptionProvider"/> to be used by XSerializer when
    /// encrypting or decrypting data.
    /// </summary>
    public static class EncryptionProvider
    {
        /// <summary>
        /// The default instance of <see cref="IEncryptionProvider"/>.
        /// </summary>
        public static readonly IEncryptionProvider DefaultEncryptionProvider = new ClearTextEncryptionProvider();

        private static IEncryptionProvider _current = DefaultEncryptionProvider;

        /// <summary>
        /// Gets or sets the instance of <see cref="IEncryptionProvider"/>
        /// to be used by XSerializer when encrypting or decrypting data.
        /// When setting this property, if <paramref name="value"/> is null,
        /// then <see cref="DefaultEncryptionProvider"/> will be used instead.
        /// </summary>
        public static IEncryptionProvider Current
        {
            internal get { return _current; }
            set { _current = value ?? DefaultEncryptionProvider; }
        }
    }
}