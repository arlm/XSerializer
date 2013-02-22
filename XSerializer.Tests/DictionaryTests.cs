namespace XSerializer.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    public class DictionaryTests
    {
        private const string _genericDictionaryXmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<{0} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Map>
    <Item>
      <Key>foo</Key>
      <Value>bar</Value>
    </Item>
    <Item>
      <Key>baz</Key>
      <Value>qux</Value>
    </Item>
  </Map>
</{0}>";

        public class DictionarySerializationTests : ObjectToXml
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    new ReadWriteGenericDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } } },
                    typeof(ReadWriteGenericDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteGenericDictionaryClass).Name))
                        .SetName("Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    new ReadOnlyGenericDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }),
                    typeof(ReadOnlyGenericDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyGenericDictionaryClass).Name))
                        .SetName("Generic Read-Only Dictionary");
            }
        }

        public class DictionaryDeserializationTests : XmlToObject
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteGenericDictionaryClass).Name),
                    typeof(ReadWriteGenericDictionaryClass),
                    new ReadWriteGenericDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } } })
                        .SetName("Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyGenericDictionaryClass).Name),
                    typeof(ReadOnlyGenericDictionaryClass),
                    new ReadOnlyGenericDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }))
                        .SetName("Generic Read-Only Dictionary");
            }
        }

        public class ReadWriteGenericDictionaryClass
        {
            public Dictionary<string, string> Map { get; set; }
        }

        public class ReadOnlyGenericDictionaryClass
        {
            public ReadOnlyGenericDictionaryClass()
            {
                Map = new Dictionary<string, string>();
            }

            public ReadOnlyGenericDictionaryClass(IDictionary<string, string> map)
            {
                Map = new Dictionary<string, string>(map);
            }

            public Dictionary<string, string> Map { get; private set; }
        }
    }
}