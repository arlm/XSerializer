using NUnit.Framework;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

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

        [Test]
        public void CanDeserializeReadWriteProperties()
        {
            const string json = @"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}";

            var serializer = new JsonSerializer<Bar>();

            var instance = serializer.Deserialize(json);

            Assert.That(instance.Baz.Qux, Is.EqualTo("abc"));
            Assert.That(instance.Baz.Garply, Is.EqualTo(true));
            Assert.That(instance.Corge, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeEncryptedReadWriteProperties()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var json = @"{""Qux"":"""
                + encryptionMechanism.Encrypt(@"""abc""")
                + @""",""Garply"":"""
                + encryptionMechanism.Encrypt("true")
                + @"""}";

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true
            };

            var serializer = new JsonSerializer<Waldo>(configuration);

            var instance = serializer.Deserialize(json);

            Assert.That(instance.Qux, Is.EqualTo("abc"));
            Assert.That(instance.Garply, Is.EqualTo(true));
        }

        [Test]
        public void CanSerializeWithEncryptRootObjectEnabled()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<Bar>(configuration);

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

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void AClassDecoratedWithTheEncryptAttributeIsEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true
            };

            var serializer = new JsonSerializer<Grault>(configuration);

            var instance = new Grault
            {
                Qux = "abc",
                Garply = true
            };

            var json = serializer.Serialize(instance);

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void APropertyDecoratedWithTheEncryptAttributeIsEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true
            };

            var serializer = new JsonSerializer<Waldo>(configuration);

            var instance = new Waldo
            {
                Qux = "abc",
                Garply = true
            };

            var json = serializer.Serialize(instance);

            var expected =
                @"{""Qux"":"""
                + encryptionMechanism.Encrypt(@"""abc""")
                + @""",""Garply"":"""
                + encryptionMechanism.Encrypt("true")
                + @"""}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void DuplicatedEncryptAttributesHaveNoEffectOnSerialization()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptionEnabled = true
            };

            var serializer = new JsonSerializer<Thud>(configuration);

            var instance = new Thud
            {
                Grault = new Grault
                {
                    Qux = "abc",
                    Garply = true
                },
                Waldo = new Waldo
                {
                    Qux = "abc",
                    Garply = true
                }
            };

            var json = serializer.Serialize(instance);

            var expected =
                @"{""Grault"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                    + @""""
                + @",""Waldo"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                    + @""""
                + @"}";

            Assert.That(json, Is.EqualTo(expected));
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

        [Encrypt]
        public class Grault
        {
            public string Qux { get; set; }
            public bool Garply { get; set; }
        }

        public class Waldo
        {
            [Encrypt]
            public string Qux { get; set; }
            [Encrypt]
            public bool Garply { get; set; }
        }

        public class Fred
        {
            public Baz Baz { get; set; }
            public Grault Grault { get; set; }
            public Waldo Waldo { get; set; }
        }

        public class Thud
        {
            [Encrypt]
            public Grault Grault { get; set; }
            [Encrypt]
            public Waldo Waldo { get; set; }
        }
    }
}