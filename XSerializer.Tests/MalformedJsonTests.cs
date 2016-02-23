using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class MalformedJsonTests
    {
        [Test]
        public void Foo()
        {
            var serializer = new JsonSerializer<string>();

            var json = "abcd";

            serializer.Deserialize(json);
        }

        [Test]
        public void Bar()
        {
            var serializer = new JsonSerializer<FooString>();

            var json = @"{""Bar"":abcd}";

            try
            {
                serializer.Deserialize(json);
            }
            catch (MalformedDocumentException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [Test]
        public void Bar2()
        {
            var serializer = new JsonSerializer<FooDateTime>();

            var json = @"{""Bar"":""abc""}";

            try
            {
                serializer.Deserialize(json);
            }
            catch (MalformedDocumentException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public class FooString
        {
            public string Bar { get; set; }
        }

        public class FooDateTime
        {
            public DateTime Bar { get; set; }
        }
    }
}