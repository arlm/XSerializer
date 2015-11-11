using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XSerializer
{
    internal class JsonMappings
    {
        public static JsonMappings Empty = new JsonMappings(new Dictionary<Type, Type>(), new Dictionary<PropertyInfo, Type>());

        private readonly IDictionary<Type, Type> _mappingsByType;
        private readonly IDictionary<PropertyInfo, Type> _mappingsByProperty;
        
        private readonly List<Tuple<MemberInfo, Type>> _mappings;
        private readonly int _hashCode;

        public JsonMappings(
            IDictionary<Type, Type> mappingsByType,
            IDictionary<PropertyInfo, Type> mappingsByProperty)
        {
            _mappingsByType = mappingsByType;
            _mappingsByProperty = mappingsByProperty;

            _mappings =
                mappingsByType.OrderBy(x => x.Key.FullName)
                    .Select(item => new Tuple<MemberInfo, Type>(item.Key, item.Value))
                .Concat(mappingsByProperty.OrderBy(DeclaringTypeAndPropertyName)
                    .Select(item => new Tuple<MemberInfo, Type>(item.Key, item.Value))).ToList();

            _hashCode =
                unchecked(_mappings.Aggregate(
                    typeof(JsonMappings).GetHashCode(),
                    (currentHashCode, item) => (currentHashCode * 397) ^ item.GetHashCode()));
        }

        public IDictionary<Type, Type> MappingsByType
        {
            get { return _mappingsByType; }
        }

        public IDictionary<PropertyInfo, Type> MappingsByProperty
        {
            get { return _mappingsByProperty; }
        }

        private static string DeclaringTypeAndPropertyName(KeyValuePair<PropertyInfo, Type> item)
        {
            return (item.Key.DeclaringType == null ? "" : item.Key.DeclaringType.FullName)
                   + ":" + item.Key.Name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as JsonMappings;

            if (other == null)
            {
                return false;
            }

            if (_mappings.Count != other._mappings.Count)
            {
                return false;
            }

            var thisEnumerator = _mappings.GetEnumerator();
            var otherEnumerator = other._mappings.GetEnumerator();

            while (thisEnumerator.MoveNext() && otherEnumerator.MoveNext())
            {
                if (!Equals(thisEnumerator.Current, otherEnumerator.Current))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}