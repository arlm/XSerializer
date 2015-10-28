#if !DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
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

        [Test]
        public void BenchmarkJson()
        {
            var dateTime = DateTime.Parse("2015-09-16T11:43:50.8355302-04:00");
            var guid = Guid.Parse("862663f1-3dd1-46c2-97d5-f9034b784854");

            var foo = new Foo
            {
                Corge = "abc",
                Grault = 123.45,
                Garply = true,
                Bar = new Bar
                {
                    Waldo = dateTime,
                    Fred = guid
                },
                Bazes = new List<Baz>
                {
                    new Baz
                    {
                        Wibble = 2147483647,
                        Wobble = 9223372036854775807,
                        Wubble = 123.45M
                    },
                    new Baz
                    {
                        Wibble = 0,
                        Wobble = 0,
                        Wubble = 0.5M
                    },
                    new Baz
                    {
                        Wibble = -2147483648,
                        Wobble = -9223372036854775808,
                        Wubble = -123.45M
                    }
                }
            };

            var newtonsoftJsonSerializer = new Newtonsoft.Json.JsonSerializer();
            var xSerializerJsonSerializer = new JsonSerializer<Foo>();

            var newtonsoftJson = NewtonsoftJsonSerialize(newtonsoftJsonSerializer, foo);
            var xSerializerJson = XSerializerJsonSerialize(xSerializerJsonSerializer, foo);

            Assert.That(xSerializerJson, Is.EqualTo(newtonsoftJson));

            const int iterations = 1000000;

            var newtonsoftStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                NewtonsoftJsonSerialize(newtonsoftJsonSerializer, foo);
            }
            newtonsoftStopwatch.Stop();

            var xSerializerStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                XSerializerJsonSerialize(xSerializerJsonSerializer, foo);
            }
            xSerializerStopwatch.Stop();

            Console.WriteLine("Serialization");
            Console.WriteLine("Newtonsoft Elapsed Time: {0}", newtonsoftStopwatch.Elapsed);
            Console.WriteLine("XSerializer Elapsed Time: {0}", xSerializerStopwatch.Elapsed);
        }

        private static string NewtonsoftJsonSerialize(Newtonsoft.Json.JsonSerializer jsonSerializer, Foo foo)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                jsonSerializer.Serialize(writer, foo);
            }
            return sb.ToString();
        }

        private static string XSerializerJsonSerialize(JsonSerializer<Foo> jsonSerializer, Foo foo)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                jsonSerializer.Serialize(writer, foo);
            }
            return sb.ToString();
        }
    }
}
#endif