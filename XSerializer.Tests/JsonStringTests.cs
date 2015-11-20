using NUnit.Framework;

namespace XSerializer.Tests
{
    public class JsonStringTests
    {
        [Test]
        public void CanDeserializeJsonStringAsInt()
        {
            var serializer = new JsonSerializer<FooInt>();

            var json = @"{""Bar"":""123""}";

            var result = serializer.Deserialize(json);

            Assert.That(result.Bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeJsonStringAsDouble()
        {
            var serializer = new JsonSerializer<FooDouble>();

            var json = @"{""Bar"":""123.45""}";

            var result = serializer.Deserialize(json);

            Assert.That(result.Bar, Is.EqualTo(123.45));
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("", null)]
        public void CanDeserializeJsonStringAsBool(string stringValue, object expectedValue)
        {
            var serializer = new JsonSerializer<FooBool>();

            var json = @"{""Bar"":""" + stringValue + @"""}";

            var result = serializer.Deserialize(json);

            Assert.That(result.Bar, Is.EqualTo(expectedValue));
        }

        public class FooInt
        {
            public int Bar { get; set; }
        }

        public class FooDouble
        {
            public double Bar { get; set; }
        }

        public class FooBool
        {
            public bool? Bar { get; set; }
        }
    }
}