using XSerializer.Encryption;

namespace XSerializer
{
    internal interface IJsonSerializeOperationInfo
    {
        bool RedactEnabled { get; }
        bool EncryptionEnabled { get; }
        IEncryptionMechanism EncryptionMechanism { get; }
        object EncryptKey { get; }
        SerializationState SerializationState { get; }
    }
}