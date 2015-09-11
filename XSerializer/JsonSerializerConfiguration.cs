using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    public class JsonSerializerConfiguration : IJsonSerializerConfiguration
    {
        public JsonSerializerConfiguration()
        {
            Encoding = Encoding.UTF8;
        }

        public Encoding Encoding { get; set; }
        public bool RedactEnabled { get; set; }
        public bool EncryptionEnabled { get; set; }
        public IEncryptionMechanism EncryptionMechanism { get; set; }
        public object EncryptKey { get; set; }
        public bool EncryptRootObject { get; set; }
    }
}