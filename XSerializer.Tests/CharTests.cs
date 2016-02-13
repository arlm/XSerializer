using NUnit.Framework;

namespace XSerializer.Tests
{
    public class CharTests
    {
        [Test]
        public void CanSerializeCharAsChar()
        {
            var serializer = new XmlSerializer<ClassWithChar>();

            var obj = new ClassWithChar { Foo = 'a' };

            var xml = serializer.Serialize(obj);

            Assert.That(xml, Contains.Substring("<Foo>a</Foo>"));
        }

        [Test]
        public void CanSerializeCharAsInt()
        {
            var serializer = new XmlSerializer<ClassWithChar>(x => x.SerializeCharAsInt());

            var obj = new ClassWithChar { Foo = 'a' };

            var xml = serializer.Serialize(obj);

            Assert.That(xml, Contains.Substring("<Foo>97</Foo>"));
        }

        [Test]
        public void CanSerializeNullableCharAsChar()
        {
            var serializer = new XmlSerializer<ClassWithNullableChar>();

            var obj = new ClassWithNullableChar { Foo = 'a' };

            var xml = serializer.Serialize(obj);

            Assert.That(xml, Contains.Substring("<Foo>a</Foo>"));
        }

        [Test]
        public void CanSerializeNullableCharAsInt()
        {
            var serializer = new XmlSerializer<ClassWithNullableChar>(x => x.SerializeCharAsInt());

            var obj = new ClassWithNullableChar { Foo = 'a' };

            var xml = serializer.Serialize(obj);

            Assert.That(xml, Contains.Substring("<Foo>97</Foo>"));
        }

        [Test]
        public void CanDeserializeCharAsChar()
        {
            var serializer = new XmlSerializer<ClassWithChar>();

            var xml = "<ClassWithChar><Foo>a</Foo></ClassWithChar>";

            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.EqualTo('a'));
        }

        [Test]
        public void CanDeserializeCharAsInt()
        {
            var serializer = new XmlSerializer<ClassWithChar>(x => x.SerializeCharAsInt());

            var xml = "<ClassWithChar><Foo>97</Foo></ClassWithChar>";

            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.EqualTo('a'));
        }

        [Test]
        public void CanDeserializeNullableCharFromChar()
        {
            var serializer = new XmlSerializer<ClassWithNullableChar>();

            var xml = "<ClassWithNullableChar><Foo>a</Foo></ClassWithNullableChar>";

            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.EqualTo('a'));
        }

        [Test]
        public void CanDeserializeNullableCharFromInt()
        {
            var serializer = new XmlSerializer<ClassWithNullableChar>(x => x.SerializeCharAsInt());

            var xml = "<ClassWithNullableChar><Foo>97</Foo></ClassWithNullableChar>";

            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.EqualTo('a'));
        }

        [Test]
        public void CanDeserializeNullableCharFromNull()
        {
            var serializer = new XmlSerializer<ClassWithNullableChar>();

            var xml = "<ClassWithNullableChar></ClassWithNullableChar>";

            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.Null);
        }

        [Test]
        public void CanDeserializeNullableCharFromNullWithSerializeCharAsIntOption()
        {
            var serializer = new XmlSerializer<ClassWithNullableChar>(x => x.SerializeCharAsInt());

            var xml = "<ClassWithNullableChar></ClassWithNullableChar>";

            var obj = serializer.Deserialize(xml);

            Assert.That(obj.Foo, Is.Null);
        }

        public class ClassWithChar
        {
            public char Foo { get; set; }
        }

        public class ClassWithNullableChar
        {
            public char? Foo { get; set; }
        }
    }
}