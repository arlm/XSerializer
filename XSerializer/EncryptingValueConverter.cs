using System;

namespace XSerializer
{
    internal class EncryptingValueConverter : IValueConverter
    {
        private readonly IValueConverter _decoratedValueConverter;
        private readonly IEncryptionAlgorithm _encryptionAlgorithm;

        public EncryptingValueConverter(
            IValueConverter decoratedValueConverter,
            IEncryptionAlgorithmFactory encryptionAlgorithmFactory)
        {
            _decoratedValueConverter = decoratedValueConverter;
            _encryptionAlgorithm = encryptionAlgorithmFactory.GetAlgorithm(decoratedValueConverter.Type);
        }

        public Type Type { get { return _decoratedValueConverter.Type; } }

        public object ParseString(string value)
        {
            string plainTextValue;

            try
            {
                plainTextValue = _encryptionAlgorithm.Decrypt(value);
            }
            catch
            {
                plainTextValue = value;
            }

            return _decoratedValueConverter.ParseString(plainTextValue);
        }

        public string GetString(object value, ISerializeOptions options)
        {
            var returnValue = _decoratedValueConverter.GetString(value, options);

            if (options.ShouldEncrypt)
            {
                returnValue = _encryptionAlgorithm.Encrypt(returnValue);
            }

            return returnValue;
        }
    }
}