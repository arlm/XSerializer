using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class NilTests
    {
        [Test]
        public void VerifyThatNilIsEmittedForEachPropertyType()
        {
            var serializer = new XmlSerializer<Everything>(x => x.EmitNil().Indent());

            var everything = new Everything();

            var xml = serializer.Serialize(everything);

            Assert.That(Regex.Matches(xml, @"xsi:nil=""true""").Count, Is.EqualTo(6));
        }

        [Test]
        public void VerifyThatAllPropertyTypesWithNilValuesCanBeDeserializedIntoNull()
        {
            var serializer = new XmlSerializer<Everything>();

            var xml = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Everything xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <IsAwesome xsi:nil=""true"" />
  <Special xsi:nil=""true"" />
  <Ordinary xsi:nil=""true"" />
  <Insignificant xsi:nil=""true"" />
  <Forgettable xsi:nil=""true"" />
  <Inconsequential xsi:nil=""true"" />
</Everything>";

            var everything = serializer.Deserialize(xml);

            Assert.That(everything, Is.Not.Null);

            Assert.That(everything.IsAwesome, Is.Null);
            Assert.That(everything.Special, Is.Null);
            Assert.That(everything.Ordinary, Is.Null);
            Assert.That(everything.Insignificant, Is.Null);
            Assert.That(everything.Inconsequential, Is.Null);
            Assert.That(everything.Forgettable, Is.Null);
        }

        public class Everything
        {
            public bool? IsAwesome { get; set; }
            public IIsSpecial Special { get; set; }
            public List<int> Ordinary { get; set; }
            public object Insignificant { get; set; }
            public Dictionary<string, string> Inconsequential { get; set; }
            public Enum Forgettable { get; set; }
        }

        public interface IIsSpecial
        {
        }
    }
}
