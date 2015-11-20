using NUnit.Framework;

namespace XSerializer.Tests
{
    public class JsonStringTests
    {
        [TestCase("123", 123)]
        [TestCase("", null)]
        public void CanDeserializeJsonStringAsInt(string stringValue, int? expectedValue)
        {
            var serializer = new JsonSerializer<FooInt>();

            var json = @"{""Bar"":""" + stringValue + @"""}";

            var result = serializer.Deserialize(json);

            Assert.That(result.Bar, Is.EqualTo(expectedValue));
        }

        [TestCase("123.45", 123.45)]
        [TestCase("", null)]
        public void CanDeserializeJsonStringAsDouble(string stringValue, double? expectedValue)
        {
            var serializer = new JsonSerializer<FooDouble>();

            var json = @"{""Bar"":""" + stringValue + @"""}";

            var result = serializer.Deserialize(json);

            Assert.That(result.Bar, Is.EqualTo(expectedValue));
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
            public int? Bar { get; set; }
        }

        public class FooDouble
        {
            public double? Bar { get; set; }
        }

        public class FooBool
        {
            public bool? Bar { get; set; }
        }
    }
}