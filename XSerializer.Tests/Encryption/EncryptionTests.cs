using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests.Encryption
{
    [TestFixture]
    public class EncryptionTests
    {
        private SerializationState _serializationState;

        [SetUp]
        public void Setup()
        {
            _serializationState = new SerializationState();
        }

        [Test]
        public void CanEncryptAndDecryptEntireRootObjectViaOptions()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer = new XmlSerializer<UnencryptedThing>(x => x.EncryptRootObject().WithEncryptionMechanism(encryptionMechanism));

            var instance = new UnencryptedThing
            {
                Foo = 123,
                Bar = true
            };

            var xml = serializer.Serialize(instance);

            var doc = XDocument.Parse(xml);

            Assert.That(encryptionMechanism.Decrypt(doc.Root.Value, null, _serializationState), Is.EqualTo("<Bar>true</Bar>"));
            Assert.That(encryptionMechanism.Decrypt(doc.Root.Attribute("Foo").Value, null, _serializationState), Is.EqualTo("123"));

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Foo, Is.EqualTo(instance.Foo));
            Assert.That(roundTrip.Bar, Is.EqualTo(instance.Bar));
        }

        [Test]
        public void CanEncryptAndDecryptEntireRootObjectViaEncryptAttribute()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer = new XmlSerializer<EncryptedThing>(x => x.WithEncryptionMechanism(encryptionMechanism));

            var instance = new EncryptedThing
            {
                Foo = 123,
                Bar = true
            };

            var xml = serializer.Serialize(instance);

            var doc = XDocument.Parse(xml);

            Assert.That(encryptionMechanism.Decrypt(doc.Root.Value, null, _serializationState), Is.EqualTo("<Bar>true</Bar>"));
            Assert.That(encryptionMechanism.Decrypt(doc.Root.Attribute("Foo").Value, null, _serializationState), Is.EqualTo("123"));

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Foo, Is.EqualTo(instance.Foo));
            Assert.That(roundTrip.Bar, Is.EqualTo(instance.Bar));
        }

        [Test]
        public void EncryptsAndDecryptsPropertyWhenTheClassOfThePropertyTypeIsDecoratedWithEncryptAttribute()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer = new XmlSerializer<Container<EncryptedThing>>(x => x.WithEncryptionMechanism(encryptionMechanism));

            var instance = new Container<EncryptedThing>
            {
                Item = new EncryptedThing
                {
                    Foo = 123,
                    Bar = true
                }
            };

            var xml = serializer.Serialize(instance);

            var doc = XDocument.Parse(xml);

            Assert.That(encryptionMechanism.Decrypt(doc.Root.Element("Item").Value, null, _serializationState), Is.EqualTo("<Bar>true</Bar>"));
            Assert.That(encryptionMechanism.Decrypt(doc.Root.Element("Item").Attribute("Foo").Value, null, _serializationState), Is.EqualTo("123"));

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Item.Foo, Is.EqualTo(instance.Item.Foo));
            Assert.That(roundTrip.Item.Bar, Is.EqualTo(instance.Item.Bar));
        }

        [Test]
        public void EncryptsAndDecryptsGenericListPropertyWhenTheListGenericArgumentClassIsDecoratedWithEncryptAttribute()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer = new XmlSerializer<Container<List<EncryptedThing>>>(x => x.WithEncryptionMechanism(encryptionMechanism));

            var instance = new Container<List<EncryptedThing>>
            {
                Item = new List<EncryptedThing>
                {
                    new EncryptedThing
                    {
                        Foo = 123,
                        Bar = true
                    },
                    new EncryptedThing
                    {
                        Foo = 789,
                        Bar = false
                    },
                }
            };

            var xml = serializer.Serialize(instance);

            var doc = XDocument.Parse(xml);

            var actualDecryptedItemElementValue = encryptionMechanism.Decrypt(doc.Root.Element("Item").Value, null, _serializationState);
            const string expectedDecryptedItemElementValue =
                @"<EncryptedThing Foo=""123""><Bar>true</Bar></EncryptedThing>"
                + @"<EncryptedThing Foo=""789""><Bar>false</Bar></EncryptedThing>";

            Assert.That(actualDecryptedItemElementValue, Is.EqualTo(expectedDecryptedItemElementValue));

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Item.Count, Is.EqualTo(2));
            Assert.That(roundTrip.Item[0].Foo, Is.EqualTo(instance.Item[0].Foo));
            Assert.That(roundTrip.Item[0].Bar, Is.EqualTo(instance.Item[0].Bar));
            Assert.That(roundTrip.Item[1].Foo, Is.EqualTo(instance.Item[1].Foo));
            Assert.That(roundTrip.Item[1].Bar, Is.EqualTo(instance.Item[1].Bar));
        }

        [Test]
        public void EncryptsAndDecryptsGenericDictionaryPropertyWhenTheKeyClassIsDecoratedWithEncryptAttribute()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer = new XmlSerializer<Container<Dictionary<EncryptedThing, int>>>(x => x.WithEncryptionMechanism(encryptionMechanism));

            var instance = new Container<Dictionary<EncryptedThing, int>>
            {
                Item = new Dictionary<EncryptedThing, int>
                {
                    {
                        new EncryptedThing
                        {
                            Foo = 123,
                            Bar = true
                        },
                        1
                    },
                    {
                        new EncryptedThing
                        {
                            Foo = 789,
                            Bar = false
                        },
                        2
                    },
                }
            };

            var xml = serializer.Serialize(instance);

            var doc = XDocument.Parse(xml);

            var actualDecryptedItemElementValue = encryptionMechanism.Decrypt(doc.Root.Element("Item").Value, null, _serializationState);
            const string expectedDecryptedItemElementValue =
                @"<Item><Key Foo=""123""><Bar>true</Bar></Key><Value>1</Value></Item>"
                + @"<Item><Key Foo=""789""><Bar>false</Bar></Key><Value>2</Value></Item>";

            Assert.That(actualDecryptedItemElementValue, Is.EqualTo(expectedDecryptedItemElementValue));

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Item.Keys, Is.EquivalentTo(instance.Item.Keys));
            
            var key = new EncryptedThing { Foo = 123, Bar = true };
            Assert.That(roundTrip.Item[key], Is.EqualTo(instance.Item[key]));

            key = new EncryptedThing { Foo = 789, Bar = false };
            Assert.That(roundTrip.Item[key], Is.EqualTo(instance.Item[key]));
        }

        [Test]
        public void EncryptsAndDecryptsGenericDictionaryPropertyWhenTheValueClassIsDecoratedWithEncryptAttribute()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer = new XmlSerializer<Container<Dictionary<int, EncryptedThing>>>(x => x.WithEncryptionMechanism(encryptionMechanism));

            var instance = new Container<Dictionary<int, EncryptedThing>>
            {
                Item = new Dictionary<int, EncryptedThing>
                {
                    {
                        1,
                        new EncryptedThing
                        {
                            Foo = 123,
                            Bar = true
                        }
                    },
                    {
                        2,
                        new EncryptedThing
                        {
                            Foo = 789,
                            Bar = false
                        }
                    },
                }
            };

            var xml = serializer.Serialize(instance);

            var doc = XDocument.Parse(xml);

            var actualDecryptedItemElementValue = encryptionMechanism.Decrypt(doc.Root.Element("Item").Value, null, _serializationState);
            const string expectedDecryptedItemElementValue =
                @"<Item><Key>1</Key><Value Foo=""123""><Bar>true</Bar></Value></Item>"
                + @"<Item><Key>2</Key><Value Foo=""789""><Bar>false</Bar></Value></Item>";

            Assert.That(actualDecryptedItemElementValue, Is.EqualTo(expectedDecryptedItemElementValue));

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Item.Keys, Is.EquivalentTo(instance.Item.Keys));
            Assert.That(roundTrip.Item[1], Is.EqualTo(instance.Item[1]));
            Assert.That(roundTrip.Item[2], Is.EqualTo(instance.Item[2]));
        }

        [Encrypt]
        public class EncryptedThing
        {
            [XmlAttribute]
            public int Foo { get; set; }

            public bool Bar { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as EncryptedThing;

                if (other == null)
                {
                    return false;
                }

                return Foo == other.Foo && Bar == other.Bar;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Foo * 397) ^ Bar.GetHashCode();
                }
            }
        }

        public class UnencryptedThing
        {
            [XmlAttribute]
            public int Foo { get; set; }

            public bool Bar { get; set; }
        }

        [Test]
        public void AListPropertyDecoratedWithXmlElementAndEncryptCannotExistWithOtherNonXmlAttributeProperties()
        {
            Assert.That(() => new XmlSerializer<Invalid>(), Throws.InvalidOperationException);
        }

        [Test]
        public void AListPropertyDecoratedWithXmlElementAndEncryptCanExistWithOtherXmlAttributeProperties()
        {
            var serializer = new XmlSerializer<Valid>();

            var instance = new Valid
            {
                CleartextAttribute = 123,
                CiphertextAttribute = 789,
                Items = new List<int> { 4, 5, 6 }
            };

            var xml = serializer.Serialize(instance);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.CleartextAttribute, Is.EqualTo(instance.CleartextAttribute));
            Assert.That(roundTrip.CiphertextAttribute, Is.EqualTo(instance.CiphertextAttribute));
            Assert.That(roundTrip.Items, Is.EqualTo(instance.Items));
        }

        public class Invalid
        {
            [XmlElement("Item")]
            [Encrypt]
            public List<int> Items { get; set; }

            public int Wtf { get; set; }
        }

        public class Valid
        {
            [XmlElement("Item")]
            [Encrypt]
            public List<int> Items { get; set; }

            [XmlAttribute]
            public int CleartextAttribute { get; set; }

            [XmlAttribute]
            [Encrypt]
            public int CiphertextAttribute { get; set; }
        }

        #region ByXmlStructure

        [TestCaseSource("_byXmlStructureTestCases")]
        public void ByXmlStructure(object objectToSerialize, Func<object, object> getTargetValue, Func<XElement, object> getTargetNodeValue, object expectedTargetNodeValue, Type[] extraTypes)
        {
            PerformTest(objectToSerialize, getTargetValue, getTargetNodeValue, expectedTargetNodeValue, extraTypes);
        }

        // ReSharper disable PossibleNullReferenceException
        private static readonly TestCaseData[] _byXmlStructureTestCases =
        {
            GetTestCaseData(
                "XmlElement",
                new XmlElementExample { Value = 123 },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIz"),

            GetTestCaseData(
                "XmlAttribute",
                new XmlAttributeExample { Value = 123 },
                x => x.Value,
                root => root.Attribute("Value").Value,
                "MTIz"),

            GetTestCaseData(
                "XmlText",
                new Container<XmlTextExample> { Item = new XmlTextExample { Value = 123 } },
                x => x.Item.Value,
                root => root.Element("Item").Value,
                "MTIz"),

            GetTestCaseData(
                "XmlArray + XmlArrayItem",
                new XmlArrayAndItemExample { Items = new[] { 123 }},
                x => x.Items[0],
                root => root.Element("Items").Value,
                "PEl0ZW0+MTIzPC9JdGVtPg=="),

            GetTestCaseData(
                "XmlElement decorating array property",
                new XmlElementOnArrayPropertyExample { Items = new[] { 123 }},
                x => x.Items[0],
                root => root.Value,
                "PEl0ZW0+MTIzPC9JdGVtPg==")
        };
        // ReSharper restore PossibleNullReferenceException

        public class XmlElementExample
        {
            [Encrypt]
            [XmlElement]
            public int Value { get; set; }

            public int Dummy { get; set; }
        }

        public class XmlAttributeExample
        {
            [Encrypt]
            [XmlAttribute]
            public int Value { get; set; }

            public int Dummy { get; set; }
        }

        public class XmlTextExample
        {
            [Encrypt]
            [XmlText]
            public int Value { get; set; }
        }

        public class XmlArrayAndItemExample
        {
            [Encrypt]
            [XmlArray]
            [XmlArrayItem("Item")]
            public int[] Items { get; set; }

            public int Dummy { get; set; }
        }

        public class XmlElementOnArrayPropertyExample
        {
            [Encrypt]
            [XmlElement("Item")]
            public int[] Items { get; set; }
        }

        #endregion

        #region ByType

        [TestCaseSource("_byTypeTestCases")]
        public void ByType(object objectToSerialize, Func<object, object> getTargetValue, Func<XElement, object> getTargetNodeValue, object expectedTargetNodeValue, Type[] extraTypes)
        {
            PerformTest(objectToSerialize, getTargetValue, getTargetNodeValue, expectedTargetNodeValue, extraTypes);
        }

        private static readonly Guid _exampleGuid = Guid.Parse("e3287204-92ab-4a54-a148-f554007beddd");

        // ReSharper disable PossibleNullReferenceException
        private static readonly TestCaseData[] _byTypeTestCases =
        {
            GetTestCaseData(
                "string",
                new TypeExample<string> { Value = "123" },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIz"),

            GetTestCaseData(
                "int",
                new TypeExample<int> { Value = 123 },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIz"),

            GetTestCaseData(
                "Nullable int",
                new TypeExample<int?> { Value = 123 },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIz"),

            GetTestCaseData(
                "bool",
                new TypeExample<bool> { Value = true },
                x => x.Value,
                root => root.Element("Value").Value,
                "dHJ1ZQ=="),

            GetTestCaseData(
                "Nullable bool",
                new TypeExample<bool?> { Value = true },
                x => x.Value,
                root => root.Element("Value").Value,
                "dHJ1ZQ=="),

            GetTestCaseData(
                "double",
                new TypeExample<double> { Value = 123.45 },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIzLjQ1"),

            GetTestCaseData(
                "Nullable double",
                new TypeExample<double?> { Value = 123.45 },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIzLjQ1"),

            GetTestCaseData(
                "decimal",
                new TypeExample<decimal> { Value = 123.45M },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIzLjQ1"),

            GetTestCaseData(
                "Nullable decimal",
                new TypeExample<decimal?> { Value = 123.45M },
                x => x.Value,
                root => root.Element("Value").Value,
                "MTIzLjQ1"),

            GetTestCaseData(
                "enum (specific)",
                new TypeExample<TestEnum> { Value = TestEnum.Second },
                x => x.Value,
                root => root.Element("Value").Value,
                "U2Vjb25k"),

            GetTestCaseData(
                "Nullable enum (specific)",
                new TypeExample<TestEnum?> { Value = TestEnum.Second },
                x => x.Value,
                root => root.Element("Value").Value,
                "U2Vjb25k"),

            GetTestCaseData(
                "DateTime",
                new TypeExample<DateTime> { Value = new DateTime(2000, 1, 1) },
                x => x.Value,
                root => root.Element("Value").Value,
                "MjAwMC0wMS0wMVQwMDowMDowMC4wMDAwMDAw"),

            GetTestCaseData(
                "Nullable DateTime",
                new TypeExample<DateTime?> { Value = new DateTime(2000, 1, 1) },
                x => x.Value,
                root => root.Element("Value").Value,
                "MjAwMC0wMS0wMVQwMDowMDowMC4wMDAwMDAw"),

            GetTestCaseData(
                "Guid",
                new TypeExample<Guid> { Value = _exampleGuid },
                x => x.Value,
                root => root.Element("Value").Value,
                "ZTMyODcyMDQtOTJhYi00YTU0LWExNDgtZjU1NDAwN2JlZGRk"),

            GetTestCaseData(
                "Nullable Guid",
                new TypeExample<Guid?> { Value = _exampleGuid },
                x => x.Value,
                root => root.Element("Value").Value,
                "ZTMyODcyMDQtOTJhYi00YTU0LWExNDgtZjU1NDAwN2JlZGRk"),

            GetTestCaseData(
                "Enum (non-specific)",
                new TypeExample<Enum> { Value = TestEnum.Second },
                x => x.Value,
                root => root.Element("Value").Value,
                "VGVzdEVudW0uU2Vjb25k",
                typeof(TestEnum)),

            GetTestCaseData(
                "Type",
                new TypeExample<Type> { Value = typeof(string) },
                x => x.Value,
                root => root.Element("Value").Value,
                "U3lzdGVtLlN0cmluZw=="),

            GetTestCaseData(
                "Uri",
                new TypeExample<Uri> { Value = new Uri("http://google.com/") },
                x => x.Value,
                root => root.Element("Value").Value,
                "aHR0cDovL2dvb2dsZS5jb20v"),

            GetTestCaseData(
                "Complex Object With Elements",
                new TypeExample<Bar> { Value = new Bar { Baz = 123, Qux = true } },
                x => x.Value,
                root => root.Element("Value").Value,
                "PEJhej4xMjM8L0Jhej48UXV4PnRydWU8L1F1eD4="),

            GetTestCaseData(
                "Complex Object With Elements And Non-Default Constructor",
                new TypeExample<BarImmutable> { Value = new BarImmutable(123, true) },
                x => x.Value,
                root => root.Element("Value").Value,
                "PEJhej4xMjM8L0Jhej48UXV4PnRydWU8L1F1eD4="),

            GetTestCaseData(
                "Complex Object With Attributes",
                new TypeExample<Baz> { Value = new Baz { Qux = 123, Corge = true } },
                x => x.Value,
                root => root.Element("Value").Attribute("Qux").Value + "|" + root.Element("Value").Attribute("Corge").Value,
                "MTIz|dHJ1ZQ=="),

            GetTestCaseData(
                "Complex Object With Attributes And Non-Default Constructor",
                new TypeExample<BazImmutable> { Value = new BazImmutable(123, true) },
                x => x.Value,
                root => root.Element("Value").Attribute("Qux").Value + "|" + root.Element("Value").Attribute("Corge").Value,
                "MTIz|dHJ1ZQ=="),

            GetTestCaseData(
                "Complex Object With Elements And Attributes",
                new TypeExample<Foo> { Value = new Foo { Bar = 123, Baz = true, Qux = 123, Corge = true } },
                x => x.Value,
                root => root.Element("Value").Value + "|" + root.Element("Value").Attribute("Qux").Value + "|" + root.Element("Value").Attribute("Corge").Value,
                "PEJhcj4xMjM8L0Jhcj48QmF6PnRydWU8L0Jhej4=|MTIz|dHJ1ZQ=="),

            GetTestCaseData(
                "Complex Object With Elements And Attributes And Non-Default Constructor",
                new TypeExample<FooImmutable> { Value = new FooImmutable(123, true, 123, true) },
                x => x.Value,
                root => root.Element("Value").Value + "|" + root.Element("Value").Attribute("Qux").Value + "|" + root.Element("Value").Attribute("Corge").Value,
                "PEJhcj4xMjM8L0Jhcj48QmF6PnRydWU8L0Jhej4=|MTIz|dHJ1ZQ=="),

            GetTestCaseData(
                "Dictionary",
                new TypeExample<Dictionary<int, string>> { Value = new Dictionary<int, string> { { 123, "abc"}, { 789, "xyz" } } },
                x => x.Value,
                root => root.Element("Value").Value,
                "PEl0ZW0+PEtleT4xMjM8L0tleT48VmFsdWU+YWJjPC9WYWx1ZT48L0l0ZW0+PEl0ZW0+PEtleT43ODk8L0tleT48VmFsdWU+eHl6PC9WYWx1ZT48L0l0ZW0+"),

            GetTestCaseData(
                "Dynamic With ExpandoObject Input",
                new TypeExample<dynamic> { Value = GetExampleExpandoObject() },
                x => x.Value,
                root => root.Element("Value").Value,
                "PEZvbz4xMjM8L0Zvbz48QmFyPmFiYzwvQmFyPjxCYXo+dHJ1ZTwvQmF6Pg=="),

            GetTestCaseData(
                "Dynamic With Anonymous Object Input",
                new TypeExample<dynamic> { Value = new { Foo = 123, Bar = "abc", Baz = true } },
                x => x.Value is ExpandoObject ? x.Value : ToExpandoObject(x.Value),
                root => root.Element("Value").Value,
                "PEZvbz4xMjM8L0Zvbz48QmFyPmFiYzwvQmFyPjxCYXo+dHJ1ZTwvQmF6Pg=="),

            GetTestCaseData(
                "Dynamic With Concrete Object Input",
                new TypeExample<dynamic> { Value = new Example { Foo = 123, Bar = "abc", Baz = true } },
                x => x.Value,
                root => root.Element("Value").Value,
                "PEZvbz4xMjM8L0Zvbz48QmFyPmFiYzwvQmFyPjxCYXo+dHJ1ZTwvQmF6Pg=="),

            GetTestCaseData(
                "ExpandoObject",
                new TypeExample<ExpandoObject> { Value = GetExampleExpandoObject() },
                x => x.Value,
                root => root.Element("Value").Value,
                "PEZvbz4xMjM8L0Zvbz48QmFyPmFiYzwvQmFyPjxCYXo+dHJ1ZTwvQmF6Pg=="),
        };

        private static ExpandoObject GetExampleExpandoObject()
        {
            var expandoObject = new ExpandoObject();
            dynamic d = expandoObject;

            d.Foo = 123;
            d.Bar = "abc";
            d.Baz = true;

            return expandoObject;
        }

        private static object ToExpandoObject(object anonymousObject)
        {
            IDictionary<string, object> expndoObject = new ExpandoObject();

            foreach (var property in anonymousObject.GetType().GetProperties())
            {
                expndoObject[property.Name] = property.GetValue(anonymousObject);
            }

            return expndoObject;
        }

        public class TypeExample<T>
        {
            [Encrypt]
            public T Value { get; set; }

            public int Dummy { get; set; }
        }

        public enum TestEnum
        {
            First, Second, Third
        }

        public class Bar
        {
            public int Baz { get; set; }
            public bool Qux { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;

                var expando = (IDictionary<string, object>)(obj as ExpandoObject);
                if (expando != null)
                {
                    object value;
                    return
                        (expando.TryGetValue("Baz", out value) && value is int && (int)value == Baz)
                        && (expando.TryGetValue("Qux", out value) && value is bool && (bool)value == Qux);
                }

                if (obj.GetType() != this.GetType()) return false;
                return Equals((Bar)obj);
            }

            protected bool Equals(Bar other)
            {
                return Baz == other.Baz && Qux.Equals(other.Qux);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Baz * 397) ^ Qux.GetHashCode();
                }
            }
        }

        public class BarImmutable
        {
            private readonly int _baz;
            private readonly bool _qux;

            public BarImmutable(int baz, bool qux)
            {
                _baz = baz;
                _qux = qux;
            }

            public int Baz { get { return _baz; } }
            public bool Qux { get { return _qux; } }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((BarImmutable)obj);
            }

            protected bool Equals(BarImmutable other)
            {
                return Baz == other.Baz && Qux.Equals(other.Qux);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Baz * 397) ^ Qux.GetHashCode();
                }
            }
        }

        public class Baz
        {
            [XmlAttribute]
            public int Qux { get; set; }
            [XmlAttribute]
            public bool Corge { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Baz)obj);
            }

            protected bool Equals(Baz other)
            {
                return Qux == other.Qux && Corge.Equals(other.Corge);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Qux * 397) ^ Corge.GetHashCode();
                }
            }
        }

        public class BazImmutable
        {
            private readonly int _qux;
            private readonly bool _corge;

            public BazImmutable(int qux, bool corge)
            {
                _qux = qux;
                _corge = corge;
            }

            [XmlAttribute]
            public int Qux { get { return _qux; } }
            [XmlAttribute]
            public bool Corge { get { return _corge; } }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((BazImmutable)obj);
            }

            protected bool Equals(BazImmutable other)
            {
                return Qux == other.Qux && Corge.Equals(other.Corge);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Qux * 397) ^ Corge.GetHashCode();
                }
            }
        }

        public class Foo
        {
            public double Bar { get; set; }
            public bool Baz { get; set; }
            [XmlAttribute]
            public int Qux { get; set; }
            [XmlAttribute]
            public bool Corge { get; set; }

            protected bool Equals(Foo other)
            {
                return Bar.Equals(other.Bar) && Baz.Equals(other.Baz) && Qux == other.Qux && Corge.Equals(other.Corge);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Foo)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Bar.GetHashCode();
                    hashCode = (hashCode * 397) ^ Baz.GetHashCode();
                    hashCode = (hashCode * 397) ^ Qux;
                    hashCode = (hashCode * 397) ^ Corge.GetHashCode();
                    return hashCode;
                }
            }
        }

        public class FooImmutable
        {
            private readonly int _bar;
            private readonly bool _baz;
            private readonly int _qux;
            private readonly bool _corge;

            public FooImmutable(int bar, bool baz, int qux, bool corge)
            {
                _bar = bar;
                _baz = baz;
                _qux = qux;
                _corge = corge;
            }

            public int Bar { get { return _bar; } }
            public bool Baz { get { return _baz; } }
            [XmlAttribute]
            public int Qux { get { return _qux; } }
            [XmlAttribute]
            public bool Corge { get { return _corge; } }

            protected bool Equals(FooImmutable other)
            {
                return Bar.Equals(other.Bar) && Baz.Equals(other.Baz) && Qux == other.Qux && Corge.Equals(other.Corge);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((FooImmutable)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Bar.GetHashCode();
                    hashCode = (hashCode * 397) ^ Baz.GetHashCode();
                    hashCode = (hashCode * 397) ^ Qux;
                    hashCode = (hashCode * 397) ^ Corge.GetHashCode();
                    return hashCode;
                }
            }
        }

        public class Example
        {
            public int Foo { get; set; }
            public string Bar { get; set; }
            public bool Baz { get; set; }

            protected bool Equals(Example other)
            {
                return Foo == other.Foo && string.Equals(Bar, other.Bar) && Baz.Equals(other.Baz);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Example)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Foo;
                    hashCode = (hashCode * 397) ^ (Bar != null ? Bar.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ Baz.GetHashCode();
                    return hashCode;
                }
            }
        }

        #endregion

        private static void PerformTest(object objectToSerialize, Func<object, object> getTargetValue, Func<XElement, object> getTargetNodeValue, object expectedTargetNodeValue, Type[] extraTypes)
        {
            var serializer = XmlSerializer.Create(objectToSerialize.GetType(), options => options.Indent().AlwaysEmitTypes(), extraTypes);

            var xml = serializer.Serialize(objectToSerialize);
            Console.WriteLine(xml);

            var roundTrip = serializer.Deserialize(xml);

            var targetValue = getTargetValue(roundTrip);
            var expectedTargetValue = getTargetValue(objectToSerialize);

            Assert.That(targetValue, Is.EqualTo(expectedTargetValue));

            var targetNodeValue = getTargetNodeValue(XDocument.Parse(xml).Root);

            Assert.That(targetNodeValue, Is.EqualTo(expectedTargetNodeValue));
        }

        private static TestCaseData GetTestCaseData<TToSerialize, TTargetValue>(
            string name,
            TToSerialize objectToSerialize,
            Func<TToSerialize, TTargetValue> getTargetValue,
            Func<XElement, object> getTargetNodeValue,
            object expectedTargetNodeValue,
            params Type[] extraTypes)
        {
            var testCaseData =
                new TestCaseData(
                    objectToSerialize,
                    (Func<object, object>)(target => getTargetValue((TToSerialize)target)),
                    getTargetNodeValue,
                    expectedTargetNodeValue,
                    extraTypes);

            return testCaseData.SetName(name);
        }

        public class Container<T>
        {
            public T Item { get; set; }
        }
    }
}