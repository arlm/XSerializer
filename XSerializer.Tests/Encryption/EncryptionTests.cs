using System;
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
                "aHR0cDovL2dvb2dsZS5jb20v")
        };

        public class TypeExample<T>
        {
            [Encrypt]
            public T Value { get; set; }
        }

        public enum TestEnum
        {
            First, Second, Third
        }

        #endregion

        private static void PerformTest(object objectToSerialize, Func<object, object> getTargetValue, Func<XElement, object> getTargetNodeValue, object expectedTargetNodeValue, Type[] extraTypes)
        {
            var serializer = XmlSerializer.Create(objectToSerialize.GetType(), options => options.Indent(), extraTypes);

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