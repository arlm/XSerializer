using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    public interface IJsonSerializerConfiguration
    {
        Encoding Encoding { get; }
        bool RedactEnabled { get; }
        bool EncryptionEnabled { get; }
        IEncryptionMechanism EncryptionMechanism { get; }
        object EncryptKey { get; }
        bool EncryptRootObject { get; }
    }
}