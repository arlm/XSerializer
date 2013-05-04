using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class RedactAttributeTests
    {
        [Test]
        public void ANullStringReturnsNull()
        {
            string input = null;
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input, true);

            Assert.That(redacted, Is.Null);
        }

        [Test]
        public void ANullObjectReturnsNull()
        {
            object input = null;
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input, true);

            Assert.That(redacted, Is.Null);
        }

        [Test]
        public void LettersAreRedacted()
        {
            var input = "abcXYZ";
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input, true);

            Assert.That(redacted, Is.EqualTo("XXXXXX"));
        }

        [Test]
        public void NumbersAreRedacted()
        {
            var input = "123789";
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input, true);

            Assert.That(redacted, Is.EqualTo("111111"));
        }

        [Test]
        public void NonAlphanumericsAreNotRedacted()
        {
            var input = @"!%&( +~?<|{]\";
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input, true);

            Assert.That(redacted, Is.EqualTo(input));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TrueAndFalseRedactTheSame(bool booleanValue)
        {
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(booleanValue, true);

            Assert.That(redacted, Is.EqualTo("XXXXXX"));
        }

        [TestCase(MyEnum.A)]
        [TestCase(MyEnum.Ab)]
        [TestCase(MyEnum.Abc)]
        [TestCase(MyEnum.Abcd)]
        public void TrueAndFalseRedactTheSame(MyEnum myEnum)
        {
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(myEnum, true);

            Assert.That(redacted, Is.EqualTo("XXXXXX"));
        }

        [TestCase(MyEnum.A)]
        [TestCase(MyEnum.Ab)]
        [TestCase(MyEnum.Abc)]
        [TestCase(MyEnum.Abcd)]
        [TestCase(MyOtherEnum.X)]
        [TestCase(MyOtherEnum.Xy)]
        [TestCase(MyOtherEnum.Xyz)]
        public void AllEnumsRedactTheSame(Enum enumValue)
        {
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(enumValue, true);

            Assert.That(redacted, Is.EqualTo("XXXXXX"));
        }
        
        [TestCase(123, "111")]
        [TestCase(-123, "1111")]
        [TestCase(123.45, "111111")]
        [TestCase(-123.45, "1111111")]
        public void ObjectsRedactToTheirStringRepresentation(object value, string expectedRedactedValue)
        {
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(value, true);

            Assert.That(redacted, Is.EqualTo(expectedRedactedValue));
        }

        [Test]
        public void DateTimeRedactsToRedactedRoundTripRepresentation()
        {
            var dateTime = new DateTime(2013, 5, 3, 20, 8, 55, DateTimeKind.Utc);
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(dateTime, true);

            Assert.That(redacted, Is.EqualTo("1111-11-11T11:11:11.1111111Z"));
        }

        public enum MyEnum
        {
            A,
            Ab,
            Abc,
            Abcd
        }

        public enum MyOtherEnum
        {
            X,
            Xy,
            Xyz
        }
    }
}