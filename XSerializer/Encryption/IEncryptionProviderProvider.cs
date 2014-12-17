namespace XSerializer.Encryption
{
    public interface IEncryptionProviderProvider
    {
        IEncryptionProvider GetEncryptionProvider();
    }
}