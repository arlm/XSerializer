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
                root => root.Element("Items").Element("Item").Value,
                "MTIz"),

            GetTestCaseData(
                "XmlElement decorating array property",
                new XmlElementOnArrayPropertyExample { Items = new[] { 123 }},
                x => x.Items[0],
                root => root.Element("Item").Value,
                "MTIz")
        };
        // ReSharper restore PossibleNullReferenceException

        public class XmlElementExample
        {
            [Encrypt]
            [XmlElement]
            public int Value { get; set; }
        }

        public class XmlAttributeExample
        {
            [Encrypt]
            [XmlAttribute]
            public int Value { get; set; }
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