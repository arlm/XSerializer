using System;

namespace XSerializer.Encryption
{
    /// <summary>
    /// Indicates that the value of a property should be encrypted or decrypted,
    /// depending on whether the current serialization operation is configured
    /// to do so.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class EncryptAttribute : Attribute
    {
    }
}
