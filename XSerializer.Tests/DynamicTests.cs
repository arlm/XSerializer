using System.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml.Serialization;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;

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
        public void AClassWithoutADynamicOrObjectPropertyUsesCustomSerializer()
        {
            var xmlSerializer = new XmlSerializer<FooWithoutDynamicOrObjectProperty>();
            Assert.That(xmlSerializer.Serializer, Is.InstanceOf<CustomSerializer<FooWithoutDynamicOrObjectProperty>>());
        }

        [Test]
        public void ADynamicPropertyWithAZeroPaddedNumberValueRoundTripsFromXmlCorrectly()
        {
            dynamic dynamicProperty = new ExpandoObject();
            dynamicProperty.Qwerty = "000123";

            var instance = new ClassWithDynamicProperty
            {
                IntProperty = 789,
                DynamicProperty = dynamicProperty
            };

            var serializer = new XmlSerializer<ClassWithDynamicProperty>(options => options.Indent());

            var xml = serializer.Serialize(instance);
            var roundTripInstance = serializer.Deserialize(xml);
            var roundTripXml = serializer.Serialize(roundTripInstance);
            
            Assert.That(roundTripInstance.IntProperty, Is.EqualTo(instance.IntProperty));
            Assert.That(roundTripInstance.DynamicProperty.Qwerty, Is.EqualTo(instance.DynamicProperty.Qwerty));

            Assert.That(roundTripXml, Is.EqualTo(xml));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ADynamicPropertyWithAnEmptyValueRoundTripsFromXmlCorrectly(bool shouldTreatEmptyElementAsString)
        {
            dynamic dynamicProperty = new ExpandoObject();
            dynamicProperty.Qwerty = new object();
            dynamicProperty.Uiop = "";

            var instance = new ClassWithDynamicProperty
            {
                IntProperty = 123,
                DynamicProperty = dynamicProperty
            };

            var serializer = new XmlSerializer<ClassWithDynamicProperty>(options =>
            {
                if (shouldTreatEmptyElementAsString)
                {
                    options.Indent().ShouldTreatEmptyElementAsString();
                }
                else
                {
                    options.Indent();
                }
            });

            var xml = serializer.Serialize(instance);
            var roundTripInstance = serializer.Deserialize(xml);
            var roundTripXml = serializer.Serialize(roundTripInstance);

            if (shouldTreatEmptyElementAsString)
            {
                Assert.That(roundTripInstance.IntProperty, Is.EqualTo(instance.IntProperty));
                Assert.That(roundTripInstance.DynamicProperty.Uiop, Is.EqualTo(instance.DynamicProperty.Uiop));

                // When we treat empty elements as strings, we won't deserialize as the original type.
                Assert.That(roundTripInstance.DynamicProperty.Qwerty, Is.Not.EqualTo(instance.DynamicProperty.Qwerty));
                Assert.That(roundTripInstance.DynamicProperty.Qwerty, Is.EqualTo(""));
            }
            else
            {
                Assert.That(roundTripInstance, Has.PropertiesEqualTo(instance));
            }

            Assert.That(roundTripXml, Is.EqualTo(xml));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AnObjectPropertyWithAnEmptyValueRoundTripsFromXmlCorrectly(bool shouldTreatEmptyElementAsString)
        {
            var instance = new ClassWithObjectProperty
            {
                IntProperty = 123,
                ObjectProperty = ""
            };

            var serializer = new XmlSerializer<ClassWithObjectProperty>(options =>
            {
                if (shouldTreatEmptyElementAsString)
                {
                    options.Indent().ShouldTreatEmptyElementAsString();
                }
                else
                {
                    options.Indent();
                }
            });

            var xml = serializer.Serialize(instance);
            var roundTripInstance = serializer.Deserialize(xml);
            var roundTripXml = serializer.Serialize(roundTripInstance);

            Assert.That(roundTripInstance, Has.PropertiesEqualTo(instance));
            Assert.That(roundTripXml, Is.EqualTo(xml));
        }

        public class GivenADynamicPropertyWhenAnEmptyXmlElementIsDeserialized
        {
            private const string xmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithDynamicProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>123</IntProperty>
  <DynamicProperty>
    {0}
    {1}
  </DynamicProperty>
</ClassWithDynamicProperty>";

            [TestCase("<Qwerty />", "<Uiop />")]
            [TestCase("<Qwerty></Qwerty>", "<Uiop></Uiop>")]
            public void AndTreatEmptyElementAsStringIsTrueThenTheTypeOfThePropertyIsString(string emptyElement1, string emptyElement2)
            {
                var serializer = new XmlSerializer<ClassWithDynamicProperty>(options => options.ShouldTreatEmptyElementAsString());

                var instance = serializer.Deserialize(string.Format(xmlFormat, emptyElement1, emptyElement2));

                Assert.That(instance.DynamicProperty.Qwerty, Is.TypeOf<string>());
                Assert.That(instance.DynamicProperty.Uiop, Is.TypeOf<string>());
            }

            [TestCase("<Qwerty />", "<Uiop />")]
            [TestCase("<Qwerty></Qwerty>", "<Uiop></Uiop>")]
            public void AndTreatEmptyElementAsStringIsFalseThenTheTypeOfThePropertyIsNotString(string emptyElement1, string emptyElement2)
            {
                var serializer = new XmlSerializer<ClassWithDynamicProperty>();

                var instance = serializer.Deserialize(string.Format(xmlFormat, emptyElement1, emptyElement2));

                Assert.That(instance.DynamicProperty.Qwerty, Is.Not.TypeOf<string>());
                Assert.That(instance.DynamicProperty.Uiop, Is.Not.TypeOf<string>());
            }
        }

        public class GivenAObjectPropertyWhenAnEmptyXmlElementIsDeserialized
        {
            private const string xmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ClassWithObjectProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IntProperty>123</IntProperty>
  {0}
</ClassWithObjectProperty>";

            [TestCase("<ObjectProperty />")]
            [TestCase("<ObjectProperty></ObjectProperty>")]
            public void AndTreatEmptyElementAsStringIsTrueThenTheTypeOfThePropertyIsString(string emptyElement)
            {
                var serializer = new XmlSerializer<ClassWithObjectProperty>(options => options.ShouldTreatEmptyElementAsString());

                var instance = serializer.Deserialize(string.Format(xmlFormat, emptyElement));

                Assert.That(instance.ObjectProperty, Is.TypeOf<string>());
            }

            [TestCase("<ObjectProperty />")]
            [TestCase("<ObjectProperty></ObjectProperty>")]
            public void AndTreatEmptyElementAsStringIsFalseThenTheTypeOfThePropertyIsNotString(string emptyElement)
            {
                var serializer = new XmlSerializer<ClassWithObjectProperty>();

                var instance = serializer.Deserialize(string.Format(xmlFormat, emptyElement));

                Assert.That(instance.ObjectProperty, Is.Not.TypeOf<string>());
            }
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
            var serializerProperty = serializerType.GetProperty("Serializer", BindingFlags.Instance | BindingFlags.NonPublic);

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

        internal class DynamicSerializationTestsWithAlwaysEmitTypesSetToFalse
        {
            [TestCaseSource("TestCaseData")]
            public void SerializesCorrectly(object instance, Type type, string expectedXml)
            {
                var customSerializer = GetSerializer(type);

                var customXml = customSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions(shouldAlwaysEmitTypes: AlwaysEmitTypes));

                Console.WriteLine("Expected XML:");
                Console.WriteLine(expectedXml);
                Console.WriteLine();
                Console.WriteLine("Actual XML:");
                Console.WriteLine(customXml);

                Assert.That(customXml, Is.EqualTo(expectedXml));
            }

            private static IXmlSerializerInternal GetSerializer(Type type)
            {
                return CustomSerializer.GetSerializer(type, null, TestXmlSerializerOptions.Empty);
            }

            private static bool AlwaysEmitTypes
            {
                get { return false; }
            }

            private static IEnumerable<TestCaseData> TestCaseData
            {
                get
                {
                    return GetTestCaseData().Select(testCaseData =>
                    {
                        if (string.IsNullOrWhiteSpace(testCaseData.TestName))
                        {
                            var instanceType = testCaseData.Arguments[0].GetType();
                            var type = (Type)testCaseData.Arguments[1];

                            return testCaseData.SetName(type == instanceType ? type.Name : string.Format("{0} as {1}", instanceType.Name, type.Name));
                        }

                        return testCaseData;
                    });
                }
            }

            private static IEnumerable<TestCaseData> GetTestCaseData()
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
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.2950000Z"))
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

            private static object GetAnonymousObject()
            {
                return new { Bar = new { Baz = true }, Qux = "abc" };
            }

            private static string GetXsiTypeString(Type type)
            {
                return "";
            }
        }

        internal class DynamicSerializationTestsWithAlwaysEmitTypesSetToTrue// : DynamicSerializationTestsWithAlwaysEmitTypesSetToFalse
        {
            [TestCaseSource("TestCaseData")]
            public void SerializesCorrectly(object instance, Type type, string expectedXml)
            {
                var customSerializer = GetSerializer(type);

                var customXml = customSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions(shouldAlwaysEmitTypes: AlwaysEmitTypes));

                Console.WriteLine("Expected XML:");
                Console.WriteLine(expectedXml);
                Console.WriteLine();
                Console.WriteLine("Actual XML:");
                Console.WriteLine(customXml);

                Assert.That(customXml, Is.EqualTo(expectedXml));
            }

            private static IXmlSerializerInternal GetSerializer(Type type)
            {
                return CustomSerializer.GetSerializer(type, null, TestXmlSerializerOptions.Empty);
            }

            private static bool AlwaysEmitTypes
            {
                get { return true; }
            }

            private static IEnumerable<TestCaseData> TestCaseData
            {
                get
                {
                    return GetTestCaseData().Select(testCaseData =>
                    {
                        if (string.IsNullOrWhiteSpace(testCaseData.TestName))
                        {
                            var instanceType = testCaseData.Arguments[0].GetType();
                            var type = (Type)testCaseData.Arguments[1];

                            return testCaseData.SetName(type == instanceType ? type.Name : string.Format("{0} as {1}", instanceType.Name, type.Name));
                        }

                        return testCaseData;
                    });
                }
            }

            private static IEnumerable<TestCaseData> GetTestCaseData()
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
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.2950000Z"))
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

            private static object GetAnonymousObject()
            {
                return new { Bar = new { Baz = true }, Qux = "abc" };
            }

            private static string GetXsiTypeString(Type type)
            {
                if (type == typeof(ExpandoObject) || type.IsAnonymous())
                {
                    return "";
                }

                return string.Format(" xsi:type=\"{0}\"", type.GetXsdType());
            }
        }

        internal class DynamicDeserializationTestsWithWithXsdTypes// : XmlToObject
        {
            [TestCaseSource("TestCaseData")]
            public void DeserializesCorrectly(string xml, Type type, object expectedObject)
            {
                Console.WriteLine("Input XML:");
                Console.WriteLine(xml);

                var customSerializer = GetSerializer(type);

                var customObject = customSerializer.DeserializeObject(xml);

                Assert.That(customObject, Has.PropertiesEqualTo(expectedObject));
            }

            private static IXmlSerializerInternal GetSerializer(Type type)
            {
                return CustomSerializer.GetSerializer(type, null, TestXmlSerializerOptions.Empty);
            }

            private static IEnumerable<TestCaseData> TestCaseData
            {
                get
                {
                    return GetTestCaseData().Select(testCaseData =>
                    {
                        if (string.IsNullOrWhiteSpace(testCaseData.TestName))
                        {
                            var instanceType = testCaseData.Arguments[0].GetType();
                            var type = (Type)testCaseData.Arguments[1];

                            return testCaseData.SetName(type == instanceType ? type.Name : string.Format("{0} as {1}", instanceType.Name, type.Name));
                        }

                        return testCaseData;
                    });
                }
            }

            private static IEnumerable<TestCaseData> GetTestCaseData()
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
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.2950000Z"),
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

            private static ClassWithDynamicProperty GetExpected(ClassWithDynamicProperty expected)
            {
                return expected;
            }

            private static string GetXsiTypeString(Type type)
            {
                if (type == typeof(ExpandoObject))
                {
                    return "";
                }

                return string.Format(" xsi:type=\"{0}\"", type.GetXsdType());
            }

            private static bool ShouldSkipUseCase
            {
                get { return false; }
            }
        }

        internal class DynamicDeserializationTestsWithWithoutXsdTypes// : DynamicDeserializationTestsWithWithXsdTypes
        {
            [TestCaseSource("TestCaseData")]
            public void DeserializesCorrectly(string xml, Type type, object expectedObject)
            {
                Console.WriteLine("Input XML:");
                Console.WriteLine(xml);

                var customSerializer = GetSerializer(type);

                var customObject = customSerializer.DeserializeObject(xml);

                Assert.That(customObject, Has.PropertiesEqualTo(expectedObject));
            }

            private static IXmlSerializerInternal GetSerializer(Type type)
            {
                return CustomSerializer.GetSerializer(type, null, TestXmlSerializerOptions.Empty);
            }

            private static IEnumerable<TestCaseData> TestCaseData
            {
                get
                {
                    return GetTestCaseData().Select(testCaseData =>
                    {
                        if (string.IsNullOrWhiteSpace(testCaseData.TestName))
                        {
                            var instanceType = testCaseData.Arguments[0].GetType();
                            var type = (Type)testCaseData.Arguments[1];

                            return testCaseData.SetName(type == instanceType ? type.Name : string.Format("{0} as {1}", instanceType.Name, type.Name));
                        }

                        return testCaseData;
                    });
                }
            }

            private static IEnumerable<TestCaseData> GetTestCaseData()
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
                    string.Format(ExpectedDynamicXmlFormat, GetXsiTypeString(typeof(DateTime)), "2013-03-17T06:56:10.2950000Z"),
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

            private static string GetXsiTypeString(Type type)
            {
                return "";
            }

            private static bool ShouldSkipUseCase
            {
                get { return true; }
            }

            private static ClassWithDynamicProperty GetExpected(ClassWithDynamicProperty expected)
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

        [XmlInclude(typeof(Foo))]
        public class ClassWithObjectProperty
        {
            public int IntProperty { get; set; }
            public object ObjectProperty { get; set; }
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
