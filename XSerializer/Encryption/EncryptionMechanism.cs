using XSerializer.Rock.StaticDependencyInjection;

namespace XSerializer.Encryption
{
    /// <summary>
    /// Provides a means for an application to specify a default instance of
    /// <see cref="IEncryptionMechanism"/> to be used by XSerializer when encrypting
    /// or decrypting data.
    /// </summary>
    public static class EncryptionMechanism
    {
        private static readonly Default<IEncryptionMechanism> _defaultEncryptionMechanism = new Default<IEncryptionMechanism>(() => new ClearTextEncryptionMechanism());

        /// <summary>
        /// Sets the default instance of <see cref="IEncryptionMechanism"/> to be used by
        /// XSerializer when encrypting or decrypting data and an <see cref="IEncryptionMechanism"/>
        /// is not otherwise specified. If this property is never set, it will have a value of type
        /// <see cref="ClearTextEncryptionMechanism"/>. When set, if <paramref name="value"/> is
        /// null, then an instance of <see cref="ClearTextEncryptionMechanism"/> will be used
        /// instead.
        /// </summary>
        public static IEncryptionMechanism Default
        {
            set { _defaultEncryptionMechanism.SetCurrent(value); }
        }

        /// <summary>
        /// Gets the <see cref="IEncryptionMechanism"/> specified by the
        /// <see cref="ISerializeOptions.EncryptionMechanism"/> property of the
        /// <paramref name="options"/> parameter. If that value is null, then the value specified
        /// by the static <see cref="Default"/> property of the <see cref="EncryptionMechanism"/>
        /// class is returned.
        /// </summary>
        internal static IEncryptionMechanism GetEncryptionMechanism(this ISerializeOptions options)
        {
            return options.EncryptionMechanism ?? _defaultEncryptionMechanism.Current;
        }
    }
}