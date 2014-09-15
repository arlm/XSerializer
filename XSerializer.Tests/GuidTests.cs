using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class GuidTests
    {
        [Test]
        public void GuidValuesRoundTripCorrectly()
        {
            var foo = new Foo
            {
                Bar = Guid.NewGuid(),
                Baz = Guid.NewGuid()
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
            public Guid Bar { get; set; }
            public Guid? Baz { get; set; }
            public Guid? Qux { get; set; }
        }
    }
}
