namespace XSerializer
{
    public interface IEncryptionAlgorithm
    {
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
    }
}