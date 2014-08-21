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
        public void ForTypesWithoutInterfacesCustomSerializerSerializesTheSameAsDefaultSerializer(
            object instance,
            string defaultNamespace,
            Type[] extraTypes,
            string rootElementName,
            Encoding encoding,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var defaultSerializer = 
                new System.Xml.Serialization.XmlSerializer(
                    instance.GetType(),
                    null,
                    extraTypes,
                    string.IsNullOrWhiteSpace(rootElementName) ? null : new XmlRootAttribute(rootElementName),
                    defaultNamespace);
            var customSerializer = 
                (IXmlSerializerInternal)Activator.CreateInstance(
                    typeof(CustomSerializer<>).MakeGenericType(instance.GetType()),
                    new TestXmlSerializerOptions
                    {
                        DefaultNamespace = defaultNamespace,
                        ExtraTypes = extraTypes,
                        RootElementName = rootElementName
                    });

            var defaultXml = defaultSerializer.SerializeObject(instance, encoding, formatting, new TestSerializeOptions(namespaces)).StripXsiXsdDeclarations();
            var customXml = customSerializer.SerializeObject(instance, encoding, formatting, new TestSerializeOptions(namespaces)).StripXsiXsdDeclarations();

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Custom XML:");
            Console.WriteLine(customXml);

            Assert.That(customXml, Is.EqualTo(defaultXml));
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
                new System.Xml.Serialization.XmlSerializer(
                    instanceWithAbstract.GetType(),
                    null,
                    extraTypes,
                    string.IsNullOrWhiteSpace(rootElementName) ? null : new XmlRootAttribute(rootElementName),
                    defaultNamespace);
            var customSerializer =
                (IXmlSerializerInternal)Activator.CreateInstance(
                    typeof(CustomSerializer<>).MakeGenericType(instanceWithInterface.GetType()),
                    new TestXmlSerializerOptions
                    {
                        DefaultNamespace = defaultNamespace,
                        ExtraTypes = extraTypes,
                        RootElementName = rootElementName
                    });

            var defaultXml = defaultSerializer.SerializeObject(instanceWithAbstract, encoding, formatting, new TestSerializeOptions(namespaces)).StripXsiXsdDeclarations();
            var customXml = customSerializer.SerializeObject(instanceWithInterface, encoding, formatting, new TestSerializeOptions(namespaces)).StripXsiXsdDeclarations();

            Console.WriteLine("Default XML:");
            Console.WriteLine(defaultXml);
            Console.WriteLine();
            Console.WriteLine("Custom XML:");
            Console.WriteLine(customXml);

            if (defaultXmlReplacements != null)
            {
                defaultXml = defaultXmlReplacements.Aggregate(defaultXml, (current, replacement) => current.Replace(replacement.Item1, replacement.Item2));
            }

            Assert.That(customXml, Is.EqualTo(defaultXml));
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
