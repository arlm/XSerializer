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
            var customSerializer = CustomSerializer.GetSerializer(type, TestXmlSerializerOptions.Empty);
            var defaultSerializer = DefaultSerializer.GetSerializer(type, TestXmlSerializerOptions.Empty);

            var customXml = customSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions()).StripXsiXsdDeclarations();
            var defaultXml = defaultSerializer.SerializeObject(instance, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions()).StripXsiXsdDeclarations();

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Custom XML:");
            Console.WriteLine(customXml);

            Assert.That(customXml, Is.EqualTo(defaultXml));

            var customInstance = customSerializer.DeserializeObject(customXml);
            var defaultInstance = defaultSerializer.DeserializeObject(defaultXml);

            Assert.That(customInstance, Has.PropertiesEqualTo(defaultInstance));
            Assert.That(customInstance, Has.PropertiesEqualTo(instance));

            AdditionalAssertions(instance, type, customXml, defaultXml, customInstance, defaultInstance);
        }

        protected virtual void AdditionalAssertions(object instance, Type type, string customXml, string defaultXml, object customInstance, object defaultInstance)
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