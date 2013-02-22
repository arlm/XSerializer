using System.Collections;
using System.Linq;
using NUnit.Framework.Constraints;

namespace XSerializer.Tests
{
    using System;

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
                    _failedExpectedValue = string.Format("{0} to be of type {1}", path, expectedValue.GetType().FullName);
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

                foreach (var property in actualValue.GetType().GetProperties().Where(p => p.IsSerializable()))
                {
                    var actualPropertyValue = property.GetValue(actualValue, null);
                    var expectedPropertyValue = property.GetValue(expectedValue, null);

                    var propertyType = property.PropertyType;
                    var propertyName = property.Name;

                    if (!this.DoPropertyValuesMatch(actualPropertyValue, expectedPropertyValue, propertyType, propertyName, path))
                    {
                        return false;
                    }
                }

                return true;
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
                            this._failedExpectedValue = string.Format("{0}.{1} to be \"{2}\"", path, propertyName, expectedPropertyValue);
                            this._failedActualValue = string.Format("\"{0}\"", actualPropertyValue);
                        }
                        else
                        {
                            this._failedExpectedValue = string.Format("{0}.{1} to be {2}", path, propertyName, expectedPropertyValue);
                            this._failedActualValue = actualPropertyValue.ToString();
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
                else
                {
                    if (!this.Matches(actualPropertyValue, expectedPropertyValue, string.Concat(path, ".", propertyName)))
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