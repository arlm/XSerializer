using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class DictionaryJsonSerializerTests
    {
        [Test]
        public void CanSerializeIDictionaryOfStringToObject()
        {
            var serializer = new JsonSerializer<IDictionary<string, object>>();

            var json = serializer.Serialize(new Dictionary<string, object>{ { "foo", "abc" }, { "bar", 123.45 } });

            Assert.That(json, Is.EqualTo(@"{""foo"":""abc"",""bar"":123.45}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToObjectEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<IDictionary<string, object>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, object> { { "foo", "abc" }, { "bar", 123.45 } });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""foo"":""abc"",""bar"":123.45}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void ForIDictionaryOfStringToObjectAValueWhoseTypeIsDecoratedWithTheEncryptAttributeIsEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, object>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, object> { { "foo", new Foo { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""abc"",""Baz"":true}")
                    + @""""
                + "}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void ForIDictionaryOfStringToObjectAValueHasAPropertyDecoratedWithTheEncryptAttributeHasThatPropertyEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, object>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, object> { { "foo", new Qux { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""abc""")
                    + @""""
                + @",""Baz"":true}}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToString()
        {
            var serializer = new JsonSerializer<IDictionary<string, string>>();

            var json = serializer.Serialize(new Dictionary<string, string> { { "foo", "abc" }, { "bar", null } });

            Assert.That(json, Is.EqualTo(@"{""foo"":""abc"",""bar"":null}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToStringEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<IDictionary<string, string>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, string> { { "foo", "abc" }, { "bar", null } });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""foo"":""abc"",""bar"":null}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomType()
        {
            var serializer = new JsonSerializer<IDictionary<string, Corge>>();

            var json = serializer.Serialize(new Dictionary<string, Corge> { { "foo", new Corge { Bar = "abc", Baz = true } } });

            Assert.That(json, Is.EqualTo(@"{""foo"":{""Bar"":""abc"",""Baz"":true}}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfCustomTypeToCustomType()
        {
            var serializer = new JsonSerializer<IDictionary<Grault, Corge>>();

            var json = serializer.Serialize(new Dictionary<Grault, Corge> { { new Grault { Bar = "xyz", Baz = false }, new Corge { Bar = "abc", Baz = true } } });

            Assert.That(json, Is.EqualTo(@"{""{\""Bar\"":\""xyz\"",\""Baz\"":false}"":{""Bar"":""abc"",""Baz"":true}}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfValueTypeToCustomType()
        {
            var serializer = new JsonSerializer<IDictionary<int, Corge>>();

            var json = serializer.Serialize(new Dictionary<int, Corge> { { 1, new Corge { Bar = "abc", Baz = true } } });

            Assert.That(json, Is.EqualTo(@"{""1"":{""Bar"":""abc"",""Baz"":true}}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomTypeEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<IDictionary<string, Corge>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, Corge> { { "foo", new Corge { Bar = "abc", Baz = true } }});

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""foo"":{""Bar"":""abc"",""Baz"":true}}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomTypeWithTypeDecoratedWithEncryptAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, Foo>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, Foo> { { "foo", new Foo { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""abc"",""Baz"":true}")
                    + @""""
                + "}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomTypeWithPropertyDecoratedWithEncryptAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, Qux>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, Qux> { { "foo", new Qux { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""abc""")
                    + @""""
                + @",""Baz"":true}}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNonGenericIDictionary()
        {
            var serializer = new JsonSerializer<Hashtable>();

            var json = serializer.Serialize(new Hashtable { { "foo", new Qux { Bar = "abc", Baz = true } } });

            var expected = @"{""foo"":{""Bar"":""abc"",""Baz"":true}}";

            Assert.That(json, Is.EqualTo(expected));
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

        public class Corge
        {
            public string Bar { get; set; }
            public bool Baz { get; set; }
        }

        public class Grault
        {
            public string Bar { get; set; }

            public bool Baz { get; set; }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Bar != null ? Bar.GetHashCode() : 0) * 397) ^ Baz.GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                var other = obj as Grault;

                if (other == null)
                {
                    return false;
                }

                return Bar == other.Bar && Baz == other.Baz;
            }
        }
    }
}