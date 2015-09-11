using System.IO;
using System.Text;
using NUnit.Framework;

namespace XSerializer.Tests
{
    internal class JsonWriterTests
    {
        [Test]
        public void CanWriteString()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteValue("Hello, world!");
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("\"Hello, world!\""));
        }

        [Test]
        public void CanWriteStringWithEscapedCharacters()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteValue("\"Ow.\"\r\n-My Pancreas");
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("\"\\\"Ow.\\\"\\r\\n-My Pancreas\""));
        }

        [Test]
        public void CanWriteDouble()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteValue(123.45);
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("123.45"));
        }

        [Test]
        public void CanWriteTrue()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteValue(true);
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("true"));
        }

        [Test]
        public void CanWriteFalse()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteValue(false);
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("false"));
        }

        [Test]
        public void CanWriteNull()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteNull();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("null"));
        }

        [Test]
        public void CanWriteOpenObject()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteOpenObject();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("{"));
        }

        [Test]
        public void CanWriteCloseObject()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteCloseObject();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("}"));
        }

        [Test]
        public void CanWriteOpenArray()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteOpenArray();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("["));
        }

        [Test]
        public void CanWriteCloseArray()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteCloseArray();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("]"));
        }

        [Test]
        public void CanWriteNameValueSeparator()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteNameValueSeparator();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo(":"));
        }

        [Test]
        public void CanWriteItemSeparator()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());
                writer.WriteItemSeparator();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo(","));
        }

        [Test]
        public void CanWriteComplexObject()
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                var writer = new JsonWriter(stringWriter, new JsonSerializeOperationInfo());

                writer.WriteOpenObject();

                writer.WriteValue("foo");

                writer.WriteNameValueSeparator();

                writer.WriteValue("bar");

                writer.WriteItemSeparator();

                writer.WriteValue("baz");

                writer.WriteNameValueSeparator();

                writer.WriteOpenArray();

                writer.WriteValue(1);
                writer.WriteItemSeparator();
                writer.WriteValue(2);
                writer.WriteItemSeparator();
                writer.WriteValue(3);

                writer.WriteCloseArray();

                writer.WriteCloseObject();

                writer.Flush();
            }

            var s = sb.ToString();

            Assert.That(s, Is.EqualTo("{\"foo\":\"bar\",\"baz\":[1,2,3]}"));
        }
    }
}