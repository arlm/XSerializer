using NUnit.Framework;

namespace XSerializer.Tests
{
    public class JsonNumberTests
    {
        [Test]
        public void CanDeserializeJsonNumberAsString()
        {
            var serializer = new JsonSerializer<FooString>();

            var json = @"{""Bar"":123.45}"; // Number not wrapped in quotes

            var result = serializer.Deserialize(json);

            Assert.That(result.Bar, Is.EqualTo("123.45"));
        }

        public class FooString
        {
            public string Bar { get; set; }
        }
    }
}