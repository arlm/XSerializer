using System;
using System.Collections.Generic;
using NUnit.Framework;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class JsonObjectTests
    {
        [TestCase(true)]
        [TestCase("abc")]
        [TestCase(null)]
        public void CanDynamicRetrieveNonNumericJsonPrimitives(object barValue)
        {
            dynamic foo = new JsonObject
            {
                { "bar", barValue },
            };

            object bar = foo.bar;

            Assert.That(bar, Is.EqualTo(barValue));
        }

        [Test]
        public void CanDynamicRetrieveNumericJsonPrimitive()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            object bar = foo.bar;

            Assert.That(bar, Is.EqualTo(123.45));
            Assert.That(bar, Is.InstanceOf<double>());
        }

        [Test]
        public void CanDynamicRetrieveDateTimeProjection()
        {
            var now = DateTime.Now;

            dynamic foo = new JsonObject
            {
                { "bar", now.ToString("O") },
            };

            object bar = foo.barAsDateTime;

            Assert.That(bar, Is.EqualTo(now));
        }

        [Test]
        public void CanDynamicRetrieveStringProjection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", "abc" },
            };

            object bar = foo.barAsString;

            Assert.That(bar, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDynamicRetrieveDateTimeOffsetProjection()
        {
            var now = DateTimeOffset.Now;

            dynamic foo = new JsonObject
            {
                { "bar", now.ToString("O") },
            };

            object bar = foo.barAsDateTimeOffset;

            Assert.That(bar, Is.EqualTo(now));
        }

        [Test]
        public void CanDynamicRetrieveGuidProjection()
        {
            var guid = Guid.NewGuid();

            dynamic foo = new JsonObject
            {
                { "bar", guid.ToString("D") },
            };

            object bar = foo.barAsGuid;

            Assert.That(bar, Is.EqualTo(guid));
        }

        [Test]
        public void CanDynamicRetrieveByteProjection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsByte;
            
            const byte expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<byte>());
        }

        [Test]
        public void CanDynamicRetrieveSByteProjection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsSByte;
            
            const sbyte expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<sbyte>());
        }

        [Test]
        public void CanDynamicRetrieveInt16Projection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsInt16;
            
            const short expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<short>());
        }

        [Test]
        public void CanDynamicRetrieveUInt16Projection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsUInt16;
            
            const ushort expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<ushort>());
        }

        [Test]
        public void CanDynamicRetrieveInt32Projection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsInt32;

            const int expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<int>());
        }

        [Test]
        public void CanDynamicRetrieveUInt32Projection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsUInt32;

            const uint expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<uint>());
        }

        [Test]
        public void CanDynamicRetrieveInt64Projection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsInt64;

            const long expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<long>());
        }

        [Test]
        public void CanDynamicRetrieveUInt64Projection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            object bar = foo.barAsUInt64;

            const ulong expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<ulong>());
        }

        [Test]
        public void CanDynamicRetrieveSingleProjection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            object bar = foo.barAsSingle;

            const float expected = 123.45F;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<float>());
        }

        [Test]
        public void CanDynamicRetrieveDoubleProjection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            object bar = foo.barAsDouble;

            const double expected = 123.45;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<double>());
        }

        [Test]
        public void CanDynamicRetrieveDecimalProjection()
        {
            dynamic foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            object bar = foo.barAsDecimal;

            const decimal expected = 123.45M;

            Assert.That(bar, Is.EqualTo(expected));
            Assert.That(bar, Is.InstanceOf<decimal>());
        }
        
        [TestCase(true)]
        [TestCase("abc")]
        [TestCase(null)]
        public void CanIndexerRetrieveNonNumericJsonPrimitives(object barValue)
        {
            var foo = new JsonObject
            {
                { "bar", barValue },
            };

            var bar = foo["bar"];

            Assert.That(bar, Is.EqualTo(barValue));
        }

        [Test]
        public void CanIndexerRetrieveNumericJsonPrimitive()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            var bar = foo["bar"];

            Assert.That(bar, Is.EqualTo(123.45));
        }

        [Test]
        public void CanIndexerRetrieveStringProjection()
        {
            var foo = new JsonObject
            {
                { "bar", "abc" },
            };

            var bar = foo["barAsString"];

            Assert.That(bar, Is.EqualTo("abc"));
        }

        [Test]
        public void CanIndexerRetrieveDateTimeProjection()
        {
            var now = DateTime.Now;

            var foo = new JsonObject
            {
                { "bar", now.ToString("O") },
            };

            var bar = foo["barAsDateTime"];

            Assert.That(bar, Is.EqualTo(now));
        }

        [Test]
        public void CanIndexerRetrieveDateTimeOffsetProjection()
        {
            var now = DateTimeOffset.Now;

            var foo = new JsonObject
            {
                { "bar", now.ToString("O") },
            };

            var bar = foo["barAsDateTimeOffset"];

            Assert.That(bar, Is.EqualTo(now));
        }

        [Test]
        public void CanIndexerRetrieveGuidProjection()
        {
            var guid = Guid.NewGuid();

            var foo = new JsonObject
            {
                { "bar", guid.ToString("D") },
            };

            var bar = foo["barAsGuid"];

            Assert.That(bar, Is.EqualTo(guid));
        }

        [Test]
        public void CanIndexerRetrieveByteProjection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsByte"];
            
            const byte expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveSByteProjection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsSByte"];
            
            const sbyte expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveInt16Projection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsInt16"];
            
            const short expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveUInt16Projection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsUInt16"];
            
            const ushort expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveInt32Projection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsInt32"];

            const int expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveUInt32Projection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsUInt32"];

            const uint expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveInt64Projection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsInt64"];

            const long expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveUInt64Projection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123") },
            };

            var bar = foo["barAsUInt64"];

            const ulong expected = 123;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveSingleProjection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            var bar = foo["barAsSingle"];

            const float expected = 123.45F;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveDoubleProjection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            var bar = foo["barAsDouble"];

            const double expected = 123.45;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [Test]
        public void CanIndexerRetrieveDecimalProjection()
        {
            var foo = new JsonObject
            {
                { "bar", new JsonNumber("123.45") },
            };

            var bar = foo["barAsDecimal"];

            const decimal expected = 123.45M;

            Assert.That(bar, Is.EqualTo(expected));
        }

        [TestCase(@"true", true)]
        [TestCase(@"false", false)]
        [TestCase(@"""abc""", "abc")]
        [TestCase(@"123.45", 123.45)]
        public void CanDecryptPrimitiveProperty(string jsonValue, object expectedValue)
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var info = new JsonSerializeOperationInfo
            {
                EncryptionEnabled = true,
                EncryptionMechanism = encryptionMechanism
            };

            dynamic foo =
                new JsonObject(info)
                {
                    { "bar", encryptionMechanism.Encrypt(jsonValue) },
                };

            string barEncrypted = foo.bar;
            foo.Decrypt("bar");
            object bar = foo.bar;

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CanDecryptJsonObjectProperty()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var info = new JsonSerializeOperationInfo
            {
                EncryptionEnabled = true,
                EncryptionMechanism = encryptionMechanism
            };

            dynamic foo =
                new JsonObject(info)
                {
                    { "bar", encryptionMechanism.Encrypt(@"{""baz"":false,""qux"":123.45}") },
                };

            string barEncrypted = foo.bar;
            foo.Decrypt("bar");
            dynamic bar = foo.bar;

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar.baz, Is.False);
            Assert.That(bar.qux, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDecryptJsonArrayProperty()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var info = new JsonSerializeOperationInfo
            {
                EncryptionEnabled = true,
                EncryptionMechanism = encryptionMechanism
            };

            dynamic foo =
                new JsonObject(info)
                {
                    { "bar", encryptionMechanism.Encrypt(@"[1,2,3]") },
                };

            string barEncrypted = foo.bar;
            foo.Decrypt("bar");
            IList<int> bar = foo.bar;

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar[0], Is.EqualTo(1));
            Assert.That(bar[1], Is.EqualTo(2));
            Assert.That(bar[2], Is.EqualTo(3));
        }

        [TestCase(true, @"true")]
        [TestCase(false, @"false")]
        [TestCase("abc", @"""abc""")]
        public void CanEncryptPrimitiveProperty(object value, string expectedPlaintextValue)
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var info = new JsonSerializeOperationInfo
            {
                EncryptionEnabled = true,
                EncryptionMechanism = encryptionMechanism
            };

            dynamic foo =
                new JsonObject(info)
                {
                    { "bar", value },
                };

            object barEncrypted = foo.bar;
            foo.Encrypt("bar");
            string bar = foo.bar;

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar, Is.EqualTo(encryptionMechanism.Encrypt(expectedPlaintextValue)));
        }

        [Test]
        public void CanEncryptNumericProperty()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var info = new JsonSerializeOperationInfo
            {
                EncryptionEnabled = true,
                EncryptionMechanism = encryptionMechanism
            };

            dynamic foo =
                new JsonObject(info)
                {
                    { "bar", new JsonNumber("123.45") },
                };

            object barEncrypted = foo.bar;
            foo.Encrypt("bar");
            string bar = foo.bar;

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar, Is.EqualTo(encryptionMechanism.Encrypt("123.45")));
        }

        [Test]
        public void CanEncryptJsonObjectProperty()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var info = new JsonSerializeOperationInfo
            {
                EncryptionEnabled = true,
                EncryptionMechanism = encryptionMechanism
            };

            dynamic foo =
                new JsonObject(info)
                {
                    {
                        "bar", new JsonObject(info)
                        {
                            { "baz", false },
                            { "qux", new JsonNumber("123.45") },
                        }
                    },
                };

            object barEncrypted = foo.bar;
            foo.Encrypt("bar");
            string bar = foo.bar;

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar, Is.EqualTo(encryptionMechanism.Encrypt(@"{""baz"":false,""qux"":123.45}")));
        }

        [Test]
        public void CanEncryptJsonArrayProperty()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var info = new JsonSerializeOperationInfo
            {
                EncryptionEnabled = true,
                EncryptionMechanism = encryptionMechanism
            };

            dynamic foo =
                new JsonObject(info)
                {
                    {
                        "bar", new JsonArray(info)
                        {
                            false,
                            new JsonNumber("123.45"),
                        }
                    },
                };

            object barEncrypted = foo.bar;
            foo.Encrypt("bar");
            string bar = foo.bar;

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar, Is.EqualTo(encryptionMechanism.Encrypt(@"[false,123.45]")));
        }

        [Test]
        public void CanDynamicSetProperty()
        {
            dynamic foo = new JsonObject
            {
                { "bar", "abc" }
            };

            Assert.That(foo.bar, Is.EqualTo("abc"));

            foo.bar = "xyz";

            Assert.That(foo.bar, Is.EqualTo("xyz"));
        }

        [Test]
        public void CanIndexerSetProperty()
        {
            var foo = new JsonObject
            {
                { "bar", "abc" }
            };

            Assert.That(foo["bar"], Is.EqualTo("abc"));

            foo["bar"] = "xyz";

            Assert.That(foo["bar"], Is.EqualTo("xyz"));
        }

        [Test]
        public void CannotDynamicSetPropertyThatDoesNotExist()
        {
            dynamic foo = new JsonObject
            {
                { "bar", "abc" }
            };

            Assert.That(() => foo.baz = "xyz", Throws.Exception);
        }

        [Test]
        public void CannotIndexerSetPropertyThatDoesNotExist()
        {
            dynamic foo = new JsonObject
            {
                { "bar", "abc" }
            };

            Assert.That(() => foo["baz"] = "xyz", Throws.Exception);
        }
    }
}