using System;
using System.Collections.Generic;
using System.Linq;
using CacheKey = System.Tuple<System.Type, XSerializer.Encryption.EncryptAttribute, XSerializer.IXmlSerializerOptions>;

namespace XSerializer
{
    internal class CacheKeyEqualityComparer : IEqualityComparer<CacheKey>
    {
        public bool Equals(CacheKey lhs, CacheKey rhs)
        {
            var lhsType = lhs.Item1;
            var lhsEncryptAttribute = lhs.Item2;
            var lhsOptions = lhs.Item3;

            var rhsType = rhs.Item1;
            var rhsEncryptAttribute = rhs.Item2;
            var rhsOptions = rhs.Item3;

            if (lhsType != rhsType) return false;

            if (lhsOptions.DefaultNamespace != rhsOptions.DefaultNamespace) return false;

            if ((lhsOptions.ExtraTypes == null) != (rhsOptions.ExtraTypes == null)) return false;
            if (lhsOptions.ExtraTypes != null)
            {
                var lhsExtraTypes = lhsOptions.ExtraTypes
                    .Where(extraType => extraType != null)
                    .Distinct(EqualityComparer<Type>.Default)
                    .OrderBy(extraType => extraType.FullName)
                    .GetEnumerator();
                var rhsExtraTypes = rhsOptions.ExtraTypes
                    .Where(extraType => extraType != null)
                    .Distinct(EqualityComparer<Type>.Default)
                    .OrderBy(extraType => extraType.FullName)
                    .GetEnumerator();
                while (true)
                {
                    bool hasNext;
                    if ((hasNext = lhsExtraTypes.MoveNext()) != rhsExtraTypes.MoveNext()) return false;
                    if (!hasNext) break;
                    if (lhsExtraTypes.Current != rhsExtraTypes.Current) return false;
                }
            }

            var lhsRootElementName = string.IsNullOrWhiteSpace(lhsOptions.RootElementName) ? lhsType.Name : lhsOptions.RootElementName;
            var rhsRootElementName = string.IsNullOrWhiteSpace(rhsOptions.RootElementName) ? rhsType.Name : rhsOptions.RootElementName;
            if (lhsRootElementName != rhsRootElementName) return false;

            if ((lhsOptions.RedactAttribute == null) != (rhsOptions.RedactAttribute == null)) return false;

            if ((lhsEncryptAttribute == null) != (rhsEncryptAttribute == null)) return false;

            if (lhsOptions.TreatEmptyElementAsString != rhsOptions.TreatEmptyElementAsString) return false;

            if (lhsOptions.ShouldAlwaysEmitNil != rhsOptions.ShouldAlwaysEmitNil) return false;

            if (lhsOptions.ShouldUseAttributeDefinedInInterface != rhsOptions.ShouldUseAttributeDefinedInInterface) return false;

            return true;
        }

        public int GetHashCode(CacheKey cacheKey)
        {
            unchecked
            {
                var type = cacheKey.Item1;
                var encryptAttribute = cacheKey.Item2;
                var options = cacheKey.Item3;

                var key = type.GetHashCode();

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(options.DefaultNamespace) ? "" : options.DefaultNamespace).GetHashCode();

                if (options.ExtraTypes != null)
                {
                    key = options.ExtraTypes
                        .Where(extraType => extraType != null)
                        .Distinct(EqualityComparer<Type>.Default)
                        .OrderBy(extraType => extraType.FullName)
                        .Aggregate(key, (current, extraType) => (current * 397) ^ extraType.GetHashCode());
                }

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(options.RootElementName) ? type.Name : options.RootElementName).GetHashCode();

                key = (key * 397) ^ (options.RedactAttribute != null).GetHashCode();

                key = (key * 397) ^ (encryptAttribute != null).GetHashCode();

                key = (key * 397) ^ options.TreatEmptyElementAsString.GetHashCode();

                key = (key * 397) ^ options.ShouldAlwaysEmitNil.GetHashCode();

                key = (key * 397) ^ options.ShouldUseAttributeDefinedInInterface.GetHashCode();

                return key;
            }
        }
    }
}
