using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class ListJsonSerializerTests
    {
        [Test]
        public void CanSerializeGenericIEnumerable()
        {
            var serializer = new JsonSerializer<List<string>>();

            var json = serializer.Serialize(new List<string> { "abc", "xyz" });

            Assert.That(json, Is.EqualTo(@"[""abc"",""xyz""]"));
        }

        [Test]
        public void CanSerializeGenericIEnumerableEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<List<string>>(configuration);

            var json = serializer.Serialize(new List<string> { "abc", "xyz" });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"[""abc"",""xyz""]")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<ArrayList>();

            var json = serializer.Serialize(new ArrayList { "abc", "xyz" });

            Assert.That(json, Is.EqualTo(@"[""abc"",""xyz""]"));
        }

        [Test]
        public void CanSerializeSerializeNonGenericIEnumerableEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<ArrayList>(configuration);

            var json = serializer.Serialize(new ArrayList { "abc", "xyz" });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"[""abc"",""xyz""]")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeGenericIEnumerableOfObjectWhenAnItemTypeIsDecoratedWithEncrypteAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<List<object>>(configuration);

            var json = serializer.Serialize(new List<object> { new Foo { Bar = "abc", Baz = true }, new Foo { Bar = "xyz", Baz = false } });

            var expected =
                "["
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""abc"",""Baz"":true}")
                    + @""""
                + ","
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""xyz"",""Baz"":false}")
                    + @""""
                + "]";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeGenericIEnumerableOfObjectWhenAnItemTypePropertyIsDecoratedWithEncrypteAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<List<object>>(configuration);

            var json = serializer.Serialize(new List<object> { new Qux { Bar = "abc", Baz = true }, new Qux { Bar = "xyz", Baz = false } });

            var expected =
                @"[{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""abc""")
                    + @""""
                + @",""Baz"":true},{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""xyz""")
                    + @""""
                + @",""Baz"":false}]";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeGenericList()
        {
            var serializer = new JsonSerializer<List<string>>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.EqualTo(new List<string> { "abc", "xyz" }));
        }

        [Test]
        public void CanDeserializeGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable<string>>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.EqualTo(new List<string> { "abc", "xyz" }));
        }

        [Test, Ignore] // TODO: unignore this test when DynamicJsonSerializer has been fully implemented.
        public void CanDeserializeNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.EqualTo(new ArrayList { "abc", "xyz" }));
        }

        [Test]
        public void CanDeserializeEmptyGenericList()
        {
            var serializer = new JsonSerializer<List<string>>();

            var result = serializer.Deserialize(@"[]");

            Assert.That(result, Is.EqualTo(new List<string>()));
        }

        [Test]
        public void CanDeserializeEmptyGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable<string>>();

            var result = serializer.Deserialize(@"[]");

            Assert.That(result, Is.EqualTo(new List<string>()));
        }

        [Test]
        public void CanDeserializeEmptyNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable>();

            var result = serializer.Deserialize(@"[]");

            Assert.That(result, Is.EqualTo(new ArrayList()));
        }

        [Test]
        public void CanDeserializeNullGenericList()
        {
            var serializer = new JsonSerializer<List<string>>();

            var result = serializer.Deserialize(@"null");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void CanDeserializeNullGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable<string>>();

            var result = serializer.Deserialize(@"null");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void CanDeserializeNullNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable>();

            var result = serializer.Deserialize(@"null");

            Assert.That(result, Is.Null);
        }

        [Encrypt]
        public class Foo
        {
            public string Bar { get; set; }
            public bool Baz { get; set; }
        }

        public class Qux
        {
            [Encrypt]
            public string Bar { get; set; }
            public bool Baz { get; set; }
        }
    }
}