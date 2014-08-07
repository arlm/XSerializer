using NUnit.Framework;

namespace XSerializer.Tests
{
    public class XmlSerializerFactoryTests
    {
        [Test]
        public void TheFirstNonGenericFactoryMethodReturnsTheCorrectTypeOfXmlSerializer()
        {
            var genericSerializer = new XmlSerializer<XmlSerializerFactoryTests>();
            var interfaceSerializer = XmlSerializer.Create(typeof(XmlSerializerFactoryTests));

            Assert.That(interfaceSerializer.GetType(), Is.EqualTo(genericSerializer.GetType()));
        }

        [Test]
        public void TheSecondNonGenericFactoryMethodReturnsTheCorrectTypeOfXmlSerializer()
        {
            var genericSerializer = new XmlSerializer<XmlSerializerFactoryTests>();
            var interfaceSerializer = XmlSerializer.Create(typeof(XmlSerializerFactoryTests), x => x.Indent());

            Assert.That(interfaceSerializer.GetType(), Is.EqualTo(genericSerializer.GetType()));
        }

        [Test]
        public void TheThirdNonGenericFactoryMethodReturnsTheCorrectTypeOfXmlSerializer()
        {
            var genericSerializer = new XmlSerializer<XmlSerializerFactoryTests>();
            var interfaceSerializer = XmlSerializer.Create(typeof(XmlSerializerFactoryTests), new XmlSerializationOptions());

            Assert.That(interfaceSerializer.GetType(), Is.EqualTo(genericSerializer.GetType()));
        }
    }
}