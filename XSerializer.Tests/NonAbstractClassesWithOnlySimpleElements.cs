using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    [TestFixture]
    public class NonAbstractClassesWithOnlySimpleElements : ObjectToXmlToObjectWithDefaultComparison
    {
        protected override IEnumerable<TestCaseData> GetTestCaseData()
        {
            yield return new TestCaseData(
                new Child1 { ByteValue = 1, Int32Value = 2, UInt64Value = 3 },
                typeof(Child1));

            yield return new TestCaseData(
                new TopLevelParent
                {
                    Parent1 =
                        new Parent1
                        {
                            Child1 =
                                new Child1
                                {
                                    ByteValue = 1,
                                    Int32Value = 2,
                                    UInt64Value = 3
                                },
                            Child2 =
                                new Child2
                                {
                                    DecimalValue = 12.34M,
                                    DoubleValue = 23.45,
                                    SingleValue = 34.56F
                                }
                        },
                    Parent2 =
                        new Parent2
                        {
                            Child3 =
                                new Child3
                                {
                                    StringValue = "abc"
                                },
                            Child4 =
                                new Child4
                                {
                                    MyEnumeration = MyEnumeration.Value3
                                }
                        }
                },
                typeof(TopLevelParent));
            
            yield return new TestCaseData(
                new TopLevelParent
                {
                    Parent2 =
                        new Parent2
                            {
                                Child3 = new Child3()
                            }
                },
                typeof(TopLevelParent)).SetName("TopLevelParent with missing children");
        }

        public class TopLevelParent
        {
            public Parent1 Parent1 { get; set; }

            public Parent2 Parent2 { get; set; }
        }

        public class Parent1
        {
            public Child1 Child1 { get; set; }

            public Child2 Child2 { get; set; }
        }

        public class Parent2
        {
            public Child3 Child3 { get; set; }

            public Child4 Child4 { get; set; }
        }

        public class Child1
        {
            public int Int32Value { get; set; }

            public byte ByteValue { get; set; }

            public ulong UInt64Value { get; set; }
        }

        public class Child2
        {
            public float SingleValue { get; set; }

            public double DoubleValue { get; set; }

            public decimal DecimalValue { get; set; }
        }

        public class Child3
        {
            public string StringValue { get; set; }
        }

        public class Child4
        {
            public MyEnumeration MyEnumeration { get; set; }
        }

        public enum MyEnumeration
        {
            Value1,
            Value2,
            Value3
        }
    }
}