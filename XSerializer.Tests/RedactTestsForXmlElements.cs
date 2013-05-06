using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class RedactTestsForXmlElements
    {
        [Test]
        public void StringElementIsRedacted()
        {
            var instance = new RedactTestClassForXmlElement { StringProperty = "abc123" };
            var serializer = new XmlSerializer<RedactTestClassForXmlElement>();

            var xml = serializer.Serialize(instance);

            Assert.That(xml, Contains.Substring("<StringProperty>XXX111</StringProperty>"));
        }

        [Test]
        public void NullableDateTimeElementIsRedacted()
        {
            var instance = new RedactTestClassForXmlElement { NullableDateTimeProperty = DateTime.UtcNow };
            var serializer = new XmlSerializer<RedactTestClassForXmlElement>();

            var xml = serializer.Serialize(instance);

            Assert.That(xml, Contains.Substring("<NullableDateTimeProperty>1111-11-11T11:11:11.1111111Z</NullableDateTimeProperty>"));
        }

        [Test]
        public void DictionaryElementIsRedacted()
        {
            var instance = new RedactTestClassForXmlElement
            {
                Map = new Dictionary<int, string>
                {
                    { 1, "1abc" },
                    { 2, "d2ef" },
                    { 3, "gh3i" },
                    { 4, "jkl4" },
                }
            };
            var serializer = new XmlSerializer<RedactTestClassForXmlElement>();

            var xml = serializer.Serialize(instance);

            Assert.That(xml, Contains.Substring("<Key>1</Key>"));
            Assert.That(xml, Contains.Substring("<Key>2</Key>"));
            Assert.That(xml, Contains.Substring("<Key>3</Key>"));
            Assert.That(xml, Contains.Substring("<Key>4</Key>"));
            Assert.That(xml, Contains.Substring("<Value>1XXX</Value>"));
            Assert.That(xml, Contains.Substring("<Value>X1XX</Value>"));
            Assert.That(xml, Contains.Substring("<Value>XX1X</Value>"));
            Assert.That(xml, Contains.Substring("<Value>XXX1</Value>"));
        }

        [Test]
        public void ListElementIsRedacted()
        {
            var instance = new RedactTestClassForXmlElement
            {
                List = new List<double> { 1.2, 34.56, 789.012 }
            };
            var serializer = new XmlSerializer<RedactTestClassForXmlElement>();

            var xml = serializer.Serialize(instance);

            Assert.That(xml, Contains.Substring("<Double>111</Double>"));
            Assert.That(xml, Contains.Substring("<Double>11111</Double>"));
            Assert.That(xml, Contains.Substring("<Double>1111111</Double>"));
        }

        [Test]
        public void DynamicElementIsRedacted()
        {
            var instance = new RedactTestClassForXmlElement
            {
                Dynamic = new { Foo = 123, Bar = DateTime.UtcNow, Baz = new { Quirble = -123.456 } }
            };
            var serializer = new XmlSerializer<RedactTestClassForXmlElement>();

            var xml = serializer.Serialize(instance);

            Assert.That(xml, Contains.Substring("<Foo>111</Foo>"));
            Assert.That(xml, Contains.Substring("<Bar>1111-11-11T11:11:11.1111111Z</Bar>"));
            Assert.That(xml, Contains.Substring("<Quirble>11111111</Quirble>"));
        }

        public class RedactTestClassForXmlElement
        {
            [Redact]
            public string StringProperty { get; set; }

            [Redact]
            public DateTime? NullableDateTimeProperty { get; set; }

            [Redact]
            public Dictionary<int, string> Map { get; set; }

            [Redact]
            public List<double> List { get; set; }

            [Redact]
            public dynamic Dynamic { get; set; }
        }
    }
}