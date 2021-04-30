using NUnit.Framework;
using System.Xml.Serialization;

namespace XSerializer.Tests
{
    public class XmlBugTests
    {
        [Test]
        public void AnUnknownElementWithTheSameNameAsAKnownElementDoesNotClearOutTheKnownValue()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Data>
    <Person>
        <Name>
            <First>Bilbo</First>
            <Last>Baggins</Last>
        </Name>
        <Pet>
            <Type>Cat</Type>
            <Name>Fluffy</Name>
        </Pet>
    </Person>
</Data>";

            var serializer = new XmlSerializer<Data>();

            var data = serializer.Deserialize(xml);

            Assert.That(data.Person.Name.First, Is.EqualTo("Bilbo"));
            Assert.That(data.Person.Name.Last, Is.EqualTo("Baggins"));
        }

        [Test]
        public void CDATAIsSupportedForXmlText()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ErrorEvent>
    <Message><![CDATA[<Oh, no!>]]></Message>
</ErrorEvent>";

            var serializer = new XmlSerializer<ErrorEvent>();

            var errorEvent = serializer.Deserialize(xml);

            Assert.That(errorEvent.Message.Value, Is.EqualTo("<Oh, no!>"));
        }
    }

    public class Data
    {
        public Person Person { get; set; }
    }

    public class Person
    {
        public Name Name { get; set; }
    }

    public class Name
    {
        public string First { get; set; }
        public string Last { get; set; }
    }

    public class ErrorEvent
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        [XmlText]
        public string Value { get; set; }
    }
}