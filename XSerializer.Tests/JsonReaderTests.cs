using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    internal class JsonReaderTests
    {
        [TestCase("true", new[] { JsonNodeType.Boolean })]
        [TestCase("false", new[] { JsonNodeType.Boolean })]
        [TestCase("null", new[] { JsonNodeType.Null })]
        [TestCase("123.45", new[] { JsonNodeType.Number })]
        [TestCase(@"""abc""", new[] { JsonNodeType.String })]
        [TestCase(@"{""foo"":""bar""}", JsonNodeType.OpenObject, JsonNodeType.String, JsonNodeType.NameValueSeparator, JsonNodeType.String, JsonNodeType.CloseObject)]
        [TestCase(@"[""foo"",""bar""]", JsonNodeType.OpenArray, JsonNodeType.String, JsonNodeType.ItemSeparator, JsonNodeType.String, JsonNodeType.CloseArray)]
        public void CanReadEachNodeType(string json, params JsonNodeType[] expectedNodeTypes)
        {
            var reader = new JsonReader(new StringReader(json), new JsonSerializeOperationInfo());
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.None));

            foreach (var expectedNodeType in expectedNodeTypes)
            {
                reader.Read();
                Assert.That(reader.NodeType, Is.EqualTo(expectedNodeType));
            }

            reader.Read();
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.EndOfString));
        }

        [TestCase("true", new[] { JsonNodeType.Boolean })]
        [TestCase("false", new[] { JsonNodeType.Boolean })]
        [TestCase("null", new[] { JsonNodeType.Null })]
        [TestCase("123.45", new[] { JsonNodeType.Number })]
        [TestCase(@"""abc""", new[] { JsonNodeType.String })]
        [TestCase(@"{""foo"":""bar""}", JsonNodeType.OpenObject, JsonNodeType.String, JsonNodeType.NameValueSeparator, JsonNodeType.String, JsonNodeType.CloseObject)]
        [TestCase(@"[""foo"",""bar""]", JsonNodeType.OpenArray, JsonNodeType.String, JsonNodeType.ItemSeparator, JsonNodeType.String, JsonNodeType.CloseArray)]
        public void CanDecryptCurrentStringValueAndAccessDecryptedNodes(string plainTextJson, params JsonNodeType[] expectedNodeTypes)
        {
            var cipherTextJson = @"""" + Convert.ToBase64String(Encoding.UTF8.GetBytes(plainTextJson)) + @"""";

            var info = new JsonSerializeOperationInfo
            {
                EncryptionMechanism = new Base64EncryptionMechanism(),
            };

            var reader = new JsonReader(new StringReader(cipherTextJson), info);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.None));

            reader.Read();
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.String));

            reader.DecryptReads = true;

            foreach (var expectedNodeType in expectedNodeTypes)
            {
                Assert.That(reader.NodeType, Is.EqualTo(expectedNodeType));
                reader.Read();
            }

            reader.DecryptReads = false;

            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.EndOfString));
        }

        [Test]
        public void ThrowsExceptionWhenSettingDecryptReadsToFalseBeforeEncryptedDataIsCompletelyConsumed()
        {
            var cipherTextJson = @"""" + Convert.ToBase64String(Encoding.UTF8.GetBytes(@"{""foo"":""bar""}")) + @"""";

            var info = new JsonSerializeOperationInfo
            {
                EncryptionMechanism = new Base64EncryptionMechanism(),
            };

            var reader = new JsonReader(new StringReader(cipherTextJson), info);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.None));

            reader.Read();
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.String));

            reader.DecryptReads = true;

            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.OpenObject));

            reader.Read();
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.String));

            Assert.That(() => reader.DecryptReads = false, Throws.InvalidOperationException);
        }

        [Test]
        public void ReadContentSkipsWhitespace()
        {
            const string json = @"
{
	""foo"" : 123.45
}
";
            var reader = new JsonReader(new StringReader(json), new JsonSerializeOperationInfo());

            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.None));
            Assert.That(reader.ReadContent(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.OpenObject));
            Assert.That(reader.ReadContent(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.String));
            Assert.That(reader.ReadContent(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.NameValueSeparator));
            Assert.That(reader.ReadContent(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.Number));
            Assert.That(reader.ReadContent(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.CloseObject));
            Assert.That(reader.ReadContent(), Is.False);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.EndOfString));
        }

        [Test]
        public void ReadPropertiesReadsAJsonObjectsProperties()
        {
            const string json = @"{""foo"":123.45,""bar"":true,""baz"":""abc""}";

            var reader = new JsonReader(new StringReader(json), new JsonSerializeOperationInfo());

            Assert.That(reader.ReadContent(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.OpenObject));

            var enumerator = reader.ReadProperties().GetEnumerator();

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo("foo"));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.Number));
            Assert.That(reader.Value, Is.EqualTo("123.45"));

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo("bar"));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.Boolean));
            Assert.That(reader.Value, Is.True);

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo("baz"));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.String));
            Assert.That(reader.Value, Is.EqualTo("abc"));

            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        public void ReadingPastTheEndOfTheStreamReturnsFalse()
        {
            var reader = new JsonReader(new StringReader("123"), new JsonSerializeOperationInfo());

            Assert.That(reader.ReadContent(), Is.True);
            Assert.That(reader.Value, Is.EqualTo("123"));
            Assert.That(reader.ReadContent(), Is.False);
            Assert.That(reader.ReadContent(), Is.False); // end of stream
            Assert.That(reader.ReadContent(), Is.False); // end of stream
        }

        [Test]
        public void CanSetDecryptReadsToTrueAndBackAgainWhenTheNodeTypeIsNull()
        {
            var reader = new JsonReader(
                new StringReader("null"),
                new JsonSerializeOperationInfo
                {
                    EncryptionMechanism = new Base64EncryptionMechanism()
                });

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.NodeType, Is.EqualTo(JsonNodeType.Null));

            Assert.That(() => reader.DecryptReads = true, Throws.Nothing);
            Assert.That(() => reader.DecryptReads = false, Throws.Nothing);
        }

        [TestCase("null")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("123.45")]
        [TestCase(@"""abc""")]
        [TestCase(@"{""foo"":true}")]
        [TestCase(@"{""foo"":{""bar"":{""baz"":null},""qux"":null}}")]
        [TestCase(@"[123.45,null]")]
        [TestCase(@"[[123.45],[null]]")]
        [TestCase(@"    null   ")]
        public void DiscardConsumesTheCurrentValue(string json)
        {
            var reader = new JsonReader(new StringReader(json), new JsonSerializeOperationInfo());

            reader.Discard();

            // There should be nothing significant left
            Assert.That(reader.ReadContent(), Is.False);
        }
    }
}