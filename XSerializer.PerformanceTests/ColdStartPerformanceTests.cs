using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests.Performance
{
    public class ColdStartPerformanceTests
    {
        [Test]
        public void Benchmark()
        {
            new System.Xml.Serialization.XmlSerializer(typeof(JitPreparation), null, null, null, null);
            CustomSerializer.GetSerializer(typeof(JitPreparation), null, TestXmlSerializerOptions.Empty);

            var containerWithAbstract =
                new ColdStartContainerWithAbstract
                {
                    Id = "a",
                    One =
                        new ColdStartOneWithAbstract
                        {
                            Id = "b",
                            Two = new ColdStartTwoWithAbstract { Id = "c", Value = "abc" }
                        }
                };
            var containerWithInterface =
                new ColdStartContainerWithInterface
                {
                    Id = "a",
                    One =
                        new ColdStartOneWithInterface
                        {
                            Id = "b",
                            Two = new ColdStartTwoWithInterface { Id = "c", Value = "abc" }
                        }
                };

            var xmlSerializerStopwatch = Stopwatch.StartNew();

            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(ColdStartContainerWithAbstract), null, null, null, null);
            var xmlSerializerStringBuilder = new StringBuilder();

            using (var stringWriter = new StringWriter(xmlSerializerStringBuilder))
            {
                using (var writer = new XmlTextWriter(stringWriter))
                {
                    xmlSerializer.Serialize(writer, containerWithAbstract, null);
                }
            }

            using (var stringReader = new StringReader(xmlSerializerStringBuilder.ToString()))
            {
                using (var reader = new XmlTextReader(stringReader))
                {
                    xmlSerializer.Deserialize(reader);
                }
            }

            xmlSerializerStopwatch.Stop();

            ISerializeOptions options = new TestSerializeOptions();

            var customSerializerStopwatch = Stopwatch.StartNew();

            var customSerializer = CustomSerializer.GetSerializer(typeof(ColdStartContainerWithInterface), null, TestXmlSerializerOptions.Empty);
            var customSerializerStringBuilder = new StringBuilder();

            using (var stringWriter = new StringWriter(customSerializerStringBuilder))
            {
                using (var writer = new XSerializerXmlTextWriter(stringWriter, options))
                {
                    customSerializer.SerializeObject(writer, containerWithInterface, new TestSerializeOptions(true));
                }
            }

            using (var stringReader = new StringReader(customSerializerStringBuilder.ToString()))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    using (var reader = new XSerializerXmlReader(xmlReader, options.GetEncryptionMechanism(), options.EncryptKey, options.SerializationState))
                    {
                        customSerializer.DeserializeObject(reader, options);
                    }
                }
            }

            customSerializerStopwatch.Stop();

            Console.WriteLine("XmlSerializer Elapsed Time: {0}", xmlSerializerStopwatch.Elapsed);
            Console.WriteLine("CustomSerializer Elapsed Time: {0}", customSerializerStopwatch.Elapsed);
        }

        [XmlRoot("Container")]
        public class ColdStartContainerWithInterface
        {
            public string Id { get; set; }
            public IColdStartOne One { get; set; }
        }

        [XmlRoot("Container")]
        public class ColdStartContainerWithAbstract
        {
            public string Id { get; set; }
            public ColdStartOneBase One { get; set; }
        }

        [XmlInclude(typeof(ColdStartOneWithInterface))]
        public interface IColdStartOne
        {
            string Id { get; set; }
            IColdStartTwo Two { get; set; }
        }

        [XmlInclude(typeof(ColdStartOneWithAbstract))]
        public abstract class ColdStartOneBase
        {
            public string Id { get; set; }
            public abstract ColdStartTwoBase Two { get; set; }
        }

        public class ColdStartOneWithInterface : IColdStartOne
        {
            public string Id { get; set; }
            public IColdStartTwo Two { get; set; }
        }

        public class ColdStartOneWithAbstract : ColdStartOneBase
        {
            public override ColdStartTwoBase Two { get; set; }
        }

        [XmlInclude(typeof(ColdStartTwoWithInterface))]
        public interface IColdStartTwo
        {
            string Id { get; set; }
            string Value { get; set; }
        }

        [XmlInclude(typeof(ColdStartTwoWithAbstract))]
        public abstract class ColdStartTwoBase
        {
            public string Id { get; set; }
            public abstract string Value { get; set; }
        }

        public class ColdStartTwoWithInterface : IColdStartTwo
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }

        public class ColdStartTwoWithAbstract : ColdStartTwoBase
        {
            public override string Value { get; set; }
        }
    }
}