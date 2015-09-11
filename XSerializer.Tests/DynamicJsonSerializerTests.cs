using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DynamicJsonSerializerTests
    {
        [TestCase("abc", "\"abc\"")]
        [TestCase(123.45, "123.45")]
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        [TestCase(null, "null")]
        public void CanSerializePrimitives(object primitive, string expectedJson)
        {
            var serializer = new JsonSerializer<object>();

            var json = serializer.Serialize(primitive);
            
            Assert.That(json, Is.EqualTo(expectedJson));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToObject()
        {
            var dictionary = new Dictionary<string, object> { { "foo", "bar" }, { "baz", null } };

            var serializer = new JsonSerializer<object>();

            var json = serializer.Serialize(dictionary);

            Assert.That(json, Is.EqualTo("{\"foo\":\"bar\",\"baz\":null}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToOther()
        {
            var dictionary = new Dictionary<string, string> { { "foo", "bar" }, { "baz", null } };

            var serializer = new JsonSerializer<object>();

            var json = serializer.Serialize(dictionary);

            Assert.That(json, Is.EqualTo("{\"foo\":\"bar\",\"baz\":null}"));
        }

        [Test]
        public void CanSerializeIEnumerable()
        {
            var dictionary = new object[] { 123.0, true, false, null, new Dictionary<string, string> { { "foo", "bar" } } };

            var serializer = new JsonSerializer<object>();

            var json = serializer.Serialize(dictionary);

            Assert.That(json, Is.EqualTo("[123,true,false,null,{\"foo\":\"bar\"}]"));
        }
    }
}