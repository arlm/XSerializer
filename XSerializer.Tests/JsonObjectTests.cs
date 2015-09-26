using System;
using NUnit.Framework;

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
    }
}