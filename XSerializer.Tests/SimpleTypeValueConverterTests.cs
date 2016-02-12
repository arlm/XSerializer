using NUnit.Framework;

namespace XSerializer.Tests
{
    public class SimpleTypeValueConverterTests
    {
        public enum MyEnum
        {
            Value1
        }

        [Test]
        public void RedactedParseForEnumParsesCorrectlyWhenIgnoreCaseForEnumIsPassedIn()
        {
            var valueConverter = SimpleTypeValueConverter.Create(typeof (MyEnum), new RedactAttribute());
            var result = (MyEnum) valueConverter.ParseString("vALue1", new TestSerializeOptions {ShouldIgnoreCaseForEnum = true});
            Assert.AreEqual(result, MyEnum.Value1);
        }

        [Test]
        public void NonRedactedParseForEnumParsesCorrectlyWhenIgnoreCaseForEnumIsPassedIn()
        {
            var valueConverter = SimpleTypeValueConverter.Create(typeof(MyEnum?), null);
            var result = (MyEnum)valueConverter.ParseString("vALue1", new TestSerializeOptions { ShouldIgnoreCaseForEnum = true });
            Assert.AreEqual(result, MyEnum.Value1);
        }

        [Test]
        public void NonRedactedParseForNullableEnumParsesCorrectlyWhenIgnoreCaseForEnumIsPassedIn()
        {
            var valueConverter = SimpleTypeValueConverter.Create(typeof(MyEnum?), null);
            var result = (MyEnum)valueConverter.ParseString("vALue1", new TestSerializeOptions { ShouldIgnoreCaseForEnum = true });
            Assert.AreEqual(result, MyEnum.Value1);
        }
    }
}
