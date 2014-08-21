using System;
using System.Globalization;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class RedactTestsForXmlAttributes
    {
        private XmlSerializer<RedactTestClassForXmlAttributeContainer> _serializer;
        private RedactTestClassForXmlAttributeContainer _container;
        private RedactTestClassForXmlAttribute _testClass;

        [SetUp]
        public void Setup()
        {
            _serializer = new XmlSerializer<RedactTestClassForXmlAttributeContainer>();
            _container = new RedactTestClassForXmlAttributeContainer();
            _testClass = new RedactTestClassForXmlAttribute();
            _container.Data = _testClass;
        }

        [TestCase("abc123", "XXX111")]
        [TestCase("123-45-6789", "111-11-1111")]
        [TestCase("123 Main Street", "111 XXXX XXXXXX")]
        [TestCase("", "")]
        public void StringAtrributesRedactCorrectly(string value, string expectedAttributeValue)
        {
            _testClass.StringAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("StringAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [Test]
        public void StringAttributeWithNullValueDoesNotSerialize()
        {
            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Is.Not.StringContaining("StringAttributeProperty"));
        }

        [Test]
        public void RedactedStringAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data StringAttributeProperty=""XXX111"" />
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.StringAttributeProperty, Is.EqualTo("XXX111"));
        }

        [TestCase(true, "XXXXXX")]
        [TestCase(false, "XXXXXX")]
        public void NullableBoolAtrributesRedactCorrectly(bool value, string expectedAttributeValue)
        {
            _testClass.NullableBoolAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("NullableBoolAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [TestCase(true, "XXXXXX")]
        [TestCase(false, "XXXXXX")]
        public void BoolAtrributesRedactCorrectly(bool value, string expectedAttributeValue)
        {
            _testClass.BoolAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("BoolAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [Test]
        public void NullableBoolAttributeWithNullValueDoesNotSerialize()
        {
            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Is.Not.StringContaining("NullableBoolAttributeProperty"));
        }

        [Test]
        public void RedactedNullableBoolAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data NullableBoolAttributeProperty=""XXXXXX""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.NullableBoolAttributeProperty, Is.Null);
        }

        [Test]
        public void RedactedBoolAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data BoolAttributeProperty=""XXXXXX""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.BoolAttributeProperty, Is.False);
        }

        [TestCase(RedactEnumForXmlAttribute.First, "XXXXXXXXXX")]
        [TestCase(RedactEnumForXmlAttribute.Second, "XXXXXXXXXX")]
        [TestCase(RedactEnumForXmlAttribute.Third, "XXXXXXXXXX")]
        public void NullableEnumAtrributesRedactCorrectly(RedactEnumForXmlAttribute value, string expectedAttributeValue)
        {
            _testClass.NullableEnumAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("NullableEnumAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [TestCase(RedactEnumForXmlAttribute.First, "XXXXXXXXXX")]
        [TestCase(RedactEnumForXmlAttribute.Second, "XXXXXXXXXX")]
        [TestCase(RedactEnumForXmlAttribute.Third, "XXXXXXXXXX")]
        public void EnumAtrributesRedactCorrectly(RedactEnumForXmlAttribute value, string expectedAttributeValue)
        {
            _testClass.EnumAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("EnumAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [Test]
        public void NullableEnumAttributeWithNullValueDoesNotSerialize()
        {
            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Is.Not.StringContaining("NullableEnumAttributeProperty"));
        }

        [Test]
        public void RedactedNullableEnumAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data NullableEnumAttributeProperty=""XXXXXX""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.NullableEnumAttributeProperty, Is.Null);
        }

        [Test]
        public void RedactedEnumAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data EnumAttributeProperty=""XXXXXX""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.EnumAttributeProperty, Is.EqualTo(default(RedactEnumForXmlAttribute)));
        }

        [Test]
        public void NullableDateTimeAtrributesWithLocalValuesRedactCorrectly()
        {
            var value = DateTime.Now;

            var expectedOffsetSign =
                TimeZone.CurrentTimeZone.GetUtcOffset(value) < TimeSpan.Zero
                ? "-"
                : "+";

            _testClass.NullableDateTimeAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("NullableDateTimeAttributeProperty=\"1111-11-11T11:11:11.1111111{0}11:11\"", expectedOffsetSign)));
        }

        [Test]
        public void NullableDateTimeAtrributesWithUtcValuesRedactCorrectly()
        {
            var value = DateTime.UtcNow;

            _testClass.NullableDateTimeAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring("NullableDateTimeAttributeProperty=\"1111-11-11T11:11:11.1111111Z\""));
        }

        [Test]
        public void DateTimeAtrributesWithLocalValuesRedactCorrectly()
        {
            var value = DateTime.Now;

            var expectedOffsetSign =
                TimeZone.CurrentTimeZone.GetUtcOffset(value) < TimeSpan.Zero
                ? "-"
                : "+";

            _testClass.DateTimeAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("DateTimeAttributeProperty=\"1111-11-11T11:11:11.1111111{0}11:11\"", expectedOffsetSign)));
        }

        [Test]
        public void DateTimeAtrributesWithUtcValuesRedactCorrectly()
        {
            var value = DateTime.UtcNow;

            _testClass.DateTimeAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring("TimeAttributeProperty=\"1111-11-11T11:11:11.1111111Z\""));
        }

        [Test]
        public void NullableDateTimeAttributeWithNullValueDoesNotSerialize()
        {
            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Is.Not.StringContaining("NullableDateTimeAttributeProperty"));
        }

        [Test]
        public void RedactedDateTimeAtrributesFromLocalValueDeserializesCorrectly()
        {
            var expectedDateTime =
                DateTime.ParseExact(
                    "1111-11-11T11:11:11.1111111-04:00",
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data DateTimeAttributeProperty=""1111-11-11T11:11:11.1111111-04:00""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.DateTimeAttributeProperty, Is.EqualTo(expectedDateTime));
        }

        [Test]
        public void RedactedNullableDateTimeAtrributesFromLocalValueDeserializesCorrectly()
        {
            var expectedDateTime =
                DateTime.ParseExact(
                    "1111-11-11T11:11:11.1111111-04:00",
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data NullableDateTimeAttributeProperty=""1111-11-11T11:11:11.1111111-04:00""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.NullableDateTimeAttributeProperty, Is.EqualTo(expectedDateTime));
        }

        [Test]
        public void RedactedDateTimeAtrributesFromUtcValueDeserializesCorrectly()
        {
            var expectedDateTime =
                DateTime.ParseExact(
                    "1111-11-11T11:11:11.1111111Z",
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data DateTimeAttributeProperty=""1111-11-11T11:11:11.1111111Z""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.DateTimeAttributeProperty, Is.EqualTo(expectedDateTime));
        }

        [Test]
        public void RedactedNullableDateTimeAtrributesFromUtcValueDeserializesCorrectly()
        {
            var expectedDateTime =
                DateTime.ParseExact(
                    "1111-11-11T11:11:11.1111111Z",
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data NullableDateTimeAttributeProperty=""1111-11-11T11:11:11.1111111Z""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.NullableDateTimeAttributeProperty, Is.EqualTo(expectedDateTime));
        }

        [TestCase(123, "111")]
        [TestCase(-123, "1111")]
        public void NullableIntAtrributesRedactCorrectly(int value, string expectedAttributeValue)
        {
            _testClass.NullableIntAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("NullableIntAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [TestCase(123, "111")]
        [TestCase(-123, "1111")]
        public void IntAtrributesRedactCorrectly(int value, string expectedAttributeValue)
        {
            _testClass.IntAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("IntAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [Test]
        public void NullableIntAttributeWithNullValueDoesNotSerialize()
        {
            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Is.Not.StringContaining("NullableIntAttributeProperty"));
        }

        [Test]
        public void RedactedNullableIntAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data NullableIntAttributeProperty=""11111""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.NullableIntAttributeProperty, Is.EqualTo(11111));
        }

        [Test]
        public void RedactedIntAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data IntAttributeProperty=""11111""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.IntAttributeProperty, Is.EqualTo(11111));
        }

        [TestCase(123, "111")]
        [TestCase(-123, "1111")]
        [TestCase(123.45, "111111")]
        [TestCase(-123.45, "1111111")]
        public void NullableDoubleAtrributesRedactCorrectly(double value, string expectedAttributeValue)
        {
            _testClass.NullableDoubleAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("NullableDoubleAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [TestCase(123, "111")]
        [TestCase(-123, "1111")]
        [TestCase(123.45, "111111")]
        [TestCase(-123.45, "1111111")]
        public void DoubleAtrributesRedactCorrectly(double value, string expectedAttributeValue)
        {
            _testClass.DoubleAttributeProperty = value;

            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Contains.Substring(string.Format("DoubleAttributeProperty=\"{0}\"", expectedAttributeValue)));
        }

        [Test]
        public void NullableDoubleAttributeWithNullValueDoesNotSerialize()
        {
            var xml = Serialize();
            Console.WriteLine(xml);

            Assert.That(xml, Is.Not.StringContaining("NullableDoubleAttributeProperty"));
        }

        [Test]
        public void RedactedNullableDoubleAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data NullableDoubleAttributeProperty=""11111""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.NullableDoubleAttributeProperty, Is.EqualTo(11111));
        }

        [Test]
        public void RedactedDoubleAttributeDeserializesCorrectly()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RedactTestClassForXmlAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <Data DoubleAttributeProperty=""11111""/>
