using System;
using System.Linq;

namespace XSerializer
{
    public static class EncryptionAlgorithm
    {
        public static readonly IEncryptionAlgorithm _defaultDefaultEncryptionAlgorithm = new LousyEncryptionAlgorithm();

        public static IEncryptionAlgorithm _defaultEncryptionAlgorithm = _defaultDefaultEncryptionAlgorithm;

        public static IEncryptionAlgorithm Default
        {
            get { return _defaultEncryptionAlgorithm; }
            set { _defaultEncryptionAlgorithm = value ?? _defaultDefaultEncryptionAlgorithm; }
        }

        private class LousyEncryptionAlgorithm : IEncryptionAlgorithm
        {
            public string Encrypt(string plaintext)
            {
                return string.Concat(plaintext.Select(c => (char)(c + 1)));
            }

            public string Decrypt(string ciphertext)
            {
                return string.Concat(ciphertext.Select(c => (char)(c - 1)));
            }
        }
    }

    public static class EncryptionAlgorithmFactory
    {
        private static readonly IEncryptionAlgorithmFactory _defaultEncryptionAlgorithmFactory = new DefaultEncryptionAlgorithmFactory();

        private static IEncryptionAlgorithmFactory _current = _defaultEncryptionAlgorithmFactory;

        public static IEncryptionAlgorithmFactory Current
        {
            get { return _current; }
            set
            {
                _current = value ?? _defaultEncryptionAlgorithmFactory;
            }
        }

        private class DefaultEncryptionAlgorithmFactory : IEncryptionAlgorithmFactory
        {
            public IEncryptionAlgorithm GetAlgorithm(Type type)
            {
                return EncryptionAlgorithm.Default;
            }
        }
    }
}