using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class ConditionalSerializationTests : ObjectToXmlWithDefaultComparison
    {
        protected override IEnumerable<TestCaseData> GetTestCaseData()
        {
            yield return new TestCaseData(CreateDemoInstance(), typeof(Demo)).SetName("Should serialize all when all conditionals return true");
            yield return new TestCaseData(CreateDemoInstance(int32ValueSpecified: false), typeof(Demo)).SetName("Should skip when Specified property returns false");
            yield return new TestCaseData(CreateDemoInstance(shouldSerializeByteValue: false), typeof(Demo)).SetName("Should skip when ShouldSerialize method returns false");
            yield return new TestCaseData(CreateDemoInstance(uInt64ValueSpecified: false), typeof(Demo)).SetName("If Specified and ShouldSerialize both exist, should skip when Specified property returns false");
            yield return new TestCaseData(CreateDemoInstance(shouldSerializeUInt64Value: false), typeof(Demo)).SetName("If Specified and ShouldSerialize both exist, should skip when ShouldSerialize method returns false");
            yield return new TestCaseData(CreateDemoInstance(uInt64ValueSpecified: false, shouldSerializeUInt64Value: false), typeof(Demo)).SetName("If Specified and ShouldSerialize both exist, should skip when both Specified property and ShouldSerialize method returns false");
        }

        private static Demo CreateDemoInstance(
            bool int32ValueSpecified = true,
            bool shouldSerializeByteValue = true,
            bool uInt64ValueSpecified = true,
            bool shouldSerializeUInt64Value = true)
        {
            return 
                new Demo(int32ValueSpecified, shouldSerializeByteValue, uInt64ValueSpecified, shouldSerializeUInt64Value)
                {
                    Int32Value = 1,
                    ByteValue = 2,
                    UInt64Value = 3,
                    Int16Value = 4
                };
        }

        public class Demo
        {
            private readonly bool _int32ValueSpecified;
            private readonly bool _shouldSerializeByteValue;
            private readonly bool _uInt64ValueSpecified;
            private readonly bool _shouldSerializeUInt64Value;

            public Demo()
            {
            }

            public Demo(bool int32ValueSpecified, bool shouldSerializeByteValue, bool uInt64ValueSpecified, bool shouldSerializeUInt64Value)
            {
                _int32ValueSpecified = int32ValueSpecified;
                _shouldSerializeByteValue = shouldSerializeByteValue;
                _uInt64ValueSpecified = uInt64ValueSpecified;
                _shouldSerializeUInt64Value = shouldSerializeUInt64Value;
            }

            public int Int32Value { get; set; }
            public bool Int32ValueSpecified
            {
                get { return _int32ValueSpecified; }
            }

            public byte ByteValue { get; set; }
            public bool ShouldSerializeByteValue()
            {
                return _shouldSerializeByteValue;
            }

            public ulong UInt64Value { get; set; }
            public bool UInt64ValueSpecified
            {
                get { return _uInt64ValueSpecified; }
            }
            public bool ShouldSerializeUInt64Value()
            {
                return _shouldSerializeUInt64Value;
            }

            public short Int16Value { get; set; }
        }
    }
}