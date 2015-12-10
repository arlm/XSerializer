using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class JsonMiscellaneousTypesTests
    {
        [Test]
        public void CanSerializeEnum()
        {
            var serializer = new JsonSerializer<Foo1>();

            var foo = new Foo1 { Bar = Garply.Fred };

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.EqualTo(@"{""Bar"":""Fred""}"));
        }

        [Test]
        public void CanDeserializeEnum()
        {
            var serializer = new JsonSerializer<Foo1>();

            var json = @"{""Bar"":""Fred""}";

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(Garply.Fred));
        }

        [TestCase(Garply.Fred, @"""Fred""")]
        [TestCase(null, "null")]
        public void CanSerializeNullableEnum(Garply? barValue, string expectedBarValue)
        {
            var serializer = new JsonSerializer<Foo4>();

            var foo = new Foo4 { Bar = barValue };

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.EqualTo(@"{""Bar"":" + expectedBarValue + "}"));
        }

        [TestCase(@"""Fred""", Garply.Fred)]
        [TestCase("null", null)]
        public void CanDeserializeNullableEnum(string barValue, Garply? expectedBarValue)
        {
            var serializer = new JsonSerializer<Foo4>();

            var json = @"{""Bar"":" + barValue + "}";

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(expectedBarValue));
        }

        [Test]
        public void CanSerializeType()
        {
            var serializer = new JsonSerializer<Foo3>();

            var foo = new Foo3 { Bar = typeof(Foo3) };

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.EqualTo(@"{""Bar"":""XSerializer.Tests.JsonMiscellaneousTypesTests+Foo3, XSerializer.Tests""}"));
        }

        [Test]
        public void CanDeserializeType()
        {
            var serializer = new JsonSerializer<Foo3>();

            var json = @"{""Bar"":""XSerializer.Tests.JsonMiscellaneousTypesTests+Foo3, XSerializer.Tests""}";

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(typeof(Foo3)));
        }

        [Test]
        public void CanSerializeUri()
        {
            var serializer = new JsonSerializer<Foo2>();

            var foo = new Foo2 { Bar = new Uri("http://google.com/") };

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.EqualTo(@"{""Bar"":""http:\/\/google.com\/""}"));
        }

        [Test]
        public void CanDeserializeUri()
        {
            var serializer = new JsonSerializer<Foo2>();

            var json = @"{""Bar"":""http:\/\/google.com\/""}";

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(new Uri("http://google.com/")));
        }

        public class Foo1
        {
            public Garply Bar { get; set; }
        }

        public class Foo4
        {
            public Garply? Bar { get; set; }
        }

        public class Foo2
        {
            public Uri Bar { get; set; }
        }

        public class Foo3
        {
            public Type Bar { get; set; }
        }

        public enum Garply
        {
            Waldo,
            Fred
        }
    }
}