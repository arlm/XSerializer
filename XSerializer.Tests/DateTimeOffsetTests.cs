using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DateTimeOffsetTests
    {
        [Test]
        public void DateTimeOffsetValuesRoundTripCorrectly()
        {
            var foo = new Foo
            {
                Bar = DateTimeOffset.Now,
                Baz = DateTimeOffset.Now + TimeSpan.FromDays(1)
            };

            var serializer = new XmlSerializer<Foo>(x => x.Indent());

            var xml = serializer.Serialize(foo);
            Console.WriteLine(xml);

            var roundTripFoo = serializer.Deserialize(xml);

            Assert.That(roundTripFoo.Bar, Is.EqualTo(foo.Bar));
            Assert.That(roundTripFoo.Baz, Is.EqualTo(foo.Baz));
            Assert.That(roundTripFoo.Qux, Is.EqualTo(foo.Qux));
        }

        public class Foo
        {
            public DateTimeOffset Bar { get; set; }
            public DateTimeOffset? Baz { get; set; }
            public DateTimeOffset? Qux { get; set; }
        }
    }
}