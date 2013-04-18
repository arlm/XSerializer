using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class ArrayTests
    {
        [Test]
        public void CanSerializeArrayAsProperty()
        {
            var container = new ContainerWithArrayProperty
            {
                Information = new[]
                {
                    new DataPoint
                    {
                        Name = "FooBar",
                        Preference = new Preference
                        {
                            Id = 123
                        }
                    }
                }
            };

            var serializer = new XmlSerializer<ContainerWithArrayProperty>(options => options.Indent(), typeof(Preference));

            var xml = serializer.Serialize(container);

            Assert.That(xml, Contains.Substring(@"xsi:type=""Preference"""));
            Assert.IsTrue(xml.IndexOf(@"xsi:type=""Preference""") == xml.LastIndexOf(@"xsi:type=""Preference"""));
        }

        [Test]
        public void CanDeserializeArrayAsProperty()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ContainerWithArrayProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Information>
    <DataPoint>
      <Name>FooBar</Name>
      <Preference xsi:type=""Preference"">
        <Id>123</Id>
      </Preference>
    </DataPoint>
  </Information>
</ContainerWithArrayProperty>";

            var serializer = new XmlSerializer<ContainerWithArrayProperty>(options => options.Indent(), typeof(Preference));

            var container = serializer.Deserialize(xml);

            Assert.That(container.Information.Length, Is.EqualTo(1));
        }

        [Test]
        public void CanSerializeArrayAsRoot()
        {
            var data = new[]
                {
                    new DataPoint
                    {
                        Name = "FooBar",
                        Preference = new Preference
                        {
                            Id = 123
                        }
                    }
                };

            var serializer = new XmlSerializer<DataPoint[]>(options => options.Indent(), typeof(Preference));

            var xml = serializer.Serialize(data);

            Assert.That(xml, Contains.Substring("</ArrayOfDataPoint>"));
            Assert.That(xml, Contains.Substring(@"xsi:type=""Preference"""));
            Assert.IsTrue(xml.IndexOf(@"xsi:type=""Preference""") == xml.LastIndexOf(@"xsi:type=""Preference"""));
        }

        [Test]
        public void CanDeserializeArrayAsRoot()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfDataPoint xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <DataPoint>
    <Name>FooBar</Name>
    <Preference xsi:type=""Preference"">
      <Id>123</Id>
    </Preference>
  </DataPoint>
</ArrayOfDataPoint>";

            var serializer = new XmlSerializer<DataPoint[]>(options => options.Indent(), typeof(Preference));

            var data = serializer.Deserialize(xml);

            Assert.That(data.Length, Is.EqualTo(1));
        }

        public class ContainerWithArrayProperty
        {
            public DataPoint[] Information { get; set; }
        }

        public class DataPoint
        {
            public string Name { get; set; }
            public IPreference Preference { get; set; }
        }

        public interface IPreference
        {
            int Id { get; set; }
        }

        public class Preference : IPreference
        {
            public int Id { get; set; }
        }
    }
}