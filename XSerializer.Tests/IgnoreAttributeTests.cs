using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class IgnoreAttributeTests
    {
        [TestCaseSource("TestCaseData")]
        public void RoundTripsCorrectly(object instance, Type type)
        {
            var customSerializer = CustomSerializer.GetSerializer(type, null, TestXmlSerializerOptions.Empty);
            var defaultSerializer = new System.Xml.Serialization.XmlSerializer(type);

            var customXml = customSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions(shouldAlwaysEmitTypes: AlwaysEmitTypes)).StripXsiXsdDeclarations();
            var defaultXml = defaultSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions(shouldAlwaysEmitTypes: AlwaysEmitTypes)).StripXsiXsdDeclarations();

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Custom XML:");
            Console.WriteLine(customXml);

            Assert.That(customXml, Is.EqualTo(defaultXml));
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
            yield return new TestCaseData(new Foo { Bar = 1, Baz = 2, Qux = 3 }, typeof(Foo)).SetName("XmlIgnoreAttribute causes property to be skipped during serialization");
        }

        [Test]
        public void VerifyThatProxyPropertiesRoundTripCorrectly()
        {
            var foo = new Foo { Bar = 1, Baz = 2, Qux = 3 };

            var serializer = new XmlSerializer<Foo>(x => x.Indent());

            var xml = serializer.Serialize(foo);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip, Has.PropertiesEqualTo(foo));
        }

        public class Foo
        {
            [XmlIgnore]
            public int Bar { get; set; }
            public int Baz { get; set; }

            [XmlIgnore]
            public int Qux { get; set; }

            [XmlElement("Qux")]
            public string QuxString
            {
                get { return Qux.ToString(CultureInfo.InvariantCulture); }
                set { Qux = int.Parse(value); }
            }
        }
    }
}