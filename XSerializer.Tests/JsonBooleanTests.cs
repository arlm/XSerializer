using NUnit.Framework;

namespace XSerializer.Tests
{
    public class JsonBooleanTests
    {
        [Test]
        public void CanDeserializeJsonBooleanAsString()
        {
            var serializer = new JsonSerializer<FooString>();

            var json = @"{""Bar"":true}"; // Boolean not wrapped in quotes

            var result = serializer.Deserialize(json);

            Assert.That(result.Bar, Is.EqualTo("true"));
        }

        public class FooString
        {
            public string Bar { get; set; }
        }
    }
}