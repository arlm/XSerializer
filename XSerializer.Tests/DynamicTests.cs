using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
namespace XSerializer.Tests
{
    public class DynamicTests
    {
        private const string ExpectedXmlNullValue = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithDynamicProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>0</IntProperty>
</ClassWithDynamicProperty>";
        
        private const string ExpectedXmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithDynamicProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>0</IntProperty>
  <DynamicProperty{0}>{1}</DynamicProperty>
</ClassWithDynamicProperty>";

        public class DynamicSerializationTestsWithAlwaysEmitTypesSetToFalse : ObjectToXml
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = null },
                    typeof(ClassWithDynamicProperty),
                    ExpectedXmlNullValue)
                        .SetName("null");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = true },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(bool)), "true"))
                        .SetName("bool");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = 123 },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(int)), "123"))
                        .SetName("int");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = 123.45 },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(double)), "123.45"))
                        .SetName("double");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = 123.45M },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(decimal)), "123.45"))
                        .SetName("decimal");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = "abc" },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(string)), "abc"))
                        .SetName("string");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = new DateTime(2013, 3, 17, 6, 56, 10, 295, DateTimeKind.Utc) },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.295Z"))
                        .SetName("DateTime");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = new Foo { Bar = new Bar { Baz = true }, Qux = "abc" } },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(Foo)), @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "))
                        .SetName("Custom Class");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = GetAnonymousObject() },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(GetAnonymousObject().GetType()), @"
    <Foo>Bar</Foo>
    <Baz>
      <Qux>123</Qux>
    </Baz>
  "))
                        .SetName("Anonymous Type");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = GetExpandoObject() },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(GetAnonymousObject().GetType()), @"
    <Foo>Bar</Foo>
    <Baz>
      <Qux>123</Qux>
    </Baz>
  "))
                        .SetName("ExpandoObject");
            }

            private object GetAnonymousObject()
            {
                return new { Foo = "Bar", Baz = new { Qux = 123 } };
            }

            private object GetExpandoObject()
            {
                dynamic obj = new ExpandoObject();
                obj.Foo = "Bar";
                dynamic baz = new ExpandoObject();
                baz.Qux = 123;
                obj.Baz = baz;
                return obj;
            }

            protected virtual string GetXsiTypeString(Type type)
            {
                return "";
            }
        }

        public class DynamicSerializationTestsWithAlwaysEmitTypesSetToTrue : DynamicSerializationTestsWithAlwaysEmitTypesSetToFalse
        {
            protected override bool AlwaysEmitTypes
            {
                get { return true; }
            }

            protected override string GetXsiTypeString(Type type)
            {
                if (type.IsAnonymous())
                {
                    return "";
                }

                return string.Format(" xsi:type=\"{0}\"", type.GetXsdType());
            }
        }

        public class DynamicDeserializationTestsWithWithXsdTypes : XmlToObject
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    ExpectedXmlNullValue,
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = null })
                        .SetName("null");

                yield return new TestCaseData(
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(bool)), "true"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = true })
                        .SetName("bool");

                yield return new TestCaseData(
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(int)), "123"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = 123 })
                        .SetName("int");

                if (!ShouldSkipUseCase)
                {
                    // There's no way to know whether we should deserialize into a decimal, double, or float.
                    yield return new TestCaseData(
                        string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(double)), "123.45"),
                        typeof(ClassWithDynamicProperty),
                        new ClassWithDynamicProperty { DynamicProperty = 123.45 })
                            .SetName("double");
                }

                yield return new TestCaseData(
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(decimal)), "123.45"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = 123.45M })
                        .SetName("decimal");

                yield return new TestCaseData(
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(string)), "abc"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = "abc" })
                        .SetName("string");

                yield return new TestCaseData(
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.295Z"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = new DateTime(2013, 3, 17, 6, 56, 10, 295, DateTimeKind.Utc) })
                        .SetName("DateTime");

                yield return new TestCaseData(
                    string.Format(ExpectedXmlFormat, GetXsiTypeString(typeof(Foo)), @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "),
                    typeof(ClassWithDynamicProperty),
                    GetExpected(new ClassWithDynamicProperty { DynamicProperty = new Foo { Bar = new Bar { Baz = true }, Qux = "abc" } }))
                        .SetName("Custom Class");
            }

            protected virtual ClassWithDynamicProperty GetExpected(ClassWithDynamicProperty expected)
            {
                return expected;
            }

            protected virtual string GetXsiTypeString(Type type)
            {
                return string.Format(" xsi:type=\"{0}\"", type.GetXsdType());
            }

            protected virtual bool ShouldSkipUseCase
            {
                get { return false; }
            }
        }

        public class DynamicDeserializationTestsWithWithoutXsdTypes : DynamicDeserializationTestsWithWithXsdTypes
        {
            protected override ClassWithDynamicProperty GetExpected(ClassWithDynamicProperty expected)
            {
                if (expected == null)
                {
                    return null;
                }

                dynamic foo = new ExpandoObject();
                foo.Bar = new ExpandoObject();
                foo.Bar.Baz = expected.DynamicProperty.Bar.Baz;
                foo.Qux = expected.DynamicProperty.Qux;
                expected.DynamicProperty = foo;

                return expected;
            }

            protected override string GetXsiTypeString(Type type)
            {
                return "";
            }

            protected override bool ShouldSkipUseCase
            {
                get { return true; }
            }
        }

        [XmlInclude(typeof(Foo))]
        public class ClassWithDynamicProperty
        {
            public int IntProperty { get; set; }
            public dynamic DynamicProperty { get; set; }
        }

        public class Foo
        {
            public Bar Bar { get; set; }
            public string Qux { get; set; }
        }

        public class Bar
        {
            public bool Baz { get; set; }
        }
    }
}
