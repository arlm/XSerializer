using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class XmlRootAttributeTests
    {
        [Test]
        public void WhenXmlRootAttributeDecoratesAClassUseItAsTheRootElementName()
        {
            var foo = new FooWithXmlRootAttribute { Bar = "abc123" };
            var serializer = new XmlSerializer<FooWithXmlRootAttribute>();
            var xml = serializer.Serialize(foo);
            Assert.That(xml, Contains.Substring("<OMGWTFBBQ"));
            Assert.That(xml, Is.Not.ContainsSubstring("<FooWithXmlRootAttribute"));
        }

        [Test]
        public void WhenXmlRootAttributeDecoratesButRootElementIsSuppliedByConstructorUseTheRootElementNameFromTheConstructor()
        {
            var foo = new FooWithXmlRootAttribute { Bar = "abc123" };
            var serializer = new XmlSerializer<FooWithXmlRootAttribute>(options => options.SetRootElementName("AndNowForSomethingCompletelyDifferent"));
            var xml = serializer.Serialize(foo);
            Assert.That(xml, Contains.Substring("<AndNowForSomethingCompletelyDifferent"));
            Assert.That(xml, Is.Not.ContainsSubstring("<OMGWTFBBQ"));
            Assert.That(xml, Is.Not.ContainsSubstring("<FooWithXmlRootAttribute"));
        }

        [XmlRoot("OMGWTFBBQ")]
        public class FooWithXmlRootAttribute
        {
            public string Bar { get; set; }
        }
    }
}