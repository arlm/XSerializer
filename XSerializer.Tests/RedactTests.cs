using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class RedactTests
    {
        private XmlSerializer<RedactTestClassContainer> _serializer;
        private RedactTestClassContainer _container;
        private RedactTestClass _testClass;

        [SetUp]
        public void Setup()
        {
            _serializer = new XmlSerializer<RedactTestClassContainer>();
            _container = new RedactTestClassContainer();
            _testClass = new RedactTestClass();
            _container.Data = _testClass;
        }

        [Test]
        public void CustomSerializerIsUsedIfRedactAttributeExists()
        {
            Assert.That(_serializer.Serializer, Is.InstanceOf<CustomSerializer<RedactTestClassContainer>>());
        }

        [TestCase("abc123", "XXX111")]
        [TestCase("123-45-6789", "111-11-1111")]
        [TestCase("123 Main Street", "111 XXXX XXXXXX")]
        [TestCase("", "")]
        public void StringAtrributesRedactCorrectly(string value, string expectedAttributeValue)
        {
            _testClass.StringAttributeProperty = value;

            var xml = Serialize();

            Assert.That(xml, Contains.Substring(string.Format("StringAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        // TODO: test bool, bool? enum, enum?, datetime, datetime?, and "other"

        private string Serialize()
        {
            return _serializer.Serialize(_container);
        }

        public class RedactTestClassContainer
        {
            public RedactTestClass Data { get; set; }
        }

        public class RedactTestClass
        {
            [Redact]
            [XmlAttribute]
            public string StringAttributeProperty { get; set; }
        }
    }
}