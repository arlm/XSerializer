using System;
using System.Collections.Generic;
using System.Linq;

namespace XSerializer
{
    public interface IOptions
    {
        string DefaultNamespace { get; }
        Type[] ExtraTypes { get; }
        string RootElementName { get; }
    }

    public static class OptionsExtensions
    {
        public static IOptions WithRootElementName(this IOptions options, string rootElementName)
        {
            return new Options
            {
                DefaultNamespace = options.DefaultNamespace,
                ExtraTypes = options.ExtraTypes,
                RootElementName = rootElementName
            };
        }

        public static IOptions WithAdditionalExtraTypes(this IOptions options, params Type[] additionalExtraTypes)
        {
            return options.WithAdditionalExtraTypes((IEnumerable<Type>)additionalExtraTypes);
        }

        public static IOptions WithAdditionalExtraTypes(this IOptions options, IEnumerable<Type> additionalExtraTypes)
        {
            return new Options
            {
                DefaultNamespace = options.DefaultNamespace,
                ExtraTypes = (options.ExtraTypes ?? new Type[0]).Concat(additionalExtraTypes).Distinct().ToArray(),
                RootElementName = options.RootElementName
            };
        }

        private class Options : IOptions
        {
            public string DefaultNamespace { get; set; }
            public Type[] ExtraTypes { get; set; }
            public string RootElementName { get; set; }
        }
    }
}