using System;
using System.Collections.Generic;
using System.Linq;

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
                RootElementName = rootElementName
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
                RootElementName = options.RootElementName
            };
        }

        private class XmlSerializerOptions : IXmlSerializerOptions
        {
            public string DefaultNamespace { get; set; }
            public Type[] ExtraTypes { get; set; }
            public string RootElementName { get; set; }
        }
    }
}