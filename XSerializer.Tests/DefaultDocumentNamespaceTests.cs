using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DefaultDocumentNamespaceTests
    {
        [Test]
        public void NotSpecifyingNamespacesYieldsXsdAndXsi()
        {
            var serializer = new XmlSerializer<Foo>();

            var foo = new Foo { Bar = "abc" };

            var xml = serializer.Serialize(foo);

            var doc = XDocument.Parse(xml);

            var attributes = doc.Root.Attributes().ToList();

            Assert.That(attributes.Count, Is.EqualTo(2));

            var attribute = attributes.FirstOrDefault(x =>
                x.Name.NamespaceName == "http://www.w3.org/2000/xmlns/"
                && x.Name.LocalName == "xsd");

            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.Value, Is.EqualTo("http://www.w3.org/2001/XMLSchema"));

            attribute = attributes.FirstOrDefault(x =>
                x.Name.NamespaceName == "http://www.w3.org/2000/xmlns/"
                && x.Name.LocalName == "xsi");

            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.Value, Is.EqualTo("http://www.w3.org/2001/XMLSchema-instance"));
        }

        [Test]
        public void SpecifyingEmptyValuesYieldsNoDocumentAttributes()
        {
            var serializer = new XmlSerializer<Foo>(
                options => options.AddNamespace("", ""));

            var foo = new Foo { Bar = "abc" };

            var xml = serializer.Serialize(foo);

            var doc = XDocument.Parse(xml);

            var attributes = doc.Root.Attributes().ToList();

            Assert.That(attributes, Is.Empty);
        }

        [Test]
        public void SpecifyingNonEmptyValuesYieldsTheSpecifiedValuesButNotXsdOrXsi()
        {
            var serializer = new XmlSerializer<Foo>(
                options => options.AddNamespace("foo", "bar").AddNamespace("baz", "qux"));

            var foo = new Foo { Bar = "abc" };

            var xml = serializer.Serialize(foo);

            var doc = XDocument.Parse(xml);

            var attributes = doc.Root.Attributes().ToList();

            Assert.That(attributes.Count, Is.EqualTo(2));

            var attribute = attributes.FirstOrDefault(x =>
                x.Name.NamespaceName == "http://www.w3.org/2000/xmlns/"
                && x.Name.LocalName == "foo");

            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.Value, Is.EqualTo("bar"));

            attribute = attributes.FirstOrDefault(x =>
                x.Name.NamespaceName == "http://www.w3.org/2000/xmlns/"
                && x.Name.LocalName == "baz");

            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.Value, Is.EqualTo("qux"));
        }

        public class Foo
        {
            public string Bar { get; set; }
        }
    }
}
