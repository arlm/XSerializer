using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests.Performance
{
    public class DeserializationPerformanceTests
    {
        private const string _xmlWithAbstract = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Container xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Id>A</Id>
  <One xsi:type=""OneWithAbstract"">
    <Id>B</Id>
    <Two xsi:type=""TwoWithAbstract"">
      <Id>C</Id>
      <Value>ABC</Value>
    </Two>
  </One>
</Container>";

        private const string _xmlWithInterface = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Container xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Id>A</Id>
  <One xsi:type=""OneWithInterface"">
    <Id>B</Id>
    <Two xsi:type=""TwoWithInterface"">
      <Id>C</Id>
      <Value>ABC</Value>
    </Two>
  </One>
</Container>";

        [Test]
        public void Benchmark()
        {
            const int Iterations = 50000;

            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(ContainerWithAbstract), null, null, null, null);
            var customSerializer = CustomSerializer.GetSerializer(typeof(ContainerWithInterface), TestXmlSerializerOptions.Empty);

            var xmlSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                using (var stringReader = new StringReader(_xmlWithAbstract))
                {
                    using (var reader = new XmlTextReader(stringReader))
                    {
                        xmlSerializer.Deserialize(reader);
                    }
                }
            }

            xmlSerializerStopwatch.Stop();

            var customSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                using (var stringReader = new StringReader(_xmlWithInterface))
                {
                    using (var reader = new XmlTextReader(stringReader))
                    {
                        customSerializer.DeserializeObject(reader);
                    }
                }
            }

            customSerializerStopwatch.Stop();

            Console.WriteLine("XmlSerializer Elapsed Time: {0}", xmlSerializerStopwatch.Elapsed);
            Console.WriteLine("CustomSerializder Elapsed Time: {0}", customSerializerStopwatch.Elapsed);
        } 
    }
}