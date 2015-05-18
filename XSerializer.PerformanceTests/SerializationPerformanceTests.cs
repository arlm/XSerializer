using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests.Performance
{
    public class SerializationPerformanceTests
    {
        private static ContainerWithAbstract _containerWithAbstract =
            new ContainerWithAbstract
                {
                    Id = "a",
                    One =
                        new OneWithAbstract
                        {
                            Id = "b",
                            Two = new TwoWithAbstract { Id = "c", Value = "abc" }
                        }
                };

        private static ContainerWithInterface _containerWithInterface =
                new ContainerWithInterface
                {
                    Id = "a",
                    One =
                        new OneWithInterface
                        {
                            Id = "b",
                            Two = new TwoWithInterface { Id = "c", Value = "abc" }
                        }
                };

        [Test]
        public void Benchmark()
        {
            const int Iterations = 100000;

            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(ContainerWithAbstract), null, null, null, null);
            var customSerializer = CustomSerializer.GetSerializer(typeof(ContainerWithInterface), null, TestXmlSerializerOptions.Empty);

            var xmlSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                var sb = new StringBuilder();
                using (var stringWriter = new StringWriter(sb))
                {
                    using (var writer = new XmlTextWriter(stringWriter))
                    {
                        xmlSerializer.Serialize(writer, _containerWithAbstract, null);
                    }
                }
            }

            xmlSerializerStopwatch.Stop();

            ISerializeOptions options = new TestSerializeOptions();

            var customSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                var sb = new StringBuilder();
                using (var stringWriter = new StringWriter(sb))
                {
                    using (var writer = new XSerializerXmlTextWriter(stringWriter, options))
                    {
                        customSerializer.SerializeObject(writer, _containerWithInterface, new TestSerializeOptions());
                    }
                }
            }

            customSerializerStopwatch.Stop();

            Console.WriteLine("XmlSerializer Elapsed Time: {0}", xmlSerializerStopwatch.Elapsed);
            Console.WriteLine("CustomSerializer Elapsed Time: {0}", customSerializerStopwatch.Elapsed);
        } 
    }
}