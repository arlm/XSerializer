using System;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class RoundTripTests
    {
        [TestCaseSource("SomeTests")]
        public void XmlToObjectToXmlToObject(string xml, Type type)
        {
            var serializer = XmlSerializerFactory.Instance.GetSerializer(type, TestOptions.Empty);
            var instance = serializer.DeserializeObject(xml);
            var roundTripXml = serializer.SerializeObject(instance, null, Encoding.UTF8, Formatting.Indented, false);
            var roundTripInstance = serializer.DeserializeObject(roundTripXml);
            AssertAreEqual(instance, roundTripInstance);
        }

        private static void AssertAreEqual(object instance, object otherInstance)
        {
            Assert.That(instance.GetType(), Is.EqualTo(otherInstance.GetType()));

            foreach (var property in instance.GetType().GetProperties().Where(p => p.IsSerializable()))
            {
                var instancePropertyValue = property.GetValue(instance, null);
                var otherInstancePropertyValue = property.GetValue(otherInstance, null);

                if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                {
                    Assert.That(instancePropertyValue, Is.EqualTo(otherInstancePropertyValue));
                }
                else
                {
                    AssertAreEqual(instancePropertyValue, otherInstancePropertyValue);
                }
            }
        }

        public TestCaseData[] SomeTests = new[]
        {
            new TestCaseData(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Container xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Id>A</Id>
  <One xsi:type=""OneWithInterface"">
    <Id>B</Id>
    <Two xsi:type=""TwoWithInterface"">
      <Id>C</Id>
      <Value>ABC</Value>
    </Two>
  </One>
</Container>", typeof(ContainerWithInterface)),
             new TestCaseData(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Container xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <One xsi:type=""OneWithInterface"">
    <Two xsi:type=""TwoWithInterface"">
      <Value>ABC</Value>
      <Id>C</Id>
    </Two>
    <Id>B</Id>
  </One>
  <Id>A</Id>
</Container>", typeof(ContainerWithInterface)),
            new TestCaseData(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Container xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Id>A</Id>
  <One xsi:type=""OneWithAbstract"">
    <Id>B</Id>
    <Two xsi:type=""TwoWithAbstract"">
      <Id>C</Id>
      <Value>ABC</Value>
    </Two>
  </One>
</Container>", typeof(ContainerWithAbstract)),
             new TestCaseData(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Container xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <One xsi:type=""OneWithAbstract"">
    <Two xsi:type=""TwoWithAbstract"">
      <Value>ABC</Value>
      <Id>C</Id>
    </Two>
    <Id>B</Id>
  </One>
  <Id>A</Id>
</Container>", typeof(ContainerWithAbstract)),
             new TestCaseData(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Foo xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Bar xsi:type=""Barnicle"" IsAttached=""true"">yohoho!</Bar>
</Foo>", typeof(FooWithInterface)),
        };
    }
}