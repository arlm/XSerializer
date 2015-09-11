using NUnit.Framework;

namespace XSerializer.Tests
{
    public class CustomJsonSerializerTests
    {
        [Test]
        public void CanSerializeReadWriteProperties()
        {
            var serializer = new JsonSerializer<Bar>();

            var instance = new Bar
                {
                    Baz = new Baz
                    {
                        Qux = "abc",
                        Garply = true
                    },
                    Corge = 123.45
                };

            var json = serializer.Serialize(instance);

            Assert.That(json, Is.EqualTo(@"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}"));
        }

        public class Bar
        {
            public Baz Baz { get; set; }
            public double Corge { get; set; }
        }

        public class Baz
        {
            public string Qux { get; set; }
            public bool Garply { get; set; }
        }
    }
}