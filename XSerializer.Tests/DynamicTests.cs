using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml.Serialization;
using System.Collections;
namespace XSerializer.Tests
{
    public class DynamicTests
    {
        private const string ExpectedXmlDynamicNullValue = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithDynamicProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>0</IntProperty>
</ClassWithDynamicProperty>";
        
        private const string ExpectedDynamicXmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithDynamicProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>0</IntProperty>
  <DynamicProperty{0}>{1}</DynamicProperty>
</ClassWithDynamicProperty>";

        private const string ExpectedXmlExpandoNullValue = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithExpandoProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>0</IntProperty>
</ClassWithExpandoProperty>";

        private const string ExpectedExpandoXmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithExpandoProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>0</IntProperty>
  <ExpandoProperty>{0}</ExpandoProperty>
</ClassWithExpandoProperty>";

        [Test]
        public void AClassWithADynamicPropertyUsesCustomSerializer()
        {
            var xmlSerializer = new XmlSerializer<FooWithDeeplyNestedDynamicProperty>();
            Assert.That(xmlSerializer.Serializer, Is.InstanceOf<CustomSerializer<FooWithDeeplyNestedDynamicProperty>>());
        }

        [Test]
        public void AClassWithAnObjectPropertyUsesCustomSerializer()
        {
            var xmlSerializer = new XmlSerializer<FooWithDeeplyNestedObjectProperty>();
            Assert.That(xmlSerializer.Serializer, Is.InstanceOf<CustomSerializer<FooWithDeeplyNestedObjectProperty>>());
        }

        [Test]
        public void AClassWithoutADynamicOrObjectPropertyUsesDefaultSerializerIfLegal()
        {
            var xmlSerializer = new XmlSerializer<FooWithoutDynamicOrObjectProperty>();
            Assert.That(xmlSerializer.Serializer, Is.InstanceOf<DefaultSerializer<FooWithoutDynamicOrObjectProperty>>());
        }

        [TestCase(typeof(ContainerClass<ClassWithNonGenericIEnumerable>))]
        [TestCase(typeof(ContainerClass<ClassWithIEnumerableOfTypeObject>))]
        [TestCase(typeof(ContainerClass<ClassWithNonGenericIDictionary>))]
        [TestCase(typeof(ContainerClass<ClassWithIDictionaryWithObjectKey>))]
        [TestCase(typeof(ContainerClass<ClassWithIDictionaryWithObjectValue>))]
        [TestCase(typeof(ContainerClass<ClassWithIEnumerableOfTypeWithObjectProperty>))]
        [TestCase(typeof(ContainerClass<ClassWithIDictionaryWithKeyOfTypeWithObjectProperty>))]
        [TestCase(typeof(ContainerClass<ClassWithIDictionaryWithValueOfTypeWithObjectProperty>))]
        public void AClassWithAPropertyThatHasAnyRelationWithTypeOfObjectUsesCustomSerializer(Type type)
        {
            var serializerType = typeof(XmlSerializer<>).MakeGenericType(type);
            var ctor = serializerType.GetConstructor(new[] { typeof(Type[]) });
            var serializerProperty = serializerType.GetProperty("Serializer");

            var serializer = ctor.Invoke(new object[] { new Type[0] });
            var serializerImplementation = serializerProperty.GetValue(serializer);

            Assert.That(serializerImplementation, Is.InstanceOf(typeof(CustomSerializer<>).MakeGenericType(type)));
        }

        public class FooWithDeeplyNestedDynamicProperty
        {
            public int Bar { get; set; }
            public BazWithDeeplyNestedDynamicProperty Baz { get; set; }
        }

        public class BazWithDeeplyNestedDynamicProperty
        {
            public string Qux { get; set; }
            public QuirbleWithDeeplyNestedDynamicProperty Quirble { get; set; }
        }

        public class QuirbleWithDeeplyNestedDynamicProperty
        {
            public bool Zaggle { get; set; }
            public dynamic Zirble { get; set; }
        }

        public class FooWithDeeplyNestedObjectProperty
        {
            public int Bar { get; set; }
            public BazWithDeeplyNestedObjectProperty Baz { get; set; }
        }

        public class BazWithDeeplyNestedObjectProperty
        {
            public string Qux { get; set; }
            public QuirbleWithDeeplyNestedObjectProperty Quirble { get; set; }
        }

        public class QuirbleWithDeeplyNestedObjectProperty
        {
            public bool Zaggle { get; set; }
            public object Zirble { get; set; }
        }

        public class FooWithoutDynamicOrObjectProperty
        {
            public int Bar { get; set; }
            public BazWithoutDynamicOrObjectProperty Baz { get; set; }
        }

        public class BazWithoutDynamicOrObjectProperty
        {
            public string Qux { get; set; }
            public QuirbleWithoutDynamicOrObjectProperty Quirble { get; set; }
        }

        public class QuirbleWithoutDynamicOrObjectProperty
        {
            public bool Zaggle { get; set; }
            public string Zirble { get; set; }
        }

        public class DynamicSerializationTestsWithAlwaysEmitTypesSetToFalse : ObjectToXml
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = null },
                    typeof(ClassWithDynamicProperty),
                    ExpectedXmlDynamicNullValue)
                        .SetName("dynamic property - null");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = true },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(bool)), "true"))
                        .SetName("dynamic property - bool");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = 123 },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(int)), "123"))
                        .SetName("dynamic property - int");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = 123.45 },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(double)), "123.45"))
                        .SetName("dynamic property - double");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = 123.45M },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(decimal)), "123.45"))
                        .SetName("dynamic property - decimal");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = "abc" },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(string)), "abc"))
                        .SetName("dynamic property - string");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = new DateTime(2013, 3, 17, 6, 56, 10, 295, DateTimeKind.Utc) },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.295Z"))
                        .SetName("dynamic property - DateTime");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = new Foo { Bar = new Bar { Baz = true }, Qux = "abc" } },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(Foo)), @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "))
                        .SetName("dynamic property - Custom Class");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = GetAnonymousObject() },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(GetAnonymousObject().GetType()), @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "))
                        .SetName("dynamic property - Anonymous Type");

                yield return new TestCaseData(
                    new ClassWithDynamicProperty { DynamicProperty = GetExpandoObject() },
                    typeof(ClassWithDynamicProperty),
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(ExpandoObject)), @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "))
                        .SetName("dynamic property - ExpandoObject");

                yield return new TestCaseData(
                    new ClassWithExpandoProperty { ExpandoProperty = null },
                    typeof(ClassWithExpandoProperty),
                    ExpectedXmlExpandoNullValue)
                        .SetName("ExpandoObject property - null");

                yield return new TestCaseData(
                    new ClassWithExpandoProperty { ExpandoProperty = GetExpandoObject() },
                    typeof(ClassWithExpandoProperty),
                    string.Format(ExpectedExpandoXmlFormat, @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "))
                        .SetName("ExpandoObject property - ExpandoObject");
            }

            private object GetAnonymousObject()
            {
                return new { Bar = new { Baz = true }, Qux = "abc" };
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
                if (type == typeof(ExpandoObject) || type.IsAnonymous())
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
                    ExpectedXmlDynamicNullValue,
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = null })
                        .SetName("dynamic property - null");

                yield return new TestCaseData(
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(bool)), "true"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = true })
                        .SetName("dynamic property - bool");

                yield return new TestCaseData(
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(int)), "123"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = 123 })
                        .SetName("dynamic property - int");

                if (!ShouldSkipUseCase)
                {
                    // There's no way to know whether we should deserialize into a decimal, double, or float.
                    yield return new TestCaseData(
                        string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(double)), "123.45"),
                        typeof(ClassWithDynamicProperty),
                        new ClassWithDynamicProperty { DynamicProperty = 123.45 })
                            .SetName("dynamic property - double");
                }

                yield return new TestCaseData(
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(decimal)), "123.45"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = 123.45M })
                        .SetName("dynamic property - decimal");

                yield return new TestCaseData(
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(string)), "abc"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = "abc" })
                        .SetName("dynamic property - string");

                yield return new TestCaseData(
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.295Z"),
                    typeof(ClassWithDynamicProperty),
                    new ClassWithDynamicProperty { DynamicProperty = new DateTime(2013, 3, 17, 6, 56, 10, 295, DateTimeKind.Utc) })
                        .SetName("dynamic property - DateTime");

                yield return new TestCaseData(
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(Foo)), @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "),
                    typeof(ClassWithDynamicProperty),
                    GetExpected(new ClassWithDynamicProperty { DynamicProperty = new Foo { Bar = new Bar { Baz = true }, Qux = "abc" } }))
                        .SetName("dynamic property - Custom Class");

                yield return new TestCaseData(
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(ExpandoObject)), @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "),
                    typeof(ClassWithDynamicProperty),
                    GetExpected(new ClassWithDynamicProperty { DynamicProperty = GetExpandoObject() }))
                        .SetName("dynamic property - ExpandoObject");

                yield return new TestCaseData(
                    ExpectedXmlExpandoNullValue,
                    typeof(ClassWithExpandoProperty),
                    new ClassWithExpandoProperty { ExpandoProperty = null })
                        .SetName("ExpandoObject property - null");

                yield return new TestCaseData(
                    string.Format(ExpectedExpandoXmlFormat, @"
    <Bar>
      <Baz>true</Baz>
    </Bar>
    <Qux>abc</Qux>
  "),
                    typeof(ClassWithExpandoProperty),
                    new ClassWithExpandoProperty { ExpandoProperty = GetExpandoObject() })
                        .SetName("ExpandoObject property - ExpandoObject");
            }

            protected virtual ClassWithDynamicProperty GetExpected(ClassWithDynamicProperty expected)
            {
                return expected;
            }

            protected virtual string GetXsiTypeString(Type type)
            {
                if (type == typeof(ExpandoObject))
                {
                    return "";
                }

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

        private static ExpandoObject GetExpandoObject()
        {
            dynamic foo = new ExpandoObject();
            foo.Bar = new ExpandoObject();
            foo.Bar.Baz = true;
            foo.Qux = "abc";
            return foo;
        }

        [XmlInclude(typeof(Foo))]
        public class ClassWithDynamicProperty
        {
            public int IntProperty { get; set; }
            public dynamic DynamicProperty { get; set; }
        }

        public class ClassWithExpandoProperty
        {
            public int IntProperty { get; set; }
            public ExpandoObject ExpandoProperty { get; set; }
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

        public class ContainerClass<T>
        {
            public T Item { get; set; }
        }

        public class ClassWithNonGenericIEnumerable
        {
            public ArrayList Items { get; set; }
        }

        public class ClassWithIEnumerableOfTypeObject
        {
            public List<object> Items { get; set; }
        }

        public class ClassWithNonGenericIDictionary
        {
            public Hashtable Map { get; set; }
        }

        public class ClassWithIDictionaryWithObjectKey
        {
            public Dictionary<object, int> Map { get; set; }
        }

        public class ClassWithIDictionaryWithObjectValue
        {
            public Dictionary<int, object> Map { get; set; }
        }

        public class ClassWithIEnumerableOfTypeWithObjectProperty
        {
            public List<ClassWithDynamicProperty> Items { get; set; }
        }

        public class ClassWithIDictionaryWithKeyOfTypeWithObjectProperty
        {
            public Dictionary<int, ClassWithDynamicProperty> Map { get; set; }
        }

        public class ClassWithIDictionaryWithValueOfTypeWithObjectProperty
        {
            public Dictionary<ClassWithDynamicProperty, int> Map { get; set; }
        }
    }
}
