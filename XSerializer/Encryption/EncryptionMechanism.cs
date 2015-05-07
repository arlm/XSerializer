namespace XSerializer.Encryption
{
    /// <summary>
    /// Provides a means for an application to specify a default instance of
    /// <see cref="IEncryptionMechanism"/> to be used by XSerializer when encrypting
    /// or decrypting data.
    /// </summary>
    public static class EncryptionMechanism
    {
        private static IEncryptionMechanism _current = new ClearTextEncryptionMechanism();

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
        /// Set it once at the "beginning" of the application, and never again.
        /// </remarks>
        public static IEncryptionMechanism Current
        {
            get { return _current; }
            set { _current = value ?? new ClearTextEncryptionMechanism(); }
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
            return options.EncryptionMechanism ?? _current;
        }
    }
}