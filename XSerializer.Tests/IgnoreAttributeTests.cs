using System.Xml.Serialization;

namespace XSerializer.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

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