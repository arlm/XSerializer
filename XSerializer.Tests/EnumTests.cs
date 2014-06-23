using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class EnumTests
    {
        [Test]
        public void EnumElementSerializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var container = new EnumElementContainer
            {
                MyEnum = MyEnum.Value2
            };

            var serializer = new CustomSerializer<EnumElementContainer>(TestXmlSerializerOptions.WithExtraTypes(typeof(IFoo)));

            var xml = serializer.Serialize(container, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions());

            Assert.That(xml, Contains.Substring("Value2"));
        }

        [Test]
        public void EnumAttributeSerializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var container = new EnumAttributeContainer
            {
                MyEnum = MyEnum.Value2
            };

            var serializer = new CustomSerializer<EnumAttributeContainer>(TestXmlSerializerOptions.WithExtraTypes(typeof(IFoo)));

            var xml = serializer.Serialize(container, Encoding.UTF8, Formatting.Indented, new TestSerializeOptions());

            Assert.That(xml, Contains.Substring("Value2"));
        }

        [Test]
        public void EnumElementDeserializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<EnumElementContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <MyEnum>Value2</MyEnum>
</EnumElementContainer>";

            var serializer = new CustomSerializer<EnumElementContainer>(TestXmlSerializerOptions.WithExtraTypes(typeof(IFoo)));

            var container = serializer.Deserialize(xml);

            Assert.That(container.MyEnum, Is.EqualTo(MyEnum.Value2));
        }

        [Test]
        public void EnumAttributeDeserializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<EnumAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" MyEnum=""Value2"" />";

            var serializer = new CustomSerializer<EnumAttributeContainer>(TestXmlSerializerOptions.WithExtraTypes(typeof(IFoo)));

            var container = serializer.Deserialize(xml);

            Assert.That(container.MyEnum, Is.EqualTo(MyEnum.Value2));
        }

        [Test]
        public void EnumTypeElementSerializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var container = new EnumTypeElementContainer
            {
                Value = MyEnum.Value2
            };

            var serializer = new XmlSerializer<EnumTypeElementContainer>(typeof(MyEnum));

            var xml = serializer.Serialize(container);

            Assert.That(xml, Contains.Substring("MyEnum.Value2"));
        }

        [Test]
        public void EnumTypeAttributeSerializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var container = new EnumTypeAttributeContainer
            {
                Value = MyEnum.Value2
            };

            var serializer = new XmlSerializer<EnumTypeAttributeContainer>(typeof(MyEnum));

            var xml = serializer.Serialize(container);

            Assert.That(xml, Contains.Substring("MyEnum.Value2"));
        }

        [Test]
        public void EnumTypeElementDeserializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<EnumTypeElementContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Value>MyEnum.Value2</Value>
</EnumTypeElementContainer>";

            var serializer = new XmlSerializer<EnumTypeElementContainer>(typeof(MyEnum));

            var container = serializer.Deserialize(xml);

            Assert.That(container.Value, Is.EqualTo(MyEnum.Value2));
        }

        [Test]
        public void EnumTypeAttributeDeserializesCorrectlyWhenExtraTypesArePassedIn()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<EnumTypeAttributeContainer xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Value=""MyEnum.Value2"" />";

            var serializer = new XmlSerializer<EnumTypeAttributeContainer>(typeof(MyEnum));

            var container = serializer.Deserialize(xml);

            Assert.That(container.Value, Is.EqualTo(MyEnum.Value2));
        }

        [Test]
        public void AClassWithAPropertyOfTypeEnumUsesACustomSerializerInternally()
        {
            var serializer = new XmlSerializer<EnumTypeElementContainer>(typeof(MyEnum));

            Assert.That(serializer.Serializer, Is.InstanceOf<CustomSerializer<EnumTypeElementContainer>>());
        }

        [Test]
        public void AnXmlSerializerOfTypeEnumSerializesCorrectly()
        {
            Enum e = MyEnum.Value3;

            var serializer = new XmlSerializer<Enum>(o => o.Indent(), typeof(MyEnum));

            var xml = serializer.Serialize(e);

            Assert.That(xml, Is.EqualTo("<Enum>MyEnum.Value3</Enum>"));
        }

        [Test]
        public void AnXmlSerializerOfTypeEnumDeserializesCorrectly()
        {
            var serializer = new XmlSerializer<Enum>(typeof(MyEnum));

            var e = serializer.Deserialize("<Enum>MyEnum.Value3</Enum>");

            Assert.That(e, Is.EqualTo(MyEnum.Value3));
        }

        public class EnumElementContainer
        {
            public MyEnum MyEnum { get; set; }
        }

        public class EnumAttributeContainer
        {
            [XmlAttribute]
            public MyEnum MyEnum { get; set; }
        }

        public interface IFoo
        {
        }

        public enum MyEnum
        {
            Value1,
            Value2,
            Value3
        }

        public class EnumTypeElementContainer
        {
            public Enum Value { get; set; }
        }

        public class EnumTypeAttributeContainer
        {
            [XmlAttribute]
            public Enum Value { get; set; }
        }
    }
}