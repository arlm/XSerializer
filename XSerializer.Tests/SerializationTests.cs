using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class SerializationTests
    {
        [TestCaseSource("NoInterfaceTestSource")]
        public void ForTypesWithoutInterfacesProxySerializerSerializesTheSameAsDefaultSerializer(
            object instance,
            string defaultNamespace,
            Type[] extraTypes,
            string rootElementName,
            Encoding encoding,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var defaultSerializer = 
                (IXmlSerializer)Activator.CreateInstance(
                    typeof(DefaultSerializer<>).MakeGenericType(instance.GetType()),
                    defaultNamespace,
                    extraTypes,
                    rootElementName);
            var interfaceSerializer = 
                (IXmlSerializer)Activator.CreateInstance(
                    typeof(InterfaceSerializer<>).MakeGenericType(instance.GetType()),
                    defaultNamespace,
                    extraTypes,
                    rootElementName);

            var defaultXml = defaultSerializer.SerializeObject(instance, encoding, formatting, namespaces);
            var proxyXml = interfaceSerializer.SerializeObject(instance, encoding, formatting, namespaces);

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Proxy XML:");
            Console.WriteLine(proxyXml);

            Assert.That(proxyXml, Is.EqualTo(defaultXml));
        }

        public TestCaseData[] NoInterfaceTestSource = new[]
        {
            new TestCaseData(new FooWithAbstract { Bar = new Barnicle { IsAttached = true, Chantey = "yohoho!"} }, null, new[] { typeof(Barnicle) }, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces()),
            new TestCaseData(new Food { Barnicle = new Barnicle { IsAttached = true, Chantey = "yohoho!"} }, null, null, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces()),
            new TestCaseData(new Food { Barnicle = new Barnicle { IsAttached = false, Chantey = null} }, null, null, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces()),
            new TestCaseData(new Fool { Barn = new Barn { CowCount = 3, Color = "Red" } }, null, null, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces()),
            new TestCaseData(new Foot { Bark = new Bark { Bite = 123 } }, null, null, null, Encoding.UTF8, Formatting.None, new XmlSerializerNamespaces()),
            new TestCaseData(new Foot { Bark = new Bark { Bite = 123 } }, null, null, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces()),
            new TestCaseData(new Foot { Bark = new Bark { Bite = 123 } }, "w00t", null, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces()),
            new TestCaseData(new IntClass { Value = 123 }, null, null, null, Encoding.UTF8, Formatting.None, new XmlSerializerNamespaces())
        };

        [TestCaseSource("InterfaceVersusAbstractTestSource")]
        public void TypeWithInterfaceSerializesSameAsTypeWithAbstract(
            object instanceWithInterface,
            object instanceWithAbstract,
            string defaultNamespace,
            Type[] extraTypes,
            string rootElementName,
            Encoding encoding,
            Formatting formatting,
            XmlSerializerNamespaces namespaces,
            IEnumerable<Tuple<string, string>> defaultXmlReplacements)
        {
            var defaultSerializer =
                (IXmlSerializer)Activator.CreateInstance(
                    typeof(DefaultSerializer<>).MakeGenericType(instanceWithAbstract.GetType()),
                    defaultNamespace,
                    extraTypes,
                    rootElementName);
            var interfaceSerializer =
                (IXmlSerializer)Activator.CreateInstance(
                    typeof(InterfaceSerializer<>).MakeGenericType(instanceWithInterface.GetType()),
                    defaultNamespace,
                    extraTypes,
                    rootElementName);

            var defaultXml = defaultSerializer.SerializeObject(instanceWithAbstract, encoding, formatting, namespaces);
            var proxyXml = interfaceSerializer.SerializeObject(instanceWithInterface, encoding, formatting, namespaces);

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Proxy XML:");
            Console.WriteLine(proxyXml);

            if (defaultXmlReplacements != null)
            {
                defaultXml = defaultXmlReplacements.Aggregate(defaultXml, (current, replacement) => current.Replace(replacement.Item1, replacement.Item2));
            }

            Assert.That(proxyXml, Is.EqualTo(defaultXml));
        }

        public TestCaseData[] InterfaceVersusAbstractTestSource = new[]
        {
            new TestCaseData(
                new ContainerWithInterface
                {
                    Id = "A",
                    One = new OneWithInterface
                    {
                        Id = "B",
                        Two = new TwoWithInterface
                        {
                            Id = "C",
                            Value = "ABC"
                        }
                    }
                },
                new ContainerWithAbstract
                {
                    Id = "A",
                    One = new OneWithAbstract
                    {
                        Id = "B",
                        Two = new TwoWithAbstract
                        {
                            Id = "C",
                            Value = "ABC"
                        }
                    }
                }, null, new[] { typeof(Barnicle) }, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces(), new[] { Tuple.Create("OneWithAbstract", "OneWithInterface"), Tuple.Create("TwoWithAbstract", "TwoWithInterface") }),
            new TestCaseData(new FooWithInterface { Bar = new Barnicle { IsAttached = true, Chantey = "yohoho!"} }, new FooWithAbstract { Bar = new Barnicle { IsAttached = true, Chantey = "yohoho!"} }, null, new[] { typeof(Barnicle) }, "FOO", Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces(), null),
            new TestCaseData(new FooWithInterface { Bar = new Barnicle { IsAttached = true, Chantey = "yohoho!"} }, new FooWithAbstract { Bar = new Barnicle { IsAttached = true, Chantey = "yohoho!"} }, null, new[] { typeof(Barnicle) }, null, Encoding.UTF8, Formatting.Indented, new XmlSerializerNamespaces(), null)
        };
    }
}
