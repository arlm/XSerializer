using XSerializer.Encryption;

namespace XSerializer.Tests.Encryption
{
    public class UnitTestEncryptionProviderProvider :IEncryptionProviderProvider
    {
        public IEncryptionProvider GetEncryptionProvider()
        {
            return new Base64EncryptionProvider();
        }
    }
}
