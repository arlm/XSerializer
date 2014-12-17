using System;
using System.Collections.Generic;
using System.Linq;
using XSerializer.Encryption;

namespace XSerializer
{
    public static class OptionsExtensions
    {
        public static IXmlSerializerOptions WithRootElementName(this IXmlSerializerOptions options, string rootElementName)
        {
            return new XmlSerializerOptions
            {
                DefaultNamespace = options.DefaultNamespace,
                ExtraTypes = options.ExtraTypes,
                RootElementName = rootElementName,
                RedactAttribute = options.RedactAttribute,
                EncryptAttribute = options.EncryptAttribute,
                TreatEmptyElementAsString = options.TreatEmptyElementAsString,
                ShouldAlwaysEmitNil = options.ShouldAlwaysEmitNil
            };
        }

        public static IXmlSerializerOptions WithAdditionalExtraTypes(this IXmlSerializerOptions options, params Type[] additionalExtraTypes)
        {
            return options.WithAdditionalExtraTypes((IEnumerable<Type>)additionalExtraTypes);
        }

        public static IXmlSerializerOptions WithAdditionalExtraTypes(this IXmlSerializerOptions options, IEnumerable<Type> additionalExtraTypes)
        {
            return new XmlSerializerOptions
            {
                DefaultNamespace = options.DefaultNamespace,
                ExtraTypes = (options.ExtraTypes ?? new Type[0]).Concat(additionalExtraTypes).Distinct().ToArray(),
                RootElementName = options.RootElementName,
                RedactAttribute = options.RedactAttribute,
                EncryptAttribute = options.EncryptAttribute,
                TreatEmptyElementAsString = options.TreatEmptyElementAsString,
                ShouldAlwaysEmitNil = options.ShouldAlwaysEmitNil
            };
        }

        public static IXmlSerializerOptions WithRedactAttribute(this IXmlSerializerOptions options, RedactAttribute redactAttribute)
        {
            return new XmlSerializerOptions
            {
                DefaultNamespace = options.DefaultNamespace,
                ExtraTypes = options.ExtraTypes,
                RootElementName = options.RootElementName,
                RedactAttribute = redactAttribute,
                EncryptAttribute = options.EncryptAttribute,
                TreatEmptyElementAsString = options.TreatEmptyElementAsString,
                ShouldAlwaysEmitNil = options.ShouldAlwaysEmitNil
            };
        }

        public static IXmlSerializerOptions WithEncryptAttribute(this IXmlSerializerOptions options, EncryptAttribute encryptAttribute)
        {
            return new XmlSerializerOptions
            {
                DefaultNamespace = options.DefaultNamespace,
                ExtraTypes = options.ExtraTypes,
                RootElementName = options.RootElementName,
                RedactAttribute = options.RedactAttribute,
                EncryptAttribute = encryptAttribute,
                TreatEmptyElementAsString = options.TreatEmptyElementAsString,
                ShouldAlwaysEmitNil = options.ShouldAlwaysEmitNil
            };
        }

        public static IXmlSerializerOptions AlwaysEmitNil(this IXmlSerializerOptions options)
        {
            return new XmlSerializerOptions
            {
                DefaultNamespace = options.DefaultNamespace,
                ExtraTypes = options.ExtraTypes,
                RootElementName = options.RootElementName,
                RedactAttribute = options.RedactAttribute,
                EncryptAttribute = options.EncryptAttribute,
                TreatEmptyElementAsString = options.TreatEmptyElementAsString,
                ShouldAlwaysEmitNil = true
            };
        }

        private class XmlSerializerOptions : IXmlSerializerOptions
        {
            public string DefaultNamespace { get; set; }
            public Type[] ExtraTypes { get; set; }
            public string RootElementName { get; set; }
            public RedactAttribute RedactAttribute { get; set; }
            public EncryptAttribute EncryptAttribute { get; set; }
            public bool TreatEmptyElementAsString { get; set; }
            public bool ShouldAlwaysEmitNil { get; set; }
        }
    }
}