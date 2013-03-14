using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DictionaryTests
    {
        private const string _genericDictionaryXmlFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

        public class DictionarySerializationTests : ObjectToXml
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    new ReadWriteGenericDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteGenericDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteGenericDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    new ReadOnlyGenericDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyGenericDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyGenericDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Only Dictionary");

                yield return new TestCaseData(
                    new ReadWriteNonGenericDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteNonGenericDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    new ReadOnlyNonGenericDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyNonGenericDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Only Dictionary");

                yield return new TestCaseData(
                    new ReadWriteGenericIDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteGenericIDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteGenericIDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Write IDictionary");

                yield return new TestCaseData(
                    new ReadOnlyGenericIDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyGenericIDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyGenericIDictionaryClass).Name, "", ""))
                        .SetName("Generic Read-Only IDictionary");

                yield return new TestCaseData(
                    new ReadWriteNonGenericIDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" },
                    typeof(ReadWriteNonGenericIDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Write IDictionary");

                yield return new TestCaseData(
                    new ReadOnlyNonGenericIDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyNonGenericIDictionaryClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""))
                        .SetName("Non-Generic Read-Only IDictionary");

                yield return new TestCaseData(
                    new ReadWriteExpandoObjectClass { Map = GetExpandoObject(), A = "Start", Z = "End" },
                    typeof(ReadWriteExpandoObjectClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteExpandoObjectClass).Name, "", " xsi:type=\"xsd:string\""))
                        .SetName("Read-Write ExpandoObject");

                yield return new TestCaseData(
                    new ReadOnlyExpandoObjectClass(new Dictionary<string, object> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" },
                    typeof(ReadOnlyExpandoObjectClass),
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyExpandoObjectClass).Name, "", " xsi:type=\"xsd:string\""))
                        .SetName("Read-Only ExpandoObject");
            }
        }

        public class DictionaryDeserializationTests : XmlToObject
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteGenericDictionaryClass).Name, "", ""),
                    typeof(ReadWriteGenericDictionaryClass),
                    new ReadWriteGenericDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyGenericDictionaryClass).Name, "", ""),
                    typeof(ReadOnlyGenericDictionaryClass),
                    new ReadOnlyGenericDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Generic Read-Only Dictionary");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadWriteNonGenericDictionaryClass),
                    new ReadWriteNonGenericDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Write Dictionary");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyNonGenericDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadOnlyNonGenericDictionaryClass),
                    new ReadOnlyNonGenericDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Only Dictionary");
                
                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteGenericIDictionaryClass).Name, "", ""),
                    typeof(ReadWriteGenericIDictionaryClass),
                    new ReadWriteGenericIDictionaryClass { Map = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Generic Read-Write IDictionary");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyGenericIDictionaryClass).Name, "", ""),
                    typeof(ReadOnlyGenericIDictionaryClass),
                    new ReadOnlyGenericIDictionaryClass(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Generic Read-Only IDictionary");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadWriteNonGenericIDictionaryClass),
                    new ReadWriteNonGenericIDictionaryClass { Map = new Hashtable { { "foo", "bar" }, { "baz", "qux" } }, A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Write IDictionary");
                
                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyNonGenericIDictionaryClass).Name, " xsi:type=\"xsd:string\"", " xsi:type=\"xsd:string\""),
                    typeof(ReadOnlyNonGenericIDictionaryClass),
                    new ReadOnlyNonGenericIDictionaryClass(new Hashtable { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Non-Generic Read-Only IDictionary");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadWriteExpandoObjectClass).Name, "", " xsi:type=\"xsd:string\""),
                    typeof(ReadWriteExpandoObjectClass),
                    new ReadWriteExpandoObjectClass { Map = GetExpandoObject(), A = "Start", Z = "End" })
                        .SetName("Read-Write ExpandoObject");

                yield return new TestCaseData(
                    string.Format(_genericDictionaryXmlFormat, typeof(ReadOnlyExpandoObjectClass).Name, "", " xsi:type=\"xsd:string\""),
                    typeof(ReadOnlyExpandoObjectClass),
                    new ReadOnlyExpandoObjectClass(new Dictionary<string, object> { { "foo", "bar" }, { "baz", "qux" } }) { A = "Start", Z = "End" })
                        .SetName("Read-Only ExpandoObject");

                Func<Entity> getExpectedEntity = () =>
                {
                    var entity = new Entity { Email = "HJDHSJ@HJDHJHSJ.com", Name = "Anson Smith" };

                    ((dynamic)entity.Extension).EmailFormat = "Html";
                    return entity;
                };

                yield return new TestCaseData(
                    @"<?xml version=""1.0"" encoding=""utf-8""?><Entity xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Email>HJDHSJ@HJDHJHSJ.com</Email><Name>Anson Smith</Name><Extension><Item><Key>EmailFormat</Key><Value xsi:type=""xsd:string"">Html</Value></Item></Extension></Entity>",
                    typeof(Entity),
                    getExpectedEntity())
                        .SetName("Protect against reading past expando's items");
            }
        }

        private static ExpandoObject GetExpandoObject()
        {
            dynamic expando = new ExpandoObject();
            expando.foo = "bar";
            expando.baz = "qux";
            return expando;
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

        public class ReadWriteExpandoObjectClass
        {
            public string A { get; set; }
            public ExpandoObject Map { get; set; }
            public string Z { get; set; }
        }

        public class ReadOnlyExpandoObjectClass
        {
            public ReadOnlyExpandoObjectClass()
            {
                Map = new ExpandoObject();
            }

            public ReadOnlyExpandoObjectClass(IDictionary<string, object> map)
            {
                Map = new ExpandoObject();

                var iMap = (IDictionary<string, object>)Map;
                foreach (var item in map)
                {
                    iMap.Add(item);
                }
            }

            public string A { get; set; }
            public ExpandoObject Map { get; private set; }
            public string Z { get; set; }
        }

        public class Entity
        {
            public Entity()
            {
                Extension = new ExpandoObject();
            }

            public string Email { get; set; }
            public string Name { get; set; }
            public ExpandoObject Extension { get; set; }
        }
    }
}