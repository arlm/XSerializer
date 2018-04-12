using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class PrimitiveArrayTests
    {
        [TestCaseSource("GetTestCases")]
        public void CanSerializeArray(dynamic item)
        {
            var serializer = XmlSerializer.Create(item.GetType());

            var xml = serializer.Serialize(item);

            dynamic roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Data, Is.EqualTo(item.Data));
        }

        private static IEnumerable<TestCaseData> GetTestCases()
        {
            yield return new TestCaseData(new ByteArrayContainer
            {
                Data = new byte[] { 0, 1, 2, 4, 8, 16, 32, 64, 128 }
            }).SetName("Byte Array");

            yield return new TestCaseData(new Int32ArrayContainer
            {
                Data = new[] { 0, 1, 2, 4, 8, 16, 32, 64, 128 }
            }).SetName("Int32 Array");

            yield return new TestCaseData(new DateTimeArrayContainer
            {
                Data = new[] { DateTime.MinValue, DateTime.MaxValue, DateTime.Now, DateTime.UtcNow }
            }).SetName("DateTime Array");

            yield return new TestCaseData(new EnumArrayContainer
            {
                Data = new[] { Choice.Yes, Choice.No, Choice.FileNotFound }
            }).SetName("Enum Array");

            yield return new TestCaseData(new BooleanArrayContainer
            {
                Data = new[] { true, false }
            }).SetName("Boolean Array");

            yield return new TestCaseData(new NullableByteArrayContainer
            {
                Data = new byte?[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, null }
            }).SetName("Nullable Byte Array");

            yield return new TestCaseData(new NullableInt32ArrayContainer
            {
                Data = new int?[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, null }
            }).SetName("Nullable Int32 Array");

            yield return new TestCaseData(new NullableDateTimeArrayContainer
            {
                Data = new DateTime?[] { DateTime.MinValue, DateTime.MaxValue, DateTime.Now, DateTime.UtcNow, null }
            }).SetName("Nullable DateTime Array");

            yield return new TestCaseData(new NullableEnumArrayContainer
            {
                Data = new Choice?[] { Choice.Yes, Choice.No, Choice.FileNotFound, null }
            }).SetName("Nullable Enum Array");

            yield return new TestCaseData(new NullableBooleanArrayContainer
            {
                Data = new bool?[] { true, false, null }
            }).SetName("Nullable Boolean Array");
        }

        public class ByteArrayContainer
        {
            public byte[] Data { get; set; }
        }

        public class Int32ArrayContainer
        {
            public int[] Data { get; set; }
        }

        public class DateTimeArrayContainer
        {
            public DateTime[] Data { get; set; }
        }

        public class EnumArrayContainer
        {
            public Choice[] Data { get; set; }
        }

        public class BooleanArrayContainer
        {
            public bool[] Data { get; set; }
        }

        public class NullableByteArrayContainer
        {
            public byte?[] Data { get; set; }
        }

        public class NullableInt32ArrayContainer
        {
            public int?[] Data { get; set; }
        }

        public class NullableDateTimeArrayContainer
        {
            public DateTime?[] Data { get; set; }
        }

        public class NullableEnumArrayContainer
        {
            public Choice?[] Data { get; set; }
        }

        public class NullableBooleanArrayContainer
        {
            public bool?[] Data { get; set; }
        }

        public enum Choice
        {
            Yes, No, FileNotFound
        }
    }
}
