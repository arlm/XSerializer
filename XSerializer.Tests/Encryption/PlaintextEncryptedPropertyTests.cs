using NUnit.Framework;
using System;
using System.Collections.Generic;
using XSerializer.Encryption;

namespace XSerializer.Tests.Encryption
{
    public class PlaintextEncryptedPropertyTests
    {
        [Test]
        [TestCase("{\"Bar\":123,\"Baz\":true,\"Qux\":{\"Value\":\"abc\"},\"Corge\":{\"someKey\":\"someValue\"},\"Garply\":123.45,\"Grault\":[456]}")]
        [TestCase(@"{""Bar"":""Encrypted::123::Encrypted"",""Baz"":""Encrypted::true::Encrypted"",""Qux"":""Encrypted::{\""Value\"":\""abc\""}::Encrypted"",""Corge"":""Encrypted::{\""someKey\"":\""someValue\""}::Encrypted"",""Garply"":""Encrypted::123.45::Encrypted"",""Grault"":""Encrypted::[456]::Encrypted""}")]
        public void CanJsonDeserializeEncryptedPropertyWithPlaintextOrCiphertextValue(string json)
        {
            var serializer = new JsonSerializer<Foo>(
                new JsonSerializerConfiguration
                {
                    EncryptionMechanism = new FakeEncryptionMechanism()
                });

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(123));
            Assert.That(foo.Baz, Is.True);
            Assert.That(foo.Qux.Value, Is.EqualTo("abc"));
            Assert.That(foo.Corge.Count, Is.EqualTo(1));
            Assert.That(foo.Corge["someKey"], Is.EqualTo("someValue"));
            Assert.That(((JsonNumber)foo.Garply).DoubleValue, Is.EqualTo(123.45));
            Assert.That(foo.Grault.Count, Is.EqualTo(1));
            Assert.That(foo.Grault[0], Is.EqualTo(456));
        }

        [Test]
        [TestCase("<Foo><Bar>123</Bar><Baz>true</Baz><Qux><Value>abc</Value></Qux><Corge><Item><Key>someKey</Key><Value>someValue</Value></Item></Corge><Garply>123.45</Garply><Grault><Int32>456</Int32></Grault></Foo>")]
        [TestCase("<Foo><Bar>Encrypted::123::Encrypted</Bar><Baz>Encrypted::true::Encrypted</Baz><Qux>Encrypted::&lt;Value&gt;abc&lt;/Value&gt;::Encrypted</Qux><Corge>Encrypted::&lt;Item&gt;&lt;Key&gt;someKey&lt;/Key&gt;&lt;Value&gt;someValue&lt;/Value&gt;&lt;/Item&gt;::Encrypted</Corge><Garply>Encrypted::123.45::Encrypted</Garply><Grault>Encrypted::&lt;Int32&gt;456&lt;/Int32&gt;::Encrypted</Grault></Foo>")]
        public void CanXmlDeserializeEncryptedPropertyWithPlaintextOrCiphertextValue(string json)
        {
            var serializer = new XmlSerializer<Foo>(x =>
                x.WithEncryptionMechanism(new FakeEncryptionMechanism()));

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(123));
            Assert.That(foo.Baz, Is.True);
            Assert.That(foo.Qux.Value, Is.EqualTo("abc"));
            Assert.That(foo.Corge.Count, Is.EqualTo(1));
            Assert.That(foo.Corge["someKey"], Is.EqualTo("someValue"));
            Assert.That(foo.Garply, Is.EqualTo(123.45M));
            Assert.That(foo.Grault.Count, Is.EqualTo(1));
            Assert.That(foo.Grault[0], Is.EqualTo(456));
        }

        [Test]
        [TestCase(@"{""Foo"":null,""Baz"":null,""Qux"":null,""Corge"":null,""Garply"":null,""Grault"":null,""Fred"":null,""Waldo"":null}")]
        [TestCase(@"{""Foo"":""Encrypted::null::Encrypted"",""Baz"":""Encrypted::null::Encrypted"",""Qux"":""Encrypted::null::Encrypted"",""Corge"":""Encrypted::null::Encrypted"",""Garply"":""Encrypted::null::Encrypted"",""Grault"":""Encrypted::null::Encrypted"",""Fred"":""Encrypted::null::Encrypted"",""Waldo"":""Encrypted::null::Encrypted""}")]
        public void CanJsonDeserializeNullLiteralsIntoEncryptedProperties(string json)
        {
            var serializer = new JsonSerializer<Bar>(
                new JsonSerializerConfiguration
                {
                    EncryptionMechanism = new FakeEncryptionMechanism()
                });

            var bar = serializer.Deserialize(json);

            Assert.That(bar.Foo, Is.Null);
            Assert.That(bar.Baz, Is.Null);
            Assert.That(bar.Qux, Is.Null);
            Assert.That(bar.Corge, Is.Null);
            Assert.That(bar.Garply, Is.Null);
            Assert.That(bar.Grault, Is.Null);
            Assert.That(bar.Fred, Is.Null);
            Assert.That(bar.Waldo, Is.Null);
        }

        public class Bar
        {
            [Encrypt] public Dictionary<string, object> Foo { get; set; }
            [Encrypt] public int[] Baz { get; set; }
            [Encrypt] public Qux Qux { get; set; }
            [Encrypt] public bool? Corge { get; set; }
            [Encrypt] public int? Garply { get; set; }
            [Encrypt] public object Grault { get; set; }
            [Encrypt] public string Fred { get; set; }
            [Encrypt] public DateTime? Waldo { get; set; }
        }

        public class Foo
        {
            [Encrypt] public int Bar { get; set; }
            [Encrypt] public bool Baz { get; set; }
            [Encrypt] public Qux Qux { get; set; }
            [Encrypt] public Dictionary<string, string> Corge { get; set; }
            [Encrypt] public object Garply { get; set; }
            [Encrypt] public List<int> Grault { get; set; }
        }

        public class Qux
        {
            public string Value { get; set; }
        }

        public class FakeEncryptionMechanism : IEncryptionMechanism
        {
            const string header = "Encrypted::";
            const string footer = "::Encrypted";

            public string Decrypt(string cipherText, object encryptKey, SerializationState serializationState)
            {
                if (!cipherText.StartsWith(header))
                    return cipherText;
                return cipherText.Substring(header.Length, cipherText.Length - header.Length - footer.Length)
                    .Replace("&lt;", "<").Replace("&gt;", ">");
            }

            public string Encrypt(string plainText, object encryptKey, SerializationState serializationState)
            {
                return header + plainText.Replace("<", "&lt;").Replace(">", "&gt;") + footer;
            }
        }
    }
}
