using NUnit.Framework;

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
}