using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DictionaryTests
    {
        private const string GenericDictionaryXmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<{0} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <A>Start</A>
  <Map>
    <Item>
      <Key{1}>foo</Key>
      <Value{2}>bar</Value>
    </Item>
    <Item>
      <Key{1}>baz</Key>
      <Value{2}>qux</Value>
    </Item>
  </Map>
  <Z>End</Z>
</{0}>";

        internal class DictionarySerializationTests : ObjectToXml
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    new ReadWriteGenericDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteGenericDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteGenericDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    new ReadOnlyGenericDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyGenericDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyGenericDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Only Dictionary");

                yield return new TestCaseData(
                    new ReadWriteNonGenericDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteNonGenericDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    new ReadOnlyNonGenericDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyNonGenericDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Only Dictionary");

                yield return new TestCaseData(
                    new ReadWriteGenericIDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteGenericIDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteGenericIDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Write IDictionary");

                yield return new TestCaseData(
                    new ReadOnlyGenericIDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyGenericIDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyGenericIDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Only IDictionary");

                yield return new TestCaseData(
                    new ReadWriteNonGenericIDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteNonGenericIDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Write IDictionary");

                yield return new TestCaseData(
                    new ReadOnlyNonGenericIDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyNonGenericIDictionaryClass),
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Only IDictionary");
            }

            protected override bool AlwaysEmitTypes
            {
                get
                {
                    return true;
                }
            }
        }

        internal class DictionaryDeserializationTests : XmlToObject
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteGenericDictionaryClass).Name, "", ""),
                    typeof(ReadWriteGenericDictionaryClass),
                    new ReadWriteGenericDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyGenericDictionaryClass).Name, "", ""),
                    typeof(ReadOnlyGenericDictionaryClass),
                    new ReadOnlyGenericDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Generic Read-Only Dictionary");

                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadWriteNonGenericDictionaryClass),
                    new ReadWriteNonGenericDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadOnlyNonGenericDictionaryClass),
                    new ReadOnlyNonGenericDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Only Dictionary");
                
                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteGenericIDictionaryClass).Name, "", ""),
                    typeof(ReadWriteGenericIDictionaryClass),
                    new ReadWriteGenericIDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Generic Read-Write IDictionary");

                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyGenericIDictionaryClass).Name, "", ""),
                    typeof(ReadOnlyGenericIDictionaryClass),
                    new ReadOnlyGenericIDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Generic Read-Only IDictionary");

                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadWriteNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadWriteNonGenericIDictionaryClass),
                    new ReadWriteNonGenericIDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Write IDictionary");
                
                yield return new TestCaseData(
                    string.Format(GenericDictionaryXmlFormat, typeof(ReadOnlyNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadOnlyNonGenericIDictionaryClass),
                    new ReadOnlyNonGenericIDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Only IDictionary");
            }
        }

        [Test]
        public void CanSerializeDictionaryAsRoot()
        {
            var data = new Dictionary<string, string>
                {
                    { "abc", "123" },
                };

            var serializer = new XmlSerializer<Dictionary<string, string>>(options => options.Indent());

            var xml = serializer.Serialize(data);

            Assert.That(xml, Contains.Substring("</DictionaryOfString_String>"));
            Assert.That(xml, Contains.Substring("abc"));
            Assert.That(xml, Contains.Substring("123"));
        }

        [Test]
        public void CanDeserializeDictionaryAsRoot()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DictionaryOfString_String xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Item>
    <Key>abc</Key>
    <Value>123</Value>
  </Item>
</DictionaryOfString_String>";

            var serializer = new XmlSerializer<Dictionary<string, string>>(options => options.Indent(), typeof(ArrayTests.Preference));

            var data = serializer.Deserialize(xml);

            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data, Contains.Item(new KeyValuePair<string, string>("abc", "123")));
        }

        public class ReadWriteGenericDictionaryClass
        {
            public string A { get; set; }
            public Dictionary<string, string> Map { get; set; }
            public string Z { get; set; }
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

            public string A { get; set; }
            public Dictionary<string, string> Map { get; private set; }
            public string Z { get; set; }
        }

        public class ReadWriteNonGenericDictionaryClass
        {
            public string A { get; set; }
            public Hashtable Map { get; set; }
            public string Z { get; set; }
        }

        public class ReadOnlyNonGenericDictionaryClass
        {
            public ReadOnlyNonGenericDictionaryClass()
            {
                Map = new Hashtable();
            }

            public ReadOnlyNonGenericDictionaryClass(IDictionary map)
            {
                Map = new Hashtable(map);
            }

            public string A { get; set; }
            public Hashtable Map { get; private set; }
            public string Z { get; set; }
        }

        public class ReadWriteGenericIDictionaryClass
        {
            public string A { get; set; }
            public IDictionary<string, string> Map { get; set; }
            public string Z { get; set; }
        }

        public class ReadOnlyGenericIDictionaryClass
        {
            public ReadOnlyGenericIDictionaryClass()
            {
                Map = new Dictionary<string, string>();
            }

            public ReadOnlyGenericIDictionaryClass(IDictionary<string, string> map)
            {
                Map = new Dictionary<string, string>(map);
            }

            public string A { get; set; }
            public IDictionary<string, string> Map { get; private set; }
            public string Z { get; set; }
        }

        public class ReadWriteNonGenericIDictionaryClass
        {
            public string A { get; set; }
            public IDictionary Map { get; set; }
            public string Z { get; set; }
        }

        public class ReadOnlyNonGenericIDictionaryClass
        {
            public ReadOnlyNonGenericIDictionaryClass()
            {
                Map = new Hashtable();
            }

            public ReadOnlyNonGenericIDictionaryClass(IDictionary map)
            {
                Map = new Hashtable(map);
            }

            public string A { get; set; }
            public IDictionary Map { get; private set; }
            public string Z { get; set; }
        }
    }
}