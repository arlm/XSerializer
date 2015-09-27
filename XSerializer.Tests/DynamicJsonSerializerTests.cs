using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DynamicJsonSerializerTests
    {
        [TestCase("abc", @"""abc""")]
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

            Assert.That(json, Is.EqualTo(@"{""foo"":""bar"",""baz"":null}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToOther()
        {
            var dictionary = new Dictionary<string, string> { { "foo", "bar" }, { "baz", null } };

            var serializer = new JsonSerializer<object>();

            var json = serializer.Serialize(dictionary);

            Assert.That(json, Is.EqualTo(@"{""foo"":""bar"",""baz"":null}"));
        }

        [Test]
        public void CanSerializeIEnumerable()
        {
            var items = new List<object> { 123.0, true, false, null };

            var serializer = new JsonSerializer<object>();

            var json = serializer.Serialize(items);

            Assert.That(json, Is.EqualTo(@"[123,true,false,null]"));
        }

        [Test]
        public void CanSerializeCustomObject()
        {
            var instance = new CustomJsonSerializerTests.Bar
            {
                Baz = new CustomJsonSerializerTests.Baz
                {
                    Qux = "abc",
                    Garply = true
                },
                Corge = 123.45
            };

            var serializer = new JsonSerializer<object>();

            var json = serializer.Serialize(instance);

            Assert.That(json, Is.EqualTo(@"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}"));
        }

        [Test]
        public void CanDeserializeJsonObjectAndReadPropertiesWithoutConversion()
        {
            var json = @"{""foo"":""abc"",""bar"":123.45}";

            var serializer = new JsonSerializer<dynamic>();

            var result = serializer.Deserialize(json);

            string foo = result.foo;
            double bar = result.bar;

            Assert.That(foo, Is.EqualTo("abc"));
            Assert.That(bar, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeJsonObjectAndReadPropertiesWithConversion()
        {
            var guid = Guid.NewGuid();

            var json = string.Format(@"{{""foo"":""{0:D}"",""bar"":123.45}}", guid);

            var serializer = new JsonSerializer<dynamic>();

            var result = serializer.Deserialize(json);
            
            // In order to project a property to another type, append "As" plus the
            // type to convert to. Guid, DateTime, and DateTimeOffset are parsed
            // from string properties and the various numeric types are parsed from
            // numeric properties.
            Guid foo = result.fooAsGuid;
            decimal bar = result.barAsDecimal;

            Assert.That(foo, Is.EqualTo(guid));
            Assert.That(bar, Is.EqualTo(123.45M));
        }

        [Test]
        public void CanDeserializeJsonArrayWithNoConversionAndReadValues()
        {
            var json = @"[""abc"",""xyz""]";

            var serializer = new JsonSerializer<dynamic>();

            var result = serializer.Deserialize(json);

            string foo = result[0];
            string bar = result[1];

            Assert.That(foo, Is.EqualTo("abc"));
            Assert.That(bar, Is.EqualTo("xyz"));
        }

        [Test]
        public void CanDeserializeJsonArrayWithNonProjectingConversionAndReadValues()
        {
            var json = @"[""abc"",""xyz""]";

            var serializer = new JsonSerializer<dynamic>();

            // Non-projecting conversions include IEnumerable<T>, ICollection<T>, IList<T>, and
            // T[] where T is one of: object, string, boolean, double, JsonObject, or JsonArray.
            // If any items in the JsonArray cannot be assigned to T, an exception is thrown.
            IList<string> result = serializer.Deserialize(json);

            string foo = result[0];
            string bar = result[1];

            Assert.That(foo, Is.EqualTo("abc"));
            Assert.That(bar, Is.EqualTo("xyz"));
        }

        [Test]
        public void CanDeserializeJsonArrayWithProjectingConversionAndReadValues()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            var json = string.Format(@"[""{0:D}"",""{1:D}""]", guid1, guid2);

            var serializer = new JsonSerializer<dynamic>();

            // Projecting conversions include IEnumerable<T>, ICollection<T>, IList<T>, and
            // T[] where T is one of: byte, sbyte, short, ushort, int, uint, long, ulong, float
            // decimal, DateTime, DateTimeOffset, or Guid. If any items in the JsonArray
            // cannot be converted to T, an exception is thrown.
            IList<Guid> result = serializer.Deserialize(json);

            Guid foo = result[0];
            Guid bar = result[1];

            Assert.That(foo, Is.EqualTo(guid1));
            Assert.That(bar, Is.EqualTo(guid2));
        }
    }
}