using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests.Encryption
{
    [TestFixture]
    public class EncryptionTests
    {
        private IEncryptionProvider _previousEncryptionProvider;

        [TestFixtureSetUp]
        public void Setup()
        {
            _previousEncryptionProvider = EncryptionProvider.Current;
            EncryptionProvider.Current = new TestEncryptionProvider();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            EncryptionProvider.Current = _previousEncryptionProvider;
        }

        #region ByXmlStructure

        [TestCaseSource("_byXmlStructureTestCases")]
        public void ByXmlStructure(object objectToSerialize, Func<XElement, object> getTargetNodeValue, object expectedTargetNodeValue, Func<object, object> getTargetValue, Type[] extraTypes)
        {
            PerformTest(objectToSerialize, getTargetNodeValue, expectedTargetNodeValue, getTargetValue, extraTypes);
        }

        // ReSharper disable PossibleNullReferenceException
        private static readonly TestCaseData[] _byXmlStructureTestCases =
        {
            GetTestCaseData(
                "XmlElement",
                new XmlElementExample { Value = 123 },
                root => root.Element("Value").Value,
                "MTIz",
                x => x.Value),

            GetTestCaseData(
                "XmlAttribute",
                new XmlAttributeExample { Value = 123 },
                root => root.Attribute("Value").Value,
                "MTIz",
                x => x.Value),

            GetTestCaseData(
                "XmlText",
                new Container<XmlTextExample> { Item = new XmlTextExample { Value = 123 } },
                root => root.Element("Item").Value,
                "MTIz",
                x => x.Item.Value),

            GetTestCaseData(
                "XmlArray + XmlArrayItem",
                new XmlArrayAndItemExample { Items = new[] { 123 }},
                root => root.Element("Items").Element("Item").Value,
                "MTIz",
                x => x.Items[0]),

            GetTestCaseData(
                "XmlElement decorating array property",
                new XmlElementOnArrayPropertyExample { Items = new[] { 123 }},
                root => root.Element("Item").Value,
                "MTIz",
                x => x.Items[0]),
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
        public void ByType(object objectToSerialize, Func<XElement, object> getTargetNodeValue, object expectedTargetNodeValue, Func<object, object> getTargetValue, Type[] extraTypes)
        {
            PerformTest(objectToSerialize, getTargetNodeValue, expectedTargetNodeValue, getTargetValue, extraTypes);
        }

        private static readonly Guid _exampleGuid = Guid.Parse("e3287204-92ab-4a54-a148-f554007beddd");

        // ReSharper disable PossibleNullReferenceException
        private static readonly TestCaseData[] _byTypeTestCases =
        {
            GetTestCaseData(
                "int",
                new TypeExample<int> { Value = 123 },
                root => root.Element("Value").Value,
                "MTIz",
                x => x.Value),

            GetTestCaseData(
                "Nullable int",
                new TypeExample<int?> { Value = 123 },
                root => root.Element("Value").Value,
                "MTIz",
                x => x.Value),

            GetTestCaseData(
                "bool",
                new TypeExample<bool> { Value = true },
                root => root.Element("Value").Value,
                "dHJ1ZQ==",
                x => x.Value),

            GetTestCaseData(
                "Nullable bool",
                new TypeExample<bool?> { Value = true },
                root => root.Element("Value").Value,
                "dHJ1ZQ==",
                x => x.Value),

                GetTestCaseData(
                "enum",
                new TypeExample<TestEnum> { Value = TestEnum.Second },
                root => root.Element("Value").Value,
                "U2Vjb25k",
                x => x.Value),

                GetTestCaseData(
                "Nullable enum",
                new TypeExample<TestEnum?> { Value = TestEnum.Second },
                root => root.Element("Value").Value,
                "U2Vjb25k",
                x => x.Value),

                GetTestCaseData(
                "DateTime",
                new TypeExample<DateTime> { Value = new DateTime(2000, 1, 1) },
                root => root.Element("Value").Value,
                "MjAwMC0wMS0wMVQwMDowMDowMC4wMDAwMDAw",
                x => x.Value),

                GetTestCaseData(
                "Nullable DateTime",
                new TypeExample<DateTime?> { Value = new DateTime(2000, 1, 1) },
                root => root.Element("Value").Value,
                "MjAwMC0wMS0wMVQwMDowMDowMC4wMDAwMDAw",
                x => x.Value),

                GetTestCaseData(
                "Guid",
                new TypeExample<Guid> { Value = _exampleGuid },
                root => root.Element("Value").Value,
                "ZTMyODcyMDQtOTJhYi00YTU0LWExNDgtZjU1NDAwN2JlZGRk",
                x => x.Value),

                GetTestCaseData(
                "Nullable Guid",
                new TypeExample<Guid?> { Value = _exampleGuid },
                root => root.Element("Value").Value,
                "ZTMyODcyMDQtOTJhYi00YTU0LWExNDgtZjU1NDAwN2JlZGRk",
                x => x.Value),

                GetTestCaseData(
                "Enum (non-specific)",
                new TypeExample<Enum> { Value = TestEnum.Second },
                root => root.Element("Value").Value,
                "VGVzdEVudW0uU2Vjb25k",
                x => x.Value,
                typeof(TestEnum)),

                GetTestCaseData(
                "Type",
                new TypeExample<Type> { Value = typeof(string) },
                root => root.Element("Value").Value,
                "U3lzdGVtLlN0cmluZw==",
                x => x.Value,
                typeof(TestEnum)),

                GetTestCaseData(
                "Uri",
                new TypeExample<Uri> { Value = new Uri("http://google.com/") },
                root => root.Element("Value").Value,
                "aHR0cDovL2dvb2dsZS5jb20v",
                x => x.Value,
                typeof(TestEnum)),
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

        private static void PerformTest(
            object objectToSerialize,
            Func<XElement, object> getTargetNodeValue,
            object expectedTargetNodeValue,
            Func<object, object> getTargetValue,
            Type[] extraTypes)
        {
            var serializer = XmlSerializer.Create(objectToSerialize.GetType(), extraTypes);

            var xml = serializer.Serialize(objectToSerialize);

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
            Func<XElement, object> getTargetNodeValue,
            object expectedTargetNodeValue,
            Func<TToSerialize, TTargetValue> getTargetValue,
            params Type[] extraTypes)
        {
            var testCaseData =
                new TestCaseData(
                    objectToSerialize,
                    getTargetNodeValue,
                    expectedTargetNodeValue,
                    (Func<object, object>)(target => getTargetValue((TToSerialize)target)),
                    extraTypes);

            return testCaseData.SetName(name);
        }

        public class Container<T>
        {
            public T Item { get; set; }
        }

        private class TestEncryptionProvider : IEncryptionProvider
        {
            public string Encrypt(string plainText)
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
            }

            public string Decrypt(string cipherText)
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
            }
        }
    }
}