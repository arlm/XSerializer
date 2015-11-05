using Newtonsoft.Json;
#if !DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using XSerializer.Encryption;

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
            var customSerializer = CustomSerializer.GetSerializer(typeof(ContainerWithInterface), null, TestXmlSerializerOptions.Empty);

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

            var options = new TestSerializeOptions();

            var customSerializerStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                using (var stringReader = new StringReader(_xmlWithInterface))
                {
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        using (var reader = new XSerializerXmlReader(xmlReader, options.GetEncryptionMechanism(), options.EncryptKey, options.SerializationState))
                        {
                            customSerializer.DeserializeObject(reader, options);
                        }
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
            var json = @"{""Corge"":""abc"",""Grault"":123.45,""Garply"":true,""Bar"":{""Waldo"":""2015-09-16T11:43:50.8355302-04:00"",""Fred"":""862663f1-3dd1-46c2-97d5-f9034b784854""},""Bazes"":[{""Wibble"":2147483647,""Wobble"":9223372036854775807,""Wubble"":123.45},{""Wibble"":0,""Wobble"":0,""Wubble"":0.5},{""Wibble"":-2147483648,""Wobble"":-9223372036854775808,""Wubble"":-123.45}]}";

            var newtonsoftJsonSerializer = new Newtonsoft.Json.JsonSerializer();
            var xSerializerJsonSerializer = new JsonSerializer<Foo>();

            var newtonsoftResult = NewtonsoftJsonDeserializeFoo(newtonsoftJsonSerializer, json);
            var xSerializerResult = XSerializerJsonDeserialize(xSerializerJsonSerializer, json);

            var newtonsoftRoundTrip = NewtonsoftJsonSerialize(newtonsoftJsonSerializer, newtonsoftResult);
            var xSerializerRoundTrip = XSerializerJsonSerialize(xSerializerJsonSerializer, xSerializerResult);

            Assert.That(xSerializerRoundTrip, Is.EqualTo(newtonsoftRoundTrip));

            const int iterations = 500000;

            var newtonsoftStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                NewtonsoftJsonDeserializeFoo(newtonsoftJsonSerializer, json);
            }
            newtonsoftStopwatch.Stop();

            var xSerializerStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                XSerializerJsonDeserialize(xSerializerJsonSerializer, json);
            }
            xSerializerStopwatch.Stop();

            Console.WriteLine("Deserialization");
            Console.WriteLine("Newtonsoft Elapsed Time: {0}", newtonsoftStopwatch.Elapsed);
            Console.WriteLine("XSerializer Elapsed Time: {0}", xSerializerStopwatch.Elapsed);
        }

        [Test]
        public void BenchmarkNonDefaultConstructorJson()
        {
            var json = @"{""Corge"":""abc"",""Grault"":123.45,""Garply"":true,""Bar"":{""Waldo"":""2015-09-16T11:43:50.8355302-04:00"",""Fred"":""862663f1-3dd1-46c2-97d5-f9034b784854""},""Bazes"":[{""Wibble"":2147483647,""Wobble"":9223372036854775807,""Wubble"":123.45},{""Wibble"":0,""Wobble"":0,""Wubble"":0.5},{""Wibble"":-2147483648,""Wobble"":-9223372036854775808,""Wubble"":-123.45}]}";

            var newtonsoftJsonSerializer = new Newtonsoft.Json.JsonSerializer();
            var xSerializerJsonSerializer = new JsonSerializer<Foo2>();

            var newtonsoftResult = NewtonsoftJsonDeserializeFoo2(newtonsoftJsonSerializer, json);
            var xSerializerResult = XSerializerJsonDeserialize(xSerializerJsonSerializer, json);

            var newtonsoftRoundTrip = NewtonsoftJsonSerialize(newtonsoftJsonSerializer, newtonsoftResult);
            var xSerializerRoundTrip = XSerializerJsonSerialize(xSerializerJsonSerializer, xSerializerResult);

            Assert.That(xSerializerRoundTrip, Is.EqualTo(newtonsoftRoundTrip));

            const int iterations = 500000;

            var newtonsoftStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                NewtonsoftJsonDeserializeFoo2(newtonsoftJsonSerializer, json);
            }
            newtonsoftStopwatch.Stop();

            var xSerializerStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                XSerializerJsonDeserialize(xSerializerJsonSerializer, json);
            }
            xSerializerStopwatch.Stop();

            Console.WriteLine("Deserialization");
            Console.WriteLine("Newtonsoft Elapsed Time: {0}", newtonsoftStopwatch.Elapsed);
            Console.WriteLine("XSerializer Elapsed Time: {0}", xSerializerStopwatch.Elapsed);
        }

        [Test]
        public void BenchmarkDynamicJsonDeserialization()
        {
            var json =
                @"{""Corge"":""abc"",""Grault"":123.45,""Garply"":true,""Bar"":{""Waldo"":""2015-09-16T11:43:50.8355302-04:00"",""Fred"":""862663f1-3dd1-46c2-97d5-f9034b784854""},""Bazes"":[{""Wibble"":2147483647,""Wobble"":9223372036854775807,""Wubble"":123.45},{""Wibble"":0,""Wobble"":0,""Wubble"":0.5},{""Wibble"":-2147483648,""Wobble"":-9223372036854775808,""Wubble"":-123.45}]}";

            var newtonsoftJsonSerializer = new Newtonsoft.Json.JsonSerializer();
            var xSerializerJsonSerializer = new JsonSerializer<dynamic>();

            var newtonsoftResult = NewtonsoftJsonDeserializeDynamic(newtonsoftJsonSerializer, json);
            var xSerializerResult = XSerializerJsonDeserialize(xSerializerJsonSerializer, json);

            AssertEquivalent(newtonsoftResult, xSerializerResult);

            const int iterations = 500000;

            var newtonsoftStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                NewtonsoftJsonDeserializeDynamic(newtonsoftJsonSerializer, json);
            }
            newtonsoftStopwatch.Stop();

            var xSerializerStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                XSerializerJsonDeserialize(xSerializerJsonSerializer, json);
            }
            xSerializerStopwatch.Stop();

            Console.WriteLine("Deserialization");
            Console.WriteLine("Newtonsoft Elapsed Time: {0}", newtonsoftStopwatch.Elapsed);
            Console.WriteLine("XSerializer Elapsed Time: {0}", xSerializerStopwatch.Elapsed);
        }

        private static void AssertEquivalent(dynamic newtonsoftResult, dynamic xSerializerResult)
        {
            string corgeN = newtonsoftResult.Corge;
            string corgeX = xSerializerResult.Corge;
            Assert.That(corgeX, Is.EqualTo(corgeN));

            double graultN = newtonsoftResult.Grault;
            double graultX = xSerializerResult.Grault;
            Assert.That(graultX, Is.EqualTo(graultN));

            bool garplyN = newtonsoftResult.Garply;
            bool garplyX = xSerializerResult.Garply;
            Assert.That(garplyX, Is.EqualTo(garplyN));

            var barN = newtonsoftResult.Bar;
            var barX = xSerializerResult.Bar;

            DateTime waldoN = barN.Waldo;
            DateTime waldoX = barX.WaldoAsDateTime;
            Assert.That(waldoX, Is.EqualTo(waldoN));

            string fredN = barN.Fred;
            string fredX = barX.Fred;
            Assert.That(fredX, Is.EqualTo(fredN));

            var bazesN = newtonsoftResult.Bazes;
            var bazesX = xSerializerResult.Bazes;
            Assert.That(bazesX.Count, Is.EqualTo(bazesN.Count));

            var bazN = bazesN[0];
            var bazX = bazesX[0];

            int wibbleN = bazN.Wibble;
            int wibbleX = bazX.WibbleAsInt32;
            Assert.That(wibbleX, Is.EqualTo(wibbleN));

            long wobbleN = bazN.Wobble;
            long wobbleX = bazX.WobbleAsInt64;
            Assert.That(wobbleX, Is.EqualTo(wobbleN));

            decimal wubbleN = bazN.Wubble;
            decimal wubbleX = bazX.WubbleAsDecimal;
            Assert.That(wubbleX, Is.EqualTo(wubbleN));

            bazN = bazesN[1];
            bazX = bazesX[1];

            wibbleN = bazN.Wibble;
            wibbleX = bazX.WibbleAsInt32;
            Assert.That(wibbleX, Is.EqualTo(wibbleN));

            wobbleN = bazN.Wobble;
            wobbleX = bazX.WobbleAsInt64;
            Assert.That(wobbleX, Is.EqualTo(wobbleN));

            wubbleN = bazN.Wubble;
            wubbleX = bazX.WubbleAsDecimal;
            Assert.That(wubbleX, Is.EqualTo(wubbleN));

            bazN = bazesN[2];
            bazX = bazesX[2];

            wibbleN = bazN.Wibble;
            wibbleX = bazX.WibbleAsInt32;
            Assert.That(wibbleX, Is.EqualTo(wibbleN));

            wobbleN = bazN.Wobble;
            wobbleX = bazX.WobbleAsInt64;
            Assert.That(wobbleX, Is.EqualTo(wobbleN));

            wubbleN = bazN.Wubble;
            wubbleX = bazX.WubbleAsDecimal;
            Assert.That(wubbleX, Is.EqualTo(wubbleN));
        }

        private static Foo NewtonsoftJsonDeserializeFoo(Newtonsoft.Json.JsonSerializer jsonSerializer, string json)
        {
            using (var reader = new StringReader(json))
            {
                return (Foo)jsonSerializer.Deserialize(reader, typeof(Foo));
            }
        }

        private static Foo2 NewtonsoftJsonDeserializeFoo2(Newtonsoft.Json.JsonSerializer jsonSerializer, string json)
        {
            using (var reader = new StringReader(json))
            {
                return (Foo2)jsonSerializer.Deserialize(reader, typeof(Foo2));
            }
        }

        private static dynamic NewtonsoftJsonDeserializeDynamic(Newtonsoft.Json.JsonSerializer jsonSerializer, string json)
        {
            using (var reader = new StringReader(json))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return jsonSerializer.Deserialize(jsonReader);
                }
            }
        }

        private static Foo XSerializerJsonDeserialize(JsonSerializer<Foo> jsonSerializer, string json)
        {
            using (var reader = new StringReader(json))
            {
                return jsonSerializer.Deserialize(reader);
            }
        }

        private static dynamic XSerializerJsonDeserialize(JsonSerializer<dynamic> jsonSerializer, string json)
        {
            using (var reader = new StringReader(json))
            {
                return jsonSerializer.Deserialize(reader);
            }
        }

        private static Foo2 XSerializerJsonDeserialize(JsonSerializer<Foo2> jsonSerializer, string json)
        {
            using (var reader = new StringReader(json))
            {
                return jsonSerializer.Deserialize(reader);
            }
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

        private static string NewtonsoftJsonSerialize(Newtonsoft.Json.JsonSerializer jsonSerializer, Foo2 foo)
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

        private static string XSerializerJsonSerialize(JsonSerializer<Foo2> jsonSerializer, Foo2 foo)
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