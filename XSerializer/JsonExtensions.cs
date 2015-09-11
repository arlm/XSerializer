using System;
using XSerializer.Encryption;

namespace XSerializer
{
    internal static class JsonExtensions
    {
        public static IJsonSerializeOperationInfo WithNewSerializationState(this IJsonSerializeOperationInfo info)
        {
            return new JsonSerializeOperationInfo
            {
                RedactEnabled = info.RedactEnabled,
                EncryptionEnabled = info.EncryptionEnabled,
                EncryptionMechanism = info.EncryptionMechanism,
                EncryptKey = info.EncryptKey,
                SerializationState = new SerializationState()
            };
        }
    }
}