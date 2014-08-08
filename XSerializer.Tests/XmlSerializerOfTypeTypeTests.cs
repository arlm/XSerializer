using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class XmlSerializerOfTypeTypeTests
    {
        [TestCase(typeof(int))]
        [TestCase(typeof(BlackHole))]
        [TestCase(null)]
        public void VerifyThatAPropertyOfTypeTypeCanSuccessfullyRoundTrip(Type type)
        {
            var serializer = new XmlSerializer<BlackHole>(x => x.Indent());

            var blackHole = new BlackHole { Type = type };

            var xml = serializer.Serialize(blackHole);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Type, Is.EqualTo(blackHole.Type));
        }

        [TestCase(typeof(int))]
        [TestCase(typeof(BlackHole))]
        [TestCase(null)]
        public void VerifyThatATypeCanRoundTrip(Type type)
        {
            var serializer = new XmlSerializer<Type>(x => x.Indent());

            var xml = serializer.Serialize(type);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip, Is.EqualTo(type));
        }

        public class BlackHole
        {
            public Type Type { get; set; }
        }
    }
}
