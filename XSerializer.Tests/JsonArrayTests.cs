using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class JsonArrayTests
    {
        #region Non-converting retrieval tests

        [TestCase(true)]
        [TestCase("abc")]
        [TestCase(null)]
        public void CanRetrieveNonNumericJsonPrimitives(object barValue)
        {
            var foo = new JsonArray
            {
                barValue,
            };

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(barValue));
        }

        [Test]
        public void CanRetrieveNumericJsonPrimitive()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"),
            };

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123.45));
            Assert.That(bar, Is.InstanceOf<double>());
        }

        #endregion

        #region TransformItems tests

        [Test]
        public void CanTransformItemsToDateTime()
        {
            var value = DateTime.Now;

            var foo = new JsonArray
            {
                value.ToString("O"),
            };

            foo.TransformItems<DateTime>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanTransformItemsToString()
        {
            var foo = new JsonArray
            {
                "abc"
            };

            foo.TransformItems<string>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo("abc"));
        }

        [Test]
        public void CanTransformItemsToDateTimeOffset()
        {
            var value = DateTimeOffset.Now;

            var foo = new JsonArray
            {
                value.ToString("O"),
            };

            foo.TransformItems<DateTimeOffset>();

            var bar = foo[0];

            Assert.That(bar, Is.InstanceOf<DateTimeOffset>());
            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanTransformItemsToGuid()
        {
            var value = Guid.NewGuid();

            var foo = new JsonArray
            {
                value.ToString("D"),
            };

            foo.TransformItems<Guid>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanTransformItemsToByte()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<byte>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<byte>());
        }

        [Test]
        public void CanTransformItemsToSByte()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<sbyte>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<sbyte>());
        }

        [Test]
        public void CanTransformItemsToInt16()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<short>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<short>());
        }

        [Test]
        public void CanTransformItemsToUInt16()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<ushort>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<ushort>());
        }

        [Test]
        public void CanTransformItemsToInt32()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<int>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<int>());
        }

        [Test]
        public void CanTransformItemsToUInt32()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<uint>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<uint>());
        }

        [Test]
        public void CanTransformItemsToInt64()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<long>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<long>());
        }

        [Test]
        public void CanTransformItemsToUInt64()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            foo.TransformItems<ulong>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(bar, Is.InstanceOf<ulong>());
        }

        [Test]
        public void CanTransformItemsToSingle()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"),
            };

            foo.TransformItems<float>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123.45F));
            Assert.That(bar, Is.InstanceOf<float>());
        }

        [Test]
        public void CanTransformItemsToDouble()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"),
            };

            foo.TransformItems<double>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123.45));
            Assert.That(bar, Is.InstanceOf<double>());
        }

        [Test]
        public void CanTransformItemsToDecimal()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"),
            };

            foo.TransformItems<decimal>();

            var bar = foo[0];

            Assert.That(bar, Is.EqualTo(123.45M));
            Assert.That(bar, Is.InstanceOf<decimal>());
        }

        #endregion

        #region Implicit conversion tests

        [Test]
        public void CanConvertToListOfObject()
        {
            var foo = new JsonArray
            {
                true, new JsonNumber("123.45")
            };

            List<object> fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.EqualTo(123.45));
        }

        [Test]
        public void CanConvertToObjectArray()
        {
            var foo = new JsonArray
            {
                true, new JsonNumber("123.45")
            };

            object[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.EqualTo(123.45));
        }

        [Test]
        public void CanConvertToJsonObjectArray()
        {
            var foo = new JsonArray
            {
                new JsonObject
                {
                    { "foo", true }
                }
            };

            JsonObject[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar["foo"], Is.True);
        }

        [Test]
        public void CanConvertToJsonArrayArray()
        {
            var foo = new JsonArray
            {
                new JsonArray
                {
                    true
                }
            };

            JsonArray[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar[0], Is.True);
        }

        [Test]
        public void CanConvertToStringArray()
        {
            var foo = new JsonArray
            {
                "abc",
            };

            string[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo("abc"));
        }

        [Test]
        public void CanConvertToDateTimeArray()
        {
            var now = DateTime.Now;

            var foo = new JsonArray
            {
                now.ToString("O"),
            };

            DateTime[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(now));
        }

        [Test]
        public void CanConvertToNullableDateTimeArray()
        {
            var now = DateTime.Now;

            var foo = new JsonArray
            {
                now.ToString("O"), null
            };

            DateTime?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(now));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToDateTimeOffsetArray()
        {
            var now = DateTimeOffset.Now;

            var foo = new JsonArray
            {
                now.ToString("O"),
            };

            DateTimeOffset[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(now));
        }

        [Test]
        public void CanConvertToNullableDateTimeOffsetArray()
        {
            var now = DateTimeOffset.Now;

            var foo = new JsonArray
            {
                now.ToString("O"), null
            };

            DateTimeOffset?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(now));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToGuidArray()
        {
            var guid = Guid.NewGuid();

            var foo = new JsonArray
            {
                guid.ToString("D"),
            };

            Guid[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(guid));
        }

        [Test]
        public void CanConvertToNullableGuidArray()
        {
            var guid = Guid.NewGuid();

            var foo = new JsonArray
            {
                guid.ToString("D"), null
            };

            Guid?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(guid));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToBooleanArray()
        {
            var foo = new JsonArray
            {
                true,
            };

            bool[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.True);
        }

        [Test]
        public void CanConvertToNullableBooleanArray()
        {
            var foo = new JsonArray
            {
                true, null
            };

            bool?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToByteArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            byte[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableByteArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            byte?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToSByteArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            sbyte[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableSByteArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            sbyte?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToInt16Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            short[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableInt16Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            short?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToUInt16Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            ushort[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableUInt16Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ushort?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToInt32Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            int[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableInt32Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            int?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToUInt32Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            uint[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableUInt32Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            uint?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToInt64Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            long[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableInt64Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            long?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToUInt64Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"),
            };

            ulong[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanConvertToNullableUInt64Array()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ulong?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToSingleArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"),
            };

            float[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123.45F));
        }

        [Test]
        public void CanConvertToNullableSingleArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            float?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123.45F));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToDoubleArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"),
            };

            double[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123.45));
        }

        [Test]
        public void CanConvertToNullableDoubleArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            double?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123.45));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanConvertToDecimalArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"),
            };

            decimal[] fooConverted = foo;

            var bar = fooConverted[0];

            Assert.That(bar, Is.EqualTo(123.45M));
        }

        [Test]
        public void CanConvertToNullableDecimalArray()
        {
            var foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            decimal?[] fooConverted = foo;

            var bar = fooConverted[0];
            var baz = fooConverted[1];

            Assert.That(bar, Is.EqualTo(123.45M));
            Assert.That(baz, Is.Null);
        }

        #endregion

        #region Dynamic conversion tests

        [Test]
        public void CanDynamicConvertToIEnumerableOfObject()
        {
            dynamic foo = new JsonArray
            {
                true, "abc"
            };

            IList<object> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfObject()
        {
            dynamic foo = new JsonArray
            {
                true, "abc"
            };

            IList<object> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDynamicConvertToIListOfObject()
        {
            dynamic foo = new JsonArray
            {
                true, "abc"
            };

            IList<object> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfJsonObject()
        {
            dynamic foo = new JsonArray
            {
                new JsonObject
                {
                    { "bar", true }
                },
                new JsonObject
                {
                    { "bar", false }
                }
            };

            IEnumerable<JsonObject> fooConverted = foo;

            var foo1 = fooConverted.First();
            var foo2 = fooConverted.Skip(1).First();

            Assert.That(foo1["bar"], Is.True);
            Assert.That(foo2["bar"], Is.False);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfJsonObject()
        {
            dynamic foo = new JsonArray
            {
                new JsonObject
                {
                    { "bar", true }
                },
                new JsonObject
                {
                    { "bar", false }
                }
            };

            ICollection<JsonObject> fooConverted = foo;

            var foo1 = fooConverted.First();
            var foo2 = fooConverted.Skip(1).First();

            Assert.That(foo1["bar"], Is.True);
            Assert.That(foo2["bar"], Is.False);
        }

        [Test]
        public void CanDynamicConvertToIListOfJsonObject()
        {
            dynamic foo = new JsonArray
            {
                new JsonObject
                {
                    { "bar", true }
                },
                new JsonObject
                {
                    { "bar", false }
                }
            };

            IList<JsonObject> fooConverted = foo;

            var foo1 = fooConverted.First();
            var foo2 = fooConverted.Skip(1).First();

            Assert.That(foo1["bar"], Is.True);
            Assert.That(foo2["bar"], Is.False);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfJsonArray()
        {
            dynamic foo = new JsonArray
            {
                new JsonArray
                {
                    "abc", "xyz"
                },
                new JsonArray
                {
                    "ABC", "XYZ"
                }
            };

            IEnumerable<JsonArray> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar[0], Is.EqualTo("abc"));
            Assert.That(bar[1], Is.EqualTo("xyz"));
            Assert.That(baz[0], Is.EqualTo("ABC"));
            Assert.That(baz[1], Is.EqualTo("XYZ"));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfJsonArray()
        {
            dynamic foo = new JsonArray
            {
                new JsonArray
                {
                    "abc", "xyz"
                },
                new JsonArray
                {
                    "ABC", "XYZ"
                }
            };

            ICollection<JsonArray> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar[0], Is.EqualTo("abc"));
            Assert.That(bar[1], Is.EqualTo("xyz"));
            Assert.That(baz[0], Is.EqualTo("ABC"));
            Assert.That(baz[1], Is.EqualTo("XYZ"));
        }

        [Test]
        public void CanDynamicConvertToIListOfJsonArray()
        {
            dynamic foo = new JsonArray
            {
                new JsonArray
                {
                    "abc", "xyz"
                },
                new JsonArray
                {
                    "ABC", "XYZ"
                }
            };

            IList<JsonArray> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar[0], Is.EqualTo("abc"));
            Assert.That(bar[1], Is.EqualTo("xyz"));
            Assert.That(baz[0], Is.EqualTo("ABC"));
            Assert.That(baz[1], Is.EqualTo("XYZ"));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfBoolean()
        {
            dynamic foo = new JsonArray
            {
                true
            };

            IEnumerable<bool> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.True);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfBoolean()
        {
            dynamic foo = new JsonArray
            {
                true
            };

            ICollection<bool> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.True);
        }

        [Test]
        public void CanDynamicConvertToIListOfBoolean()
        {
            dynamic foo = new JsonArray
            {
                true
            };

            IList<bool> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.True);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableBoolean()
        {
            dynamic foo = new JsonArray
            {
                true, null
            };

            IEnumerable<bool?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableBoolean()
        {
            dynamic foo = new JsonArray
            {
                true, null
            };

            ICollection<bool?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableBoolean()
        {
            dynamic foo = new JsonArray
            {
                true, null
            };

            IList<bool?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.True);
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<byte> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<byte> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<byte> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<byte?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<byte?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<byte?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfSByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<sbyte> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfSByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<sbyte> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfSByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<sbyte> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableSByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<sbyte?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableSByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<sbyte?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableSByte()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<sbyte?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<short> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<short> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<short> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<short?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<short?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<short?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfUInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<ushort> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfUInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<ushort> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfUInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<ushort> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableUInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<ushort?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableUInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<ushort?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableUInt16()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<ushort?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<int> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<int> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<int> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<int?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<int?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<int?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfUInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<uint> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfUInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<uint> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfUInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<uint> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableUInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<uint?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableUInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<uint?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableUInt32()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<uint?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<long> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<long> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<long> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<long?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<long?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<long?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfUInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IEnumerable<ulong> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfUInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            ICollection<ulong> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIListOfUInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123")
            };

            IList<ulong> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableUInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IEnumerable<ulong?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableUInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            ICollection<ulong?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableUInt64()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123"), null
            };

            IList<ulong?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfSingle()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            IEnumerable<float> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45F));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfSingle()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            ICollection<float> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45F));
        }

        [Test]
        public void CanDynamicConvertToIListOfSingle()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            IList<float> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45F));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableSingle()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            IEnumerable<float?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45F));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableSingle()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            ICollection<float?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45F));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableSingle()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            IList<float?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45F));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfDouble()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            IEnumerable<double> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfDouble()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            ICollection<double> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDynamicConvertToIListOfDouble()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            IList<double> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableDouble()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            IEnumerable<double?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableDouble()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            ICollection<double?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableDouble()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            IList<double?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfDecimal()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            IEnumerable<decimal> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45M));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfDecimal()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            ICollection<decimal> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45M));
        }

        [Test]
        public void CanDynamicConvertToIListOfDecimal()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45")
            };

            IList<decimal> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(123.45M));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableDecimal()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            IEnumerable<decimal?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45M));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableDecimal()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            ICollection<decimal?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45M));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableDecimal()
        {
            dynamic foo = new JsonArray
            {
                new JsonNumber("123.45"), null
            };

            IList<decimal?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(123.45M));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfString()
        {
            dynamic foo = new JsonArray
            {
                "abc"
            };

            IEnumerable<string> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfString()
        {
            dynamic foo = new JsonArray
            {
                "abc"
            };

            ICollection<string> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDynamicConvertToIListOfString()
        {
            dynamic foo = new JsonArray
            {
                "abc"
            };

            IList<string> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfDateTime()
        {
            var value = DateTime.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O")
            };

            IEnumerable<DateTime> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfDateTime()
        {
            var value = DateTime.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O")
            };

            ICollection<DateTime> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToIListOfDateTime()
        {
            var value = DateTime.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O")
            };

            IList<DateTime> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableDateTime()
        {
            var value = DateTime.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O"), null
            };

            IEnumerable<DateTime?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableDateTime()
        {
            var value = DateTime.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O"), null
            };

            ICollection<DateTime?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableDateTime()
        {
            var value = DateTime.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O"), null
            };

            IList<DateTime?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfDateTimeOffset()
        {
            var value = DateTimeOffset.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O")
            };

            IEnumerable<DateTimeOffset> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfDateTimeOffset()
        {
            var value = DateTimeOffset.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O")
            };

            ICollection<DateTimeOffset> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToIListOfDateTimeOffset()
        {
            var value = DateTimeOffset.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O")
            };

            IList<DateTimeOffset> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableDateTimeOffset()
        {
            var value = DateTimeOffset.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O"), null
            };

            IEnumerable<DateTimeOffset?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableDateTimeOffset()
        {
            var value = DateTimeOffset.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O"), null
            };

            ICollection<DateTimeOffset?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableDateTimeOffset()
        {
            var value = DateTimeOffset.Now;

            dynamic foo = new JsonArray
            {
                value.ToString("O"), null
            };

            IList<DateTimeOffset?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfGuid()
        {
            var value = Guid.NewGuid();

            dynamic foo = new JsonArray
            {
                value.ToString("D")
            };

            IEnumerable<Guid> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToICollectionOfGuid()
        {
            var value = Guid.NewGuid();

            dynamic foo = new JsonArray
            {
                value.ToString("D")
            };

            ICollection<Guid> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToIListOfGuid()
        {
            var value = Guid.NewGuid();

            dynamic foo = new JsonArray
            {
                value.ToString("D")
            };

            IList<Guid> fooConverted = foo;

            var bar = fooConverted.First();

            Assert.That(bar, Is.EqualTo(value));
        }

        [Test]
        public void CanDynamicConvertToIEnumerableOfNullableGuid()
        {
            var value = Guid.NewGuid();

            dynamic foo = new JsonArray
            {
                value.ToString("D"), null
            };

            IEnumerable<Guid?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToICollectionOfNullableGuid()
        {
            var value = Guid.NewGuid();

            dynamic foo = new JsonArray
            {
                value.ToString("D"), null
            };

            ICollection<Guid?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        [Test]
        public void CanDynamicConvertToIListOfNullableGuid()
        {
            var value = Guid.NewGuid();

            dynamic foo = new JsonArray
            {
                value.ToString("D"), null
            };

            IList<Guid?> fooConverted = foo;

            var bar = fooConverted.First();
            var baz = fooConverted.Skip(1).First();

            Assert.That(bar, Is.EqualTo(value));
            Assert.That(baz, Is.Null);
        }

        #endregion

        #region Decrypt tests

        [TestCase(@"true", true)]
        [TestCase(@"false", false)]
        [TestCase(@"""abc""", "abc")]
        [TestCase(@"123.45", 123.45)]
        public void CanDecryptPrimitiveItem(string jsonValue, object expectedValue)
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            dynamic foo =
                new JsonArray(encryptionMechanism:encryptionMechanism)
                {
                    encryptionMechanism.Encrypt(jsonValue),
                };

            string barEncrypted = foo[0];
            foo.Decrypt(0);
            object bar = foo[0];

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CanDecryptJsonObjectItem()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            dynamic foo =
                new JsonArray(encryptionMechanism:encryptionMechanism)
                {
                    encryptionMechanism.Encrypt(@"{""baz"":false,""qux"":123.45}"),
                };

            string barEncrypted = foo[0];
            foo.Decrypt(0);
            dynamic bar = foo[0];

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar.baz, Is.False);
            Assert.That(bar.qux, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDecryptJsonArrayItem()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            dynamic foo =
                new JsonArray(encryptionMechanism:encryptionMechanism)
                {
                    encryptionMechanism.Encrypt(@"[1,2,3]"),
                };

            string barEncrypted = foo[0];
            foo.Decrypt(0);
            IList<int> bar = foo[0];

            Assert.That(bar, Is.Not.EqualTo(barEncrypted));

            Assert.That(bar[0], Is.EqualTo(1));
            Assert.That(bar[1], Is.EqualTo(2));
            Assert.That(bar[2], Is.EqualTo(3));
        }

        #endregion
    }
}