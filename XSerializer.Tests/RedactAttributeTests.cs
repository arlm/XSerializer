using NUnit.Framework;

namespace XSerializer.Tests
{
    public class RedactAttributeTests
    {
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

            Assert.That(redacted, Is.EqualTo("######"));
        }

        [Test]
        public void NonAlphanumericsAreNotRedacted()
        {
            var input = @"!%&( +~?<|{]\";
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input, true);

            Assert.That(redacted, Is.EqualTo(input));
        }
    }
}