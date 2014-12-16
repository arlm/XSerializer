using System;

namespace XSerializer
{
    public interface IEncryptionAlgorithmFactory
    {
        IEncryptionAlgorithm GetAlgorithm(Type type);
    }
}