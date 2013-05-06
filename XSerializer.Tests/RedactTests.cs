using NUnit.Framework;

namespace XSerializer.Tests
{
    public class RedactTests
    {
        [Test]
        public void CustomSerializerIsUsedIfRedactAttributeExists()
        {
            var serializer = new XmlSerializer<ShouldRedactThis>();

            Assert.That(serializer.Serializer, Is.InstanceOf<CustomSerializer<ShouldRedactThis>>());
        }
        
        [Test]
        public void DefaultSerializerIsUsedIfRedactAttributeDoesNotExist()
        {
            var serializer = new XmlSerializer<ShouldNotRedactThis>();

            Assert.That(serializer.Serializer, Is.InstanceOf<DefaultSerializer<ShouldNotRedactThis>>());
        }
        
        public class ShouldRedactThis
        {
            [Redact]
            public string SomeString { get; set; }
        }

        public class ShouldNotRedactThis
        {
            public string SomeString { get; set; }
        }
    }
}