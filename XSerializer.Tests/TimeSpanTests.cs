using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class TimeSpanTests
    {
        [Test]
        public void TimeSpanValuesRoundTripCorrectly()
        {
            var foo = new Foo
            {
                Bar = TimeSpan.FromSeconds(1),
                Baz = TimeSpan.FromSeconds(2)
            };

            var serializer = new XmlSerializer<Foo>(x => x.Indent());

            var xml = serializer.Serialize(foo);
            Console.WriteLine(xml);

            var roundTripFoo = serializer.Deserialize(xml);

            Assert.That(roundTripFoo.Bar, Is.EqualTo(foo.Bar));
            Assert.That(roundTripFoo.Baz, Is.EqualTo(foo.Baz));
            Assert.That(roundTripFoo.Qux, Is.EqualTo(foo.Qux));
        }

        [Test]
        public void TimeSpanValuesRoundTripCorrectlyForJson()
        {
            var foo = new Foo
            {
                Bar = TimeSpan.FromSeconds(1),
                Baz = TimeSpan.FromSeconds(2)
            };

            var serializer = new JsonSerializer<Foo>();

            var json = serializer.Serialize(foo);
            Console.WriteLine(json);

            var roundTripFoo = serializer.Deserialize(json);

            Assert.That(roundTripFoo.Bar, Is.EqualTo(foo.Bar));
            Assert.That(roundTripFoo.Baz, Is.EqualTo(foo.Baz));
            Assert.That(roundTripFoo.Qux, Is.EqualTo(foo.Qux));
        }

        public class Foo
        {
            public TimeSpan Bar { get; set; }
            public TimeSpan? Baz { get; set; }
            public TimeSpan? Qux { get; set; }
        }
    }
}