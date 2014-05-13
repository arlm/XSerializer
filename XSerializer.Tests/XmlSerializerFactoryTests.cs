using NUnit.Framework;

namespace XSerializer.Tests
{
    public class XmlSerializerFactoryTests
    {
        [Test]
        public void TheNonGenericFactoryMethodReturnsTheCorrectTypeOfXmlSerializer()
        {
            var genericSerializer = new XmlSerializer<XmlSerializerFactoryTests>();
            var interfaceSerializer = XmlSerializer.Create(typeof(XmlSerializerFactoryTests));

            Assert.That(interfaceSerializer.GetType(), Is.EqualTo(genericSerializer.GetType()));
        }
    }
}