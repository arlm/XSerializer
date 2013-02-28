using System.Collections.Generic;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class IgnoreAttributeTests : ObjectToXmlWithDefaultComparison
    {
        protected override IEnumerable<TestCaseData> GetTestCaseData()
        {
            yield return new TestCaseData(new Foo { Bar = 1, Baz = 2 }, typeof(Foo)).SetName("XmlIgnoreAttribute causes property to be skipped during serialization");
        }

        public class Foo
        {
            [XmlIgnore]
            public int Bar { get; set; }
            public int Baz { get; set; }
        }
    }
}