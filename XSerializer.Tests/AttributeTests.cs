using NUnit.Framework;
using System.Xml.Serialization;

namespace XSerializer.Tests
{
    public class AttributeTests
    {
        [Test]
        public void XmlAttributeDeserializesIntoProperty()
        {
            var xml = @"<AttributeContainer SomeValue=""abc""></AttributeContainer>";

            var serializer = new CustomSerializer<AttributeContainer>(TestXmlSerializerOptions.Empty);

            var container = serializer.Deserialize(xml);

            Assert.That(container.SomeValue, Is.EqualTo("abc"));
        }

        public class AttributeContainer
        {
            [XmlAttribute]
            public string SomeValue { get; set; }
        }
    }
}
