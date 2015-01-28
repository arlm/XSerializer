//namespace XSerializer.Encryption
//{
//    /// <summary>
//    /// Provides a mechanism for an application to specify an instance of
//    /// <see cref="IEncryptionMechanism"/> to be used by XSerializer when
//    /// encrypting or decrypting data.
//    /// </summary>
//    public static class EncryptionMechanism
//    {
//        /// <summary>
//        /// The default instance of <see cref="IEncryptionMechanism"/>.
//        /// </summary>
//        public static readonly IEncryptionMechanism DefaultEncryptionMechanism = new ClearTextEncryptionMechanism();

//        private static IEncryptionMechanism _current = DefaultEncryptionMechanism;

//        /// <summary>
//        /// Gets or sets the instance of <see cref="IEncryptionMechanism"/>
//        /// to be used by XSerializer when encrypting or decrypting data.
//        /// When setting this property, if <paramref name="value"/> is null,
//        /// then <see cref="DefaultEncryptionMechanism"/> will be used instead.
//        /// </summary>
//        public static IEncryptionMechanism Current
//        {
//            internal get { return _current; }
//            set { _current = value ?? DefaultEncryptionMechanism; }
//        }
//    }
//}