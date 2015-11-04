using NUnit.Framework;

namespace XSerializer.Tests
{
    public class JsonSerializerFactoryTests
    {
        [Test]
        public void TheNonGenericFactoryMethodReturnsTheCorrectTypeOfSerializer()
        {
            var genericSerializer = new JsonSerializer<JsonSerializerFactoryTests>();
            var interfaceSerializer = JsonSerializer.Create(typeof(JsonSerializerFactoryTests));

            Assert.That(interfaceSerializer.GetType(), Is.EqualTo(genericSerializer.GetType()));
        }
    }
}