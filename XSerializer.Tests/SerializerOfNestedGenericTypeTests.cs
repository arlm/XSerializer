using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class SerializerOfNestedGenericTypeTests
    {
        [Test]
        public void CanRoundTripWithASerializerOfANestedGenericType()
        {
            var serializer = new XmlSerializer<Dictionary<string, List<string>>>();

            var original = new Dictionary<string, List<string>>
            {
                {"1", new List<string> {"A", "B", "C"}},
                {"2", new List<string> {"X", "Y", "Z"}}
            };

            var xml = serializer.Serialize(original);

            var roundTrip = serializer.Deserialize(xml);
            
            Assert.That(roundTrip.Count, Is.EqualTo(original.Count));

            Assert.That(roundTrip["1"], Is.EqualTo(original["1"]));
            Assert.That(roundTrip["2"], Is.EqualTo(original["2"]));
        }
    }
}
