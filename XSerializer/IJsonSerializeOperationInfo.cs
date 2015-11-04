using XSerializer.Encryption;

namespace XSerializer
{
    internal interface IJsonSerializeOperationInfo
    {
        bool RedactEnabled { get; }
        IEncryptionMechanism EncryptionMechanism { get; }
        object EncryptKey { get; }
        SerializationState SerializationState { get; }
        IDateTimeHandler DateTimeHandler { get; }
    }
}