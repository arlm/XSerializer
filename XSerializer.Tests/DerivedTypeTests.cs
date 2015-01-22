using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DerivedTypeTests
    {
        [Test]
        public void CanSerializeAnObjectOfASubclassType()
        {
            var serializer = new XmlSerializer<Thingy>(x => x.Indent());
            var thingy = new ThingyDerived { Value = "abc", AnotherValue = "xyz" };
            Assert.That(() => serializer.Serialize(thingy), Throws.Nothing);
        }

        [Test]
        public void CanRoundTripAnObjectOfASubclassType()
        {
            var serializer = new XmlSerializer<Thingy>(x => x.Indent());
            var thingy = new ThingyDerived { Value = "abc", AnotherValue = "xyz" };
            var xml = serializer.Serialize(thingy);
            var roundTrip = serializer.Deserialize(xml);
            Assert.That(roundTrip.Value, Is.EqualTo(thingy.Value));
        }

        public class Thingy
        {
            public string Value { get; set; }
        }

        public class ThingyDerived : Thingy
        {
            public string AnotherValue { get; set; }
        } 
    }
}