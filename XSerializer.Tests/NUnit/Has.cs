using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Constraints;

namespace XSerializer.Tests
{
    internal static class Has
    {
        public static IResolveConstraint PropertiesEqualTo(object expected)
        {
            return new ResolveEqualPropertiesConstraint(expected);
        }

        private class ResolveEqualPropertiesConstraint : IResolveConstraint
        {
            private readonly object _expected;

            public ResolveEqualPropertiesConstraint(object expected)
            {
                _expected = expected;
            }

            public Constraint Resolve()
            {
                return new EqualPropertiesConstraint(_expected);
            }
        }

        private class EqualPropertiesConstraint : Constraint
        {
            private readonly object _expectedValue;
            private object _failedExpectedValue;
            private object _failedActualValue;

            public EqualPropertiesConstraint(object expectedValue)
            {
                _expectedValue = expectedValue;
            }

            public override bool Matches(object actualValue)
            {
                return Matches(actualValue, _expectedValue, null);
            }

            private bool Matches(object actualValue, object expectedValue, string path)
            {
                if (AreSimpleTypes(actualValue, expectedValue))
                {
                    if (Equals(expectedValue, actualValue))
                    {
                        return true;
                    }

                    _failedExpectedValue = expectedValue;
                    _failedActualValue = actualValue;
                    return false;
                }

                if (actualValue == null && expectedValue == null)
                {
                    return true;
                }

                if (actualValue == null)
                {
                    _failedExpectedValue =
                        path == null
                        ? string.Format("instance of {0} to be not null", expectedValue.GetType().Name)
                        : string.Format("{0} to be not null", path);
                    _failedActualValue = "null";
                    return false;
                }

                if (expectedValue == null)
                {
                    _failedExpectedValue =
                        path == null
                        ? "instance to be null"
                        : string.Format("{0} to be null", path);
                    _failedActualValue = "not null";
                    return false;
                }

                if (actualValue.GetType() != expectedValue.GetType())
                {
                    _failedExpectedValue =
                        path == null
                        ? string.Format("instance to type of {0}", expectedValue.GetType())
                        : string.Format("{0} to be type of {1}", path, expectedValue.GetType());
                    _failedActualValue = string.Format("type of {0}", actualValue.GetType());
                    return false;
                }

                if (path == null)
                {
                    path = actualValue.GetType().Name;
                }

                IEnumerable<Property> properties;

                var actualExpando = actualValue as ExpandoObject;
                if (actualExpando != null)
                {
                    var actualExpandoMap = (IDictionary<string, object>)actualValue;
                    var expectedExpandoMap = (IDictionary<string, object>)expectedValue;

                    foreach (var expectedKey in expectedExpandoMap.Keys)
                    {
                        if (!actualExpandoMap.ContainsKey(expectedKey))
                        {
                            _failedExpectedValue =
                                path == null
                                ? string.Format("ExpandoObject to have a {0} property", expectedKey)
                                : string.Format("{0} to have a {1} property", path, expectedKey);
                            _failedActualValue = string.Format("no {0} property", expectedKey);
                            return false;
                        }
                    }

                    foreach (var actualKey in actualExpandoMap.Keys)
                    {
                        if (!expectedExpandoMap.ContainsKey(actualKey))
                        {
                            _failedExpectedValue =
                                path == null
                                ? string.Format("ExpandoObject to not have a {0} property", actualKey)
                                : string.Format("{0} to not have a {1} property", path, actualKey);
                            _failedActualValue = string.Format("a {0} property", actualKey);
                            return false;
                        }
                    }

                    properties = actualExpando.Select(item => Property.FromActual(item, (ExpandoObject)expectedValue));
                }
                else
                {
                    properties = actualValue.GetType().GetProperties().Where(p => p.IsSerializable()).Select(p => new Property(p, actualValue, expectedValue));
                }

                return properties.All(property => DoPropertyValuesMatch(property.ActualValue, property.ExpectedValue, property.Type, property.Name, path));
            }

            private class Property
            {
                private Property()
                {
                }

                public Property(PropertyInfo propertyInfo, object actualContainer, object expectedContainer)
                {
                    Name = propertyInfo.Name;
                    Type = propertyInfo.PropertyType;
                    ActualValue = propertyInfo.GetValue(actualContainer, null);
                    ExpectedValue = propertyInfo.GetValue(expectedContainer, null);
                }

                public static Property FromActual(KeyValuePair<string, object> actualItem, IDictionary<string, object> expectedValuesMap)
                {
                    var property = new Property();
                    property.Name = actualItem.Key;
                    property.Type = actualItem.Value == null ? typeof(object) : actualItem.Value.GetType();
                    property.ActualValue = actualItem.Value;
                    object expectedValue;
                    expectedValuesMap.TryGetValue(actualItem.Key, out expectedValue);
                    property.ExpectedValue = expectedValue;
                    if (property.ActualValue == null && property.ExpectedValue != null)
                    {
                        property.Type = property.ExpectedValue.GetType();
                    }
                    return property;
                }

                public static Property FromExpected(KeyValuePair<string, object> expectedItem, IDictionary<string, object> actualValuesMap)
                {
                    var property = new Property();
                    property.Name = expectedItem.Key;
                    property.Type = expectedItem.Value == null ? typeof(object) : expectedItem.Value.GetType();
                    property.ExpectedValue = expectedItem.Value;
                    object actualValue;
                    actualValuesMap.TryGetValue(expectedItem.Key, out actualValue);
                    property.ActualValue = actualValue;
                    if (property.ExpectedValue == null && property.ActualValue != null)
                    {
                        property.Type = property.ActualValue.GetType();
                    }
                    return property;
                }

                public string Name { get; private set; }
                public Type Type { get; private set; }
                public object ActualValue { get; private set; }
                public object ExpectedValue { get; private set; }
            }

            private bool AreSimpleTypes(object actualValue, object expectedValue)
            {
                if (actualValue == null || expectedValue == null)
                {
                    return false;
                }

                var actualValueType = actualValue.GetType();
                var expectedValueType = expectedValue.GetType();

                return (actualValueType.IsValueType && expectedValueType.IsValueType)
                       || (actualValueType == typeof(string) && expectedValueType == typeof(string));
            }

            private bool DoPropertyValuesMatch(object actualPropertyValue, object expectedPropertyValue, Type propertyType, string propertyName, string path)
            {
                if (propertyType.IsValueType || propertyType == typeof(string))
                {
                    if (!Equals(actualPropertyValue, expectedPropertyValue))
                    {
                        if (propertyType == typeof(string))
                        {
                            _failedExpectedValue = string.Format("{0}.{1} to be \"{2}\"", path, propertyName, expectedPropertyValue);
                            _failedActualValue = string.Format("\"{0}\"", actualPropertyValue);
                        }
                        else
                        {
                            _failedExpectedValue = string.Format("{0}.{1} to be {2}", path, propertyName, expectedPropertyValue);
                            _failedActualValue = actualPropertyValue.ToString();
                        }

                        return false;
                    }
                }
                else if (typeof(IDictionary).IsAssignableFrom(propertyType))
                {
                    var actualDictionary = (IDictionary)actualPropertyValue;
                    var expectedDictionary = (IDictionary)expectedPropertyValue;

                    var actualEnumerator = actualDictionary.GetEnumerator();
                    while (actualEnumerator.MoveNext())
                    {
                        if (!expectedDictionary.Contains(actualEnumerator.Key))
                        {
                            return false;
                        }

                        if (!Matches(actualEnumerator.Value, expectedDictionary[actualEnumerator.Key], string.Format("{0}.{1}[]", path, propertyName)))
                        {
                            return false;
                        }
                    }

                    var expectedEnumerator = expectedDictionary.GetEnumerator();
                    while (expectedEnumerator.MoveNext())
                    {
                        if (!actualDictionary.Contains(expectedEnumerator.Key))
                        {
                            return false;
                        }

                        if (!Matches(expectedEnumerator.Value, actualDictionary[expectedEnumerator.Key], path + "[]"))
                        {
                            return false;
                        }
                    }
                }
                else if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(ExpandoObject))
                {
                    var actualCollection = (IEnumerable)actualPropertyValue;
                    var expectedCollection = (IEnumerable)expectedPropertyValue;

                    var actualEnumerator = actualCollection.GetEnumerator();
                    var expectedEnumerator = expectedCollection.GetEnumerator();

                    while (true)
                    {
                        var actualMoveNext = actualEnumerator.MoveNext();
                        var expectedMoveNext = expectedEnumerator.MoveNext();

                        if (actualMoveNext != expectedMoveNext)
                        {
                            return false;
                        }

                        if (!actualMoveNext)
                        {
                            break;
                        }

                        if (!Matches(actualEnumerator.Current, expectedEnumerator.Current, string.Format("{0}.{1}[]", path, propertyName)))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (!Matches(actualPropertyValue, expectedPropertyValue, string.Concat(path, ".", propertyName)))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override void WriteDescriptionTo(MessageWriter writer)
            {
                writer.Write(_failedExpectedValue);
            }

            public override void WriteActualValueTo(MessageWriter writer)
            {
                writer.Write(_failedActualValue);
            }
        }
    }
}