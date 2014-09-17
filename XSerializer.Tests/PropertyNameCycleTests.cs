using NUnit.Framework;

namespace XSerializer.Tests
{
    public class PropertyNameCycleTests
    {
        [Test]
        public void AnObjectGraphWithAPropertyNameCycleRoundTripsSuccessfully()
        {
            var foo = new Foo
            {
                Value = new FooValue
                {
                    Value = "abc"
                }
            };

            var serializer = new XmlSerializer<Foo>();

            var xml = serializer.Serialize(foo);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Value, Is.Not.Null);
            Assert.That(roundTrip.Value.Value, Is.EqualTo(foo.Value.Value));
        }

        public class Foo
        {
            public FooValue Value { get; set; }
        }

        public class FooValue
        {
            public string Value { get; set; }
        }
    }
}