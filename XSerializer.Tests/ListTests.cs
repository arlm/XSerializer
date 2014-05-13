using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class ListTests
    {
        private const string MyCollectionXmlWithoutTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyCollection xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <MyItem>
    <Foo>abc</Foo>
    <Bar>
      <Name>Brian</Name>
      <Age>35</Age>
    </Bar>
    <Baz>xyz</Baz>
  </MyItem>
</MyCollection>";

        private const string MyCollectionXmlWithTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyCollection xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <MyItem xsi:type=""ListTests_ClassWithDynamicProperty"">
    <Foo>abc</Foo>
    <Bar>
      <Name>Brian</Name>
      <Age>35</Age>
    </Bar>
    <Baz>xyz</Baz>
  </MyItem>
</MyCollection>";

        private const string ContainerXmlWithNoAttributesWithoutTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Items>
    <ListTests_ClassWithDynamicProperty>
      <Foo>abc</Foo>
      <Bar>
        <Name>Brian</Name>
        <Age>35</Age>
      </Bar>
      <Baz>xyz</Baz>
    </ListTests_ClassWithDynamicProperty>
  </Items>
</MyContainer>";

        private const string ContainerXmlWithNoAttributesWithTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Items>
    <Item xsi:type=""ListTests_ClassWithDynamicProperty"">
      <Foo>abc</Foo>
      <Bar>
        <Name>Brian</Name>
        <Age>35</Age>
      </Bar>
      <Baz>xyz</Baz>
    </Item>
  </Items>
</MyContainer>";

        private const string ContainerXmlWithArrayAttributesWithoutTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Thingies>
    <Thingy>
      <Foo>abc</Foo>
      <Bar>
        <Name>Brian</Name>
        <Age>35</Age>
      </Bar>
      <Baz>xyz</Baz>
    </Thingy>
  </Thingies>
</MyContainer>";

        private const string ContainerXmlWithArrayAttributesWithTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Thingies>
    <Thingy xsi:type=""ListTests_ClassWithDynamicProperty"">
      <Foo>abc</Foo>
      <Bar>
        <Name>Brian</Name>
        <Age>35</Age>
      </Bar>
      <Baz>xyz</Baz>
    </Thingy>
  </Thingies>
</MyContainer>";

        private const string ContainerXmlWithElementAttributeWithoutTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Piece>
    <Foo>abc</Foo>
    <Bar>
      <Name>Brian</Name>
      <Age>35</Age>
    </Bar>
    <Baz>xyz</Baz>
  </Piece>
</MyContainer>";

        private const string ContainerXmlWithElementAttributeWithTypeHint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Piece xsi:type=""ListTests_ClassWithDynamicProperty"">
    <Foo>abc</Foo>
    <Bar>
      <Name>Brian</Name>
      <Age>35</Age>
    </Bar>
    <Baz>xyz</Baz>
  </Piece>
</MyContainer>";

        private const string OutOfOrderXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Item>
    <Foo>abc</Foo>
    <Bar>
      <Name>Brian</Name>
      <Age>35</Age>
    </Bar>
    <Baz>xyz</Baz>
  </Item>
  <SomethingElse>foobar</SomethingElse>
  <Item>
    <Foo>ABC</Foo>
    <Bar>
      <Name>Matilda</Name>
      <Age>3</Age>
    </Bar>
    <Baz>XYZ</Baz>
  </Item>
</MyContainer>";

        private static readonly ListTests_ClassWithDynamicProperty Item = new ListTests_ClassWithDynamicProperty { Foo = "abc", Bar = GetExpando("Brian", 35), Baz = "xyz" };
        private static readonly ListTests_ClassWithDynamicProperty SecondItem = new ListTests_ClassWithDynamicProperty { Foo = "ABC", Bar = GetExpando("Matilda", 3), Baz = "XYZ" };
        
        private static dynamic GetExpando(string name, int age)
        {
            dynamic expando = new ExpandoObject();
            expando.Name = name;
            expando.Age = age;
            return expando;
        }

        internal class ListSerializationTests : ObjectToXml
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    new List<ListTests_ClassWithDynamicProperty> { Item },
                    typeof(List<ListTests_ClassWithDynamicProperty>),
                    MyCollectionXmlWithoutTypeHint)
                        .SetName("List<> as root object");

                yield return new TestCaseData(
                    new CustomCollection { Item },
                    typeof(CustomCollection),
                    MyCollectionXmlWithoutTypeHint)
                        .SetName("Custom collection inheriting from ICollection<> as root object");

                yield return new TestCaseData(
                    new CustomEnumerable { Item },
                    typeof(CustomEnumerable),
                    MyCollectionXmlWithoutTypeHint)
                        .SetName("Custom collection inheriting from IEnumerable<> as root object");

                yield return new TestCaseData(
                    new ArrayList { Item },
                    typeof(ArrayList),
                    MyCollectionXmlWithTypeHint)
                        .SetName("ArrayList as root object");

                yield return new TestCaseData(
                    new NonGenericEnumerable { Item },
                    typeof(NonGenericEnumerable),
                    MyCollectionXmlWithTypeHint)
                        .SetName("Custom collection inheriting from IEnumerable as root object");

                yield return new TestCaseData(
                    new ContainerWithReadWriteGenericList { Items = new List<ListTests_ClassWithDynamicProperty> { Item } },
                    typeof(ContainerWithReadWriteGenericList),
                    ContainerXmlWithNoAttributesWithoutTypeHint)
                        .SetName("Container with read-write List<>");

                yield return new TestCaseData(
                    new ContainerWithReadWriteCustomCollection { Items = new CustomCollection { Item } },
                    typeof(ContainerWithReadWriteCustomCollection),
                    ContainerXmlWithNoAttributesWithoutTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from ICollection<>");

                yield return new TestCaseData(
                    new ContainerWithReadWriteCustomEnumerable { Items = new CustomEnumerable { Item } },
                    typeof(ContainerWithReadWriteCustomEnumerable),
                    ContainerXmlWithNoAttributesWithoutTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable<>");

                yield return new TestCaseData(
                    new ContainerWithReadWriteArrayList { Items = new ArrayList { Item } },
                    typeof(ContainerWithReadWriteArrayList),
                    ContainerXmlWithNoAttributesWithTypeHint)
                        .SetName("Container with read-write ArrayList");

                yield return new TestCaseData(
                    new ContainerWithReadWriteNonGenericEnumerable { Items = new NonGenericEnumerable { Item } },
                    typeof(ContainerWithReadWriteNonGenericEnumerable),
                    ContainerXmlWithNoAttributesWithTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable");

                yield return new TestCaseData(
                    new ContainerWithReadOnlyGenericList(new List<ListTests_ClassWithDynamicProperty> { Item }),
                    typeof(ContainerWithReadOnlyGenericList),
                    ContainerXmlWithNoAttributesWithoutTypeHint)
                        .SetName("Container with read-only List<>");

                yield return new TestCaseData(
                    new ContainerWithReadOnlyCustomCollection(new CustomCollection { Item }),
                    typeof(ContainerWithReadOnlyCustomCollection),
                    ContainerXmlWithNoAttributesWithoutTypeHint)
                        .SetName("Container with read-only Custom collection inheriting from ICollection<>");

                yield return new TestCaseData(
                    new ContainerWithReadOnlyCustomEnumerable(new CustomEnumerable { Item }),
                    typeof(ContainerWithReadOnlyCustomEnumerable),
                    ContainerXmlWithNoAttributesWithoutTypeHint)
                        .SetName("Container with read-only Custom collection inheriting from IEnumerable<>");

                yield return new TestCaseData(
                    new ContainerWithReadOnlyArrayList(new ArrayList { Item }),
                    typeof(ContainerWithReadOnlyArrayList),
                    ContainerXmlWithNoAttributesWithTypeHint)
                        .SetName("Container with read-only ArrayList");

                yield return new TestCaseData(
                    new ContainerWithReadOnlyNonGenericEnumerable(new NonGenericEnumerable { Item }),
                    typeof(ContainerWithReadOnlyNonGenericEnumerable),
                    ContainerXmlWithNoAttributesWithTypeHint)
                        .SetName("Container with read-only Custom collection inheriting from IEnumerable");

                yield return new TestCaseData(
                    new ContainerWithReadWriteGenericListAndArrayAttributes { Items = new List<ListTests_ClassWithDynamicProperty> { Item } },
                    typeof(ContainerWithReadWriteGenericListAndArrayAttributes),
                    ContainerXmlWithArrayAttributesWithoutTypeHint)
                        .SetName("Container with read-write List<> with xml array attributes");

                yield return new TestCaseData(
                    new ContainerWithReadWriteCustomCollectionAndArrayAttributes { Items = new CustomCollection { Item } },
                    typeof(ContainerWithReadWriteCustomCollectionAndArrayAttributes),
                    ContainerXmlWithArrayAttributesWithoutTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from ICollection<> with xml array attributes");

                yield return new TestCaseData(
                    new ContainerWithReadWriteCustomEnumerableAndArrayAttributes { Items = new CustomEnumerable { Item } },
                    typeof(ContainerWithReadWriteCustomEnumerableAndArrayAttributes),
                    ContainerXmlWithArrayAttributesWithoutTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable<> with xml array attributes");

                yield return new TestCaseData(
                    new ContainerWithReadWriteArrayListAndArrayAttributes { Items = new ArrayList { Item } },
                    typeof(ContainerWithReadWriteArrayListAndArrayAttributes),
                    ContainerXmlWithArrayAttributesWithTypeHint)
                        .SetName("Container with read-write ArrayList with xml array attributes");

                yield return new TestCaseData(
                    new ContainerWithReadWriteNonGenericEnumerableAndArrayAttributes { Items = new NonGenericEnumerable { Item } },
                    typeof(ContainerWithReadWriteNonGenericEnumerableAndArrayAttributes),
                    ContainerXmlWithArrayAttributesWithTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable with xml array attributes");

                yield return new TestCaseData(
                    new ContainerWithReadWriteGenericListAndElementAttribute { Items = new List<ListTests_ClassWithDynamicProperty> { Item } },
                    typeof(ContainerWithReadWriteGenericListAndElementAttribute),
                    ContainerXmlWithElementAttributeWithoutTypeHint)
                        .SetName("Container with read-write List<> with xml element attribute");

                yield return new TestCaseData(
                    new ContainerWithReadWriteCustomCollectionAndElementAttribute { Items = new CustomCollection { Item } },
                    typeof(ContainerWithReadWriteCustomCollectionAndElementAttribute),
                    ContainerXmlWithElementAttributeWithoutTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from ICollection<> with xml element attribute");

                yield return new TestCaseData(
                    new ContainerWithReadWriteCustomEnumerableAndElementAttribute { Items = new CustomEnumerable { Item } },
                    typeof(ContainerWithReadWriteCustomEnumerableAndElementAttribute),
                    ContainerXmlWithElementAttributeWithoutTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable<> with xml element attribute");

                yield return new TestCaseData(
                    new ContainerWithReadWriteArrayListAndElementAttribute { Items = new ArrayList { Item } },
                    typeof(ContainerWithReadWriteArrayListAndElementAttribute),
                    ContainerXmlWithElementAttributeWithTypeHint)
                        .SetName("Container with read-write ArrayList with xml element attribute");

                yield return new TestCaseData(
                    new ContainerWithReadWriteNonGenericEnumerableAndElementAttribute { Items = new NonGenericEnumerable { Item } },
                    typeof(ContainerWithReadWriteNonGenericEnumerableAndElementAttribute),
                    ContainerXmlWithElementAttributeWithTypeHint)
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable with xml element attribute");
            }

            protected override IXmlSerializerInternal GetSerializer(Type type)
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    return ListSerializer.GetSerializer(type, new TestXmlSerializerOptions { RootElementName = "MyCollection" }, "MyItem");
                }

                return XmlSerializerFactory.Instance.GetSerializer(type, new TestXmlSerializerOptions { RootElementName = "MyContainer" });
            }

            protected override bool AlwaysEmitTypes
            {
                get { return true; }
            }
        }

        internal class ListDeserializationTests : XmlToObject
        {
            protected override IEnumerable<TestCaseData> GetTestCaseData()
            {
                yield return new TestCaseData(
                    MyCollectionXmlWithoutTypeHint,
                    typeof(List<ListTests_ClassWithDynamicProperty>),
                    new List<ListTests_ClassWithDynamicProperty> { Item })
                        .SetName("List<> as root object");

                yield return new TestCaseData(
                    MyCollectionXmlWithoutTypeHint,
                    typeof(CustomCollection),
                    new CustomCollection { Item })
                        .SetName("Custom collection inheriting from ICollection<> as root object");

                yield return new TestCaseData(
                    MyCollectionXmlWithoutTypeHint,
                    typeof(CustomEnumerable),
                    new CustomEnumerable { Item })
                        .SetName("Custom collection inheriting from IEnumerable<> as root object");

                yield return new TestCaseData(
                    MyCollectionXmlWithTypeHint,
                    typeof(ArrayList),
                    new ArrayList { Item })
                        .SetName("ArrayList as root object");

                yield return new TestCaseData(
                    MyCollectionXmlWithTypeHint,
                    typeof(NonGenericEnumerable),
                    new NonGenericEnumerable { Item })
                        .SetName("Custom collection inheriting from IEnumerable as root object");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithoutTypeHint,
                    typeof(ContainerWithReadWriteGenericList),
                    new ContainerWithReadWriteGenericList { Items = new List<ListTests_ClassWithDynamicProperty> { Item } })
                        .SetName("Container with read-write List<>");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithoutTypeHint,
                    typeof(ContainerWithReadWriteCustomCollection),
                    new ContainerWithReadWriteCustomCollection { Items = new CustomCollection { Item } })
                        .SetName("Container with read-write Custom collection inheriting from ICollection<>");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithoutTypeHint,
                    typeof(ContainerWithReadWriteCustomEnumerable),
                    new ContainerWithReadWriteCustomEnumerable { Items = new CustomEnumerable { Item } })
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable<>");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithTypeHint,
                    typeof(ContainerWithReadWriteArrayList),
                    new ContainerWithReadWriteArrayList { Items = new ArrayList { Item } })
                        .SetName("Container with read-write ArrayList");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithTypeHint,
                    typeof(ContainerWithReadWriteNonGenericEnumerable),
                    new ContainerWithReadWriteNonGenericEnumerable { Items = new NonGenericEnumerable { Item } })
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithoutTypeHint,
                    typeof(ContainerWithReadOnlyGenericList),
                    new ContainerWithReadOnlyGenericList(new List<ListTests_ClassWithDynamicProperty> { Item }))
                        .SetName("Container with read-only List<>");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithoutTypeHint,
                    typeof(ContainerWithReadOnlyCustomCollection),
                    new ContainerWithReadOnlyCustomCollection(new CustomCollection { Item }))
                        .SetName("Container with read-only Custom collection inheriting from ICollection<>");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithoutTypeHint,
                    typeof(ContainerWithReadOnlyCustomEnumerable),
                    new ContainerWithReadOnlyCustomEnumerable(new CustomEnumerable { Item }))
                        .SetName("Container with read-only Custom collection inheriting from IEnumerable<>");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithTypeHint,
                    typeof(ContainerWithReadOnlyArrayList),
                    new ContainerWithReadOnlyArrayList(new ArrayList { Item }))
                        .SetName("Container with read-only ArrayList");

                yield return new TestCaseData(
                    ContainerXmlWithNoAttributesWithTypeHint,
                    typeof(ContainerWithReadOnlyNonGenericEnumerable),
                    new ContainerWithReadOnlyNonGenericEnumerable(new NonGenericEnumerable { Item }))
                        .SetName("Container with read-only Custom collection inheriting from IEnumerable");

                yield return new TestCaseData(
                    ContainerXmlWithArrayAttributesWithoutTypeHint,
                    typeof(ContainerWithReadWriteGenericListAndArrayAttributes),
                    new ContainerWithReadWriteGenericListAndArrayAttributes { Items = new List<ListTests_ClassWithDynamicProperty> { Item } })
                        .SetName("Container with read-write List<> with xml array attributes");

                yield return new TestCaseData(
                    ContainerXmlWithArrayAttributesWithoutTypeHint,
                    typeof(ContainerWithReadWriteCustomCollectionAndArrayAttributes),
                    new ContainerWithReadWriteCustomCollectionAndArrayAttributes { Items = new CustomCollection { Item } })
                        .SetName("Container with read-write Custom collection inheriting from ICollection<> with xml array attributes");

                yield return new TestCaseData(
                    ContainerXmlWithArrayAttributesWithoutTypeHint,
                    typeof(ContainerWithReadWriteCustomEnumerableAndArrayAttributes),
                    new ContainerWithReadWriteCustomEnumerableAndArrayAttributes { Items = new CustomEnumerable { Item } })
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable<> with xml array attributes");

                yield return new TestCaseData(
                    ContainerXmlWithArrayAttributesWithTypeHint,
                    typeof(ContainerWithReadWriteArrayListAndArrayAttributes),
                    new ContainerWithReadWriteArrayListAndArrayAttributes { Items = new ArrayList { Item } })
                        .SetName("Container with read-write ArrayList with xml array attributes");

                yield return new TestCaseData(
                    ContainerXmlWithArrayAttributesWithTypeHint,
                    typeof(ContainerWithReadWriteNonGenericEnumerableAndArrayAttributes),
                    new ContainerWithReadWriteNonGenericEnumerableAndArrayAttributes { Items = new NonGenericEnumerable { Item } })
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable with xml array attributes");

                yield return new TestCaseData(
                    ContainerXmlWithElementAttributeWithoutTypeHint,
                    typeof(ContainerWithReadWriteGenericListAndElementAttribute),
                    new ContainerWithReadWriteGenericListAndElementAttribute { Items = new List<ListTests_ClassWithDynamicProperty> { Item } })
                        .SetName("Container with read-write List<> with xml element attribute");

                yield return new TestCaseData(
                    ContainerXmlWithElementAttributeWithoutTypeHint,
                    typeof(ContainerWithReadWriteCustomCollectionAndElementAttribute),
                    new ContainerWithReadWriteCustomCollectionAndElementAttribute { Items = new CustomCollection { Item } })
                        .SetName("Container with read-write Custom collection inheriting from ICollection<> with xml element attribute");

                yield return new TestCaseData(
                    ContainerXmlWithElementAttributeWithoutTypeHint,
                    typeof(ContainerWithReadWriteCustomEnumerableAndElementAttribute),
                    new ContainerWithReadWriteCustomEnumerableAndElementAttribute { Items = new CustomEnumerable { Item } })
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable<> with xml element attribute");

                yield return new TestCaseData(
                    ContainerXmlWithElementAttributeWithTypeHint,
                    typeof(ContainerWithReadWriteArrayListAndElementAttribute),
                    new ContainerWithReadWriteArrayListAndElementAttribute { Items = new ArrayList { Item } })
                        .SetName("Container with read-write ArrayList with xml element attribute");

                yield return new TestCaseData(
                    ContainerXmlWithElementAttributeWithTypeHint,
                    typeof(ContainerWithReadWriteNonGenericEnumerableAndElementAttribute),
                    new ContainerWithReadWriteNonGenericEnumerableAndElementAttribute { Items = new NonGenericEnumerable { Item } })
                        .SetName("Container with read-write Custom collection inheriting from IEnumerable with xml element attribute");

                yield return new TestCaseData(
                    OutOfOrderXml,
                    typeof(ContainerWithReadWriteGenericListAndElementAttributeAndAnotherProperty),
                    new ContainerWithReadWriteGenericListAndElementAttributeAndAnotherProperty { Items = new List<ListTests_ClassWithDynamicProperty> { Item, SecondItem }, SomethingElse = "foobar" })
                        .SetName("Container with read-write collection with xml element attribute and out-of-order xml input");

                yield return new TestCaseData(
                    OutOfOrderXml,
                    typeof(ContainerWithReadOnlyGenericListAndElementAttributeAndAnotherProperty),
                    new ContainerWithReadOnlyGenericListAndElementAttributeAndAnotherProperty(new List<ListTests_ClassWithDynamicProperty> { Item, SecondItem }) { SomethingElse = "foobar" })
                        .SetName("Container with read-only collection with xml element attribute and out-of-order xml input");
            }

            protected override IXmlSerializerInternal GetSerializer(Type type)
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    return ListSerializer.GetSerializer(type, new TestXmlSerializerOptions { RootElementName = "MyCollection" }, "MyItem");
                }

                return XmlSerializerFactory.Instance.GetSerializer(type, new TestXmlSerializerOptions { RootElementName = "MyContainer" });
            }
        }

        [Test]
        public void CanSerializeListAsRoot()
        {
            var data = new List<ArrayTests.DataPoint>
                {
                    new ArrayTests.DataPoint
                    {
                        Name = "FooBar",
                        Preference = new ArrayTests.Preference
                        {
                            Id = 123
                        }
                    }
                };

            var serializer = new XmlSerializer<List<ArrayTests.DataPoint>>(options => options.Indent(), typeof(ArrayTests.Preference));

            var xml = serializer.Serialize(data);

            Assert.That(xml, Contains.Substring("</ListOfDataPoint>"));
            Assert.That(xml, Contains.Substring(@"xsi:type=""Preference"""));
            Assert.IsTrue(xml.IndexOf(@"xsi:type=""Preference""") == xml.LastIndexOf(@"xsi:type=""Preference"""));
        }

        [Test]
        public void CanDeserializeListAsRoot()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ListOfDataPoint xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <DataPoint>
    <Name>FooBar</Name>
    <Preference xsi:type=""Preference"">
      <Id>123</Id>
    </Preference>
  </DataPoint>
</ListOfDataPoint>";

            var serializer = new XmlSerializer<List<ArrayTests.DataPoint>>(options => options.Indent(), typeof(ArrayTests.Preference));

            var data = serializer.Deserialize(xml);

            Assert.That(data.Count, Is.EqualTo(1));
        }

        #region read-write with no attributes

        public class ContainerWithReadWriteGenericList
        {
            public List<ListTests_ClassWithDynamicProperty> Items { get; set; }
        }

        public class ContainerWithReadWriteCustomCollection
        {
            public CustomCollection Items { get; set; }
        }

        public class ContainerWithReadWriteCustomEnumerable
        {
            public CustomEnumerable Items { get; set; }
        }

        public class ContainerWithReadWriteArrayList
        {
            public ArrayList Items { get; set; }
        }

        public class ContainerWithReadWriteNonGenericEnumerable
        {
            public NonGenericEnumerable Items { get; set; }
        }

        #endregion

        #region read-only with no attributes

        public class ContainerWithReadOnlyGenericList
        {
            public ContainerWithReadOnlyGenericList()
            {
                Items = new List<ListTests_ClassWithDynamicProperty>();
            }

            public ContainerWithReadOnlyGenericList(IEnumerable<ListTests_ClassWithDynamicProperty> items)
            {
                Items = new List<ListTests_ClassWithDynamicProperty>();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }

            public List<ListTests_ClassWithDynamicProperty> Items { get; private set; }
        }

        public class ContainerWithReadOnlyCustomCollection
        {
            public ContainerWithReadOnlyCustomCollection()
            {
                Items = new CustomCollection();
            }

            public ContainerWithReadOnlyCustomCollection(IEnumerable<ListTests_ClassWithDynamicProperty> items)
            {
                Items = new CustomCollection();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }

            public CustomCollection Items { get; private set; }
        }

        public class ContainerWithReadOnlyCustomEnumerable
        {
            public ContainerWithReadOnlyCustomEnumerable()
            {
                Items = new CustomEnumerable();
            }

            public ContainerWithReadOnlyCustomEnumerable(IEnumerable<ListTests_ClassWithDynamicProperty> items)
            {
                Items = new CustomEnumerable();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }

            public CustomEnumerable Items { get; private set; }
        }

        public class ContainerWithReadOnlyArrayList
        {
            public ContainerWithReadOnlyArrayList()
            {
                Items = new ArrayList();
            }

            public ContainerWithReadOnlyArrayList(IEnumerable items)
            {
                Items = new ArrayList();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }

            public ArrayList Items { get; private set; }
        }

        public class ContainerWithReadOnlyNonGenericEnumerable
        {
            public ContainerWithReadOnlyNonGenericEnumerable()
            {
                Items = new NonGenericEnumerable();
            }

            public ContainerWithReadOnlyNonGenericEnumerable(IEnumerable items)
            {
                Items = new NonGenericEnumerable();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }

            public NonGenericEnumerable Items { get; private set; }
        }

        #endregion

        #region read-write with array attributes

        public class ContainerWithReadWriteGenericListAndArrayAttributes
        {
            [XmlArray("Thingies")]
            [XmlArrayItem("Thingy")]
            public List<ListTests_ClassWithDynamicProperty> Items { get; set; }
        }

        public class ContainerWithReadWriteCustomCollectionAndArrayAttributes
        {
            [XmlArray("Thingies")]
            [XmlArrayItem("Thingy")]
            public CustomCollection Items { get; set; }
        }

        public class ContainerWithReadWriteCustomEnumerableAndArrayAttributes
        {
            [XmlArray("Thingies")]
            [XmlArrayItem("Thingy")]
            public CustomEnumerable Items { get; set; }
        }

        public class ContainerWithReadWriteArrayListAndArrayAttributes
        {
            [XmlArray("Thingies")]
            [XmlArrayItem("Thingy")]
            public ArrayList Items { get; set; }
        }

        public class ContainerWithReadWriteNonGenericEnumerableAndArrayAttributes
        {
            [XmlArray("Thingies")]
            [XmlArrayItem("Thingy")]
            public NonGenericEnumerable Items { get; set; }
        }

        #endregion

        #region read-write with element attribute

        public class ContainerWithReadWriteGenericListAndElementAttribute
        {
            [XmlElement("Piece")]
            public List<ListTests_ClassWithDynamicProperty> Items { get; set; }
        }

        public class ContainerWithReadWriteCustomCollectionAndElementAttribute
        {
            [XmlElement("Piece")]
            public CustomCollection Items { get; set; }
        }

        public class ContainerWithReadWriteCustomEnumerableAndElementAttribute
        {
            [XmlElement("Piece")]
            public CustomEnumerable Items { get; set; }
        }

        public class ContainerWithReadWriteArrayListAndElementAttribute
        {
            [XmlElement("Piece")]
            public ArrayList Items { get; set; }
        }

        public class ContainerWithReadWriteNonGenericEnumerableAndElementAttribute
        {
            [XmlElement("Piece")]
            public NonGenericEnumerable Items { get; set; }
        }

        public class ContainerWithReadWriteGenericListAndElementAttributeAndAnotherProperty
        {
            [XmlElement("Item")]
            public List<ListTests_ClassWithDynamicProperty> Items { get; set; }

            public string SomethingElse { get; set; }
        }

        public class ContainerWithReadOnlyGenericListAndElementAttributeAndAnotherProperty
        {
            public ContainerWithReadOnlyGenericListAndElementAttributeAndAnotherProperty()
            {
                Items = new List<ListTests_ClassWithDynamicProperty>();
            }

            public ContainerWithReadOnlyGenericListAndElementAttributeAndAnotherProperty(IEnumerable<ListTests_ClassWithDynamicProperty> items)
            {
                Items = new List<ListTests_ClassWithDynamicProperty>();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }

            [XmlElement("Item")]
            public List<ListTests_ClassWithDynamicProperty> Items { get; private set; }

            public string SomethingElse { get; set; }
        }

        #endregion

        public class CustomCollection : ICollection<ListTests_ClassWithDynamicProperty>
        {
            private readonly List<ListTests_ClassWithDynamicProperty> _list = new List<ListTests_ClassWithDynamicProperty>();

            public IEnumerator<ListTests_ClassWithDynamicProperty> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(int value)
            {
            }

            public void Add(ListTests_ClassWithDynamicProperty item)
            {
                _list.Add(item);
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(ListTests_ClassWithDynamicProperty item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(ListTests_ClassWithDynamicProperty[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(ListTests_ClassWithDynamicProperty item)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }
        }

        public class CustomEnumerable : IEnumerable<ListTests_ClassWithDynamicProperty>
        {
            private readonly List<ListTests_ClassWithDynamicProperty> _list = new List<ListTests_ClassWithDynamicProperty>();

            public IEnumerator<ListTests_ClassWithDynamicProperty> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(int value)
            {
            }

            public void Add(ListTests_ClassWithDynamicProperty value)
            {
                _list.Add(value);
            }
        }

        public class NonGenericEnumerable : IEnumerable
        {
            private readonly ArrayList _list = new ArrayList();

            public IEnumerator GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public void Add(int value)
            {
                _list.Add(value);
            }

            public void Add(object value)
            {
                _list.Add(value);
            }
        }

        public class ListTests_ClassWithDynamicProperty
        {
            public string Foo { get; set; }
            public dynamic Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}