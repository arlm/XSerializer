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
    }
}