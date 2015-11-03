using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XSerializer
{
    internal class JsonConcreteImplementations
    {
        public static JsonConcreteImplementations Empty = new JsonConcreteImplementations(new Dictionary<Type, Type>(), new Dictionary<PropertyInfo, Type>());

        private readonly IDictionary<Type, Type> _concreteImplementationsByType;
        private readonly IDictionary<PropertyInfo, Type> _concreteImplementationsByProperty;
        
        private readonly List<Tuple<MemberInfo, Type>> _concreteImplementations;
        private readonly int _hashCode;

        public JsonConcreteImplementations(
            IDictionary<Type, Type> concreteImplementationsByType,
            IDictionary<PropertyInfo, Type> concreteImplementationsByProperty)
        {
            _concreteImplementationsByType = concreteImplementationsByType;
            _concreteImplementationsByProperty = concreteImplementationsByProperty;

            _concreteImplementations =
                concreteImplementationsByType.OrderBy(x => x.Key.FullName)
                    .Select(item => new Tuple<MemberInfo, Type>(item.Key, item.Value))
                .Concat(concreteImplementationsByProperty.OrderBy(DeclaringTypeAndPropertyName)
                    .Select(item => new Tuple<MemberInfo, Type>(item.Key, item.Value))).ToList();

            _hashCode =
                unchecked(_concreteImplementations.Aggregate(
                    typeof(JsonConcreteImplementations).GetHashCode(),
                    (currentHashCode, item) => (currentHashCode * 397) ^ item.GetHashCode()));
        }

        public IDictionary<Type, Type> ConcreteImplementationsByType
        {
            get { return _concreteImplementationsByType; }
        }

        public IDictionary<PropertyInfo, Type> ConcreteImplementationsByProperty
        {
            get { return _concreteImplementationsByProperty; }
        }

        private static string DeclaringTypeAndPropertyName(KeyValuePair<PropertyInfo, Type> item)
        {
            return (item.Key.DeclaringType == null ? "" : item.Key.DeclaringType.FullName)
                   + ":" + item.Key.Name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as JsonConcreteImplementations;

            if (other == null)
            {
                return false;
            }

            if (_concreteImplementations.Count != other._concreteImplementations.Count)
            {
                return false;
            }

            var thisEnumerator = _concreteImplementations.GetEnumerator();
            var otherEnumerator = other._concreteImplementations.GetEnumerator();

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