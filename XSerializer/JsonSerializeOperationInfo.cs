using XSerializer.Encryption;

namespace XSerializer
{
    internal class JsonSerializeOperationInfo : IJsonSerializeOperationInfo
    {
        public bool RedactEnabled { get; set; }
        public bool EncryptionEnabled { get; set; }
        public IEncryptionMechanism EncryptionMechanism { get; set; }
        public object EncryptKey { get; set; }
        public SerializationState SerializationState { get; set; }
        public IDateTimeHandler DateTimeHandler { get; set; }
    }
}