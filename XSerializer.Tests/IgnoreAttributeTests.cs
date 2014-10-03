using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class IgnoreAttributeTests : ObjectToXmlWithDefaultComparison
    {
        protected override IEnumerable<TestCaseData> GetTestCaseData()
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