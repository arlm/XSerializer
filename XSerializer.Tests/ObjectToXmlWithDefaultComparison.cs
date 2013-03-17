using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public abstract class ObjectToXmlWithDefaultComparison
    {
        [TestCaseSource("TestCaseData")]
        public void RoundTripsCorrectly(object instance, Type type)
        {
            var customSerializer = CustomSerializer.GetSerializer(type, null, null, null);
            var defaultSerializer = DefaultSerializer.GetSerializer(type, null, null, null);

            var customXml = customSerializer.SerializeObject(instance, null, Encoding.UTF8, Formatting.Indented, AlwaysEmitTypes);
            var defaultXml = defaultSerializer.SerializeObject(instance, null, Encoding.UTF8, Formatting.Indented, AlwaysEmitTypes);

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Custom XML:");
            Console.WriteLine(customXml);

            Assert.That(customXml, Is.EqualTo(defaultXml));

            AdditionalAssertions(instance, type, customXml, defaultXml);
        }

        protected virtual bool AlwaysEmitTypes
        {
            get { return false; }
        }

        protected virtual void AdditionalAssertions(object instance, Type type, string customXml, string defaultXml)
        {
        }

        protected IEnumerable<TestCaseData> TestCaseData
        {
            get
            {
                return GetTestCaseData().Select(testCaseData =>
                {
                    if (string.IsNullOrWhiteSpace(testCaseData.TestName))
                    {
                        var instanceType = testCaseData.Arguments[0].GetType();
                        var type = (Type)testCaseData.Arguments[1];

                        return testCaseData.SetName(type == instanceType ? type.Name : string.Format("{0} as {1}", instanceType.Name, type.Name));
                    }

                    return testCaseData;
                });
            }
        }

        protected abstract IEnumerable<TestCaseData> GetTestCaseData();
    }
}