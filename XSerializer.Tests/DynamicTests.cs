using NUnit.Framework;
using System;
using System.Collections.Generic;
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
                return string.Format(" xsi:type=\"{0}\"", type.GetXsdType());
            }
        }

        public class DynamicDeserializationTests
        {
            private IXmlSerializer _sut;
            private ClassWithDynamicProperty _expectedDeserializedObject;

            [SetUp]
            public void Setup()
            {
                _sut = CustomSerializer.GetSerializer(typeof(ClassWithDynamicProperty), null, null, null);
                _expectedDeserializedObject = new ClassWithDynamicProperty();
            }

            [Test]
            public void SmokeTest()
            {
                var xml = string.Format(ExpectedXmlFormat, "abc");
                _expectedDeserializedObject.DynamicProperty = "abc";

                var deserializedObject = _sut.DeserializeObject(xml);

                Assert.That(deserializedObject, Has.PropertiesEqualTo(_expectedDeserializedObject));
            }
        }

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