</RedactTestClassForXmlAttributeContainer>";

            var instance = _serializer.Deserialize(xml);

            Assert.That(instance.Data.DoubleAttributeProperty, Is.EqualTo(11111));
        }

        [Test]
        public void RedactingDoesNotHappenWhenDisabled()
        {
            var container = new RedactTestClassForXmlAttributeContainer
            {
                Data = new RedactTestClassForXmlAttribute
                {
                    BoolAttributeProperty = true,
                    DateTimeAttributeProperty = new DateTime(2222, 2, 22, 22, 22, 22, 222, DateTimeKind.Utc),
                    DoubleAttributeProperty = 123.45,
                    EnumAttributeProperty = RedactEnumForXmlAttribute.Second,
                    IntAttributeProperty = 12345,
                    NullableBoolAttributeProperty = false,
                    NullableDateTimeAttributeProperty = new DateTime(3333, 3, 30, 3, 33, 33, 333, DateTimeKind.Utc),
                    NullableDoubleAttributeProperty = 987.65,
                    NullableEnumAttributeProperty = RedactEnumForXmlAttribute.Third,
                    NullableIntAttributeProperty = 98765,
                    StringAttributeProperty = "abc123"
                }
            };

            var serializer = new XmlSerializer<RedactTestClassForXmlAttributeContainer>(options => options.DisableRedact());

            var xml = serializer.Serialize(container);

            Assert.That(xml, Contains.Substring("true"));
            Assert.That(xml, Contains.Substring("2222-02-22T22:22:22.2220000Z"));
            Assert.That(xml, Contains.Substring("123.45"));
            Assert.That(xml, Contains.Substring("Second"));
            Assert.That(xml, Contains.Substring("12345"));
            Assert.That(xml, Contains.Substring("false"));
            Assert.That(xml, Contains.Substring("3333-03-30T03:33:33.3330000Z"));
            Assert.That(xml, Contains.Substring("987.65"));
            Assert.That(xml, Contains.Substring("Third"));
            Assert.That(xml, Contains.Substring("98765"));
            Assert.That(xml, Contains.Substring("abc123"));
        }

        private string Serialize()
        {
            return _serializer.Serialize(_container);
        }

        public class RedactTestClassForXmlAttributeContainer
        {
            public RedactTestClassForXmlAttribute Data { get; set; }
        }

        public class RedactTestClassForXmlAttribute
        {
            [Redact]
            [XmlAttribute]
            public string StringAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public bool? NullableBoolAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public RedactEnumForXmlAttribute? NullableEnumAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public DateTime? NullableDateTimeAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public int? NullableIntAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public double? NullableDoubleAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public bool BoolAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public RedactEnumForXmlAttribute EnumAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public DateTime DateTimeAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public int IntAttributeProperty { get; set; }

            [Redact]
            [XmlAttribute]
            public double DoubleAttributeProperty { get; set; }
        }

        public enum RedactEnumForXmlAttribute
        {
            First,
            Second,
            Third
        }
    } 
}