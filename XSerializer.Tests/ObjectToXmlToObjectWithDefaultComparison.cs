using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public abstract class ObjectToXmlToObjectWithDefaultComparison
    {
        [TestCaseSource("TestCaseData")]
        public void RoundTripsCorrectly(object instance, Type type)
        {
            var customSerializer = CustomSerializer.GetSerializer(type, null, null, null);
            var defaultSerializer = DefaultSerializer.GetSerializer(type, null, null, null);

            var interfaceXml = customSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, null);
            var defaultXml = defaultSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, null);

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Interface XML:");
            Console.WriteLine(interfaceXml);

            Assert.That(interfaceXml, Is.EqualTo(defaultXml));

            var interfaceInstance = customSerializer.DeserializeObject(interfaceXml);
            var defaultInstance = defaultSerializer.DeserializeObject(defaultXml);

            Assert.That(interfaceInstance, Has.PropertiesEqualTo(defaultInstance));
            Assert.That(interfaceInstance, Has.PropertiesEqualTo(instance));
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