using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace XSerializer.Tests
{
    internal abstract class XmlToObject
    {
        [TestCaseSource("TestCaseData")]
        public void DeserializesCorrectly(string xml, Type type, object expectedObject)
        {
            Console.WriteLine("Input XML:");
            Console.WriteLine(xml);

            var customSerializer = GetSerializer(type);

            var customObject = customSerializer.DeserializeObject(xml);

            Assert.That(customObject, Has.PropertiesEqualTo(expectedObject));
        }

        protected virtual IXmlSerializerInternal GetSerializer(Type type)
        {
            return CustomSerializer.GetSerializer(type, TestXmlSerializerOptions.Empty);
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