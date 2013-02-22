namespace XSerializer.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    using NUnit.Framework;

    public class PerformanceTests
    {
        private ContainerWithAbstract _containerWithAbstract;
        private ContainerWithInterface _containerWithInterface;

        [SetUp]
        public void Setup()
        {
            new XmlSerializer(typeof(JitPreparation), null, null, null, null);
            CustomSerializer.GetSerializer(typeof(JitPreparation), null, null, null);
            _containerWithAbstract =
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
            _containerWithInterface =
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
        }

        [Test]
        public void CreateSingleSerializerBenchmark()
        {
            var xmlSerializerStopwatch = Stopwatch.StartNew();
            new XmlSerializer(typeof(ContainerWithAbstract), null, null, null, null);
            xmlSerializerStopwatch.Stop();

            var customSerializerStopwatch = Stopwatch.StartNew();
            CustomSerializer.GetSerializer(typeof(ContainerWithInterface), null, null, null);
            customSerializerStopwatch.Stop();

            Console.WriteLine("XmlSerializer Elapsed Time: {0}", xmlSerializerStopwatch.Elapsed);
            Console.WriteLine("CustomSerializder Elapsed Time: {0}", customSerializerStopwatch.Elapsed);
        }

        [Test]
        public void CreateManySerializersBenchmark()
        {
            const int Iterations = 1000;

            var xmlSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                new XmlSerializer(typeof(ContainerWithAbstract), null, null, null, null);
            }

            xmlSerializerStopwatch.Stop();

            var customSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                CustomSerializer.GetSerializer(typeof(ContainerWithInterface), null, null, null);
            }
            
            customSerializerStopwatch.Stop();

            Console.WriteLine("XmlSerializer Elapsed Time: {0}", xmlSerializerStopwatch.Elapsed);
            Console.WriteLine("CustomSerializder Elapsed Time: {0}", customSerializerStopwatch.Elapsed);
        }

        [Test]
        public void SerializationBenchmark()
        {
            const int Iterations = 100000;

            var xmlSerializer = new XmlSerializer(typeof(ContainerWithAbstract), null, null, null, null);
            var customSerializer = CustomSerializer.GetSerializer(typeof(ContainerWithInterface), null, null, null);

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

            var customSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                var sb = new StringBuilder();
                using (var stringWriter = new StringWriter(sb))
                {
                    using (var writer = new SerializationXmlTextWriter(stringWriter))
                    {
                        customSerializer.SerializeObject(_containerWithInterface, writer, null);
                    }
                }
            }

            customSerializerStopwatch.Stop();

            Console.WriteLine("XmlSerializer Elapsed Time: {0}", xmlSerializerStopwatch.Elapsed);
            Console.WriteLine("CustomSerializder Elapsed Time: {0}", customSerializerStopwatch.Elapsed);
        }

        public class JitPreparation
        {
        }
    }
}