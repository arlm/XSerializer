using NUnit.Framework;

namespace XSerializer.Tests
{
    public class BooleanTests
    {
        [TestCase("true", true)]
        [TestCase("1", true)]
        [TestCase("false", false)]
        [TestCase("0", false)]
        public void CanDeserializeBoolean(string xmlElementValue, bool expectedValue)
        {
            var serializer = new XmlSerializer<ClassWithBoolean>();

            var xml = string.Format("<ClassWithBoolean><Foo>{0}</Foo></ClassWithBoolean>", xmlElementValue);
            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.EqualTo(expectedValue));
        }

        [TestCase("<Foo>true</Foo>", true)]
        [TestCase("<Foo>1</Foo>", true)]
        [TestCase("<Foo>false</Foo>", false)]
        [TestCase("<Foo>0</Foo>", false)]
        [TestCase("", null)]
        public void CanDeserializeNullableBoolean(string xmlElement, bool? expectedValue)
        {
            var serializer = new XmlSerializer<ClassWithNullableBoolean>();

            var xml = string.Format("<ClassWithNullableBoolean>{0}</ClassWithNullableBoolean>", xmlElement);
            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.EqualTo(expectedValue));
        }

        public class ClassWithBoolean
        {
            public bool Foo { get; set; }
        }

        public class ClassWithNullableBoolean
        {
            public bool? Foo { get; set; }
        }
    }
}