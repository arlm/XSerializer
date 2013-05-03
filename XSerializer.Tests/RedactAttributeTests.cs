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

            var redacted = attribute.Redact(input);

            Assert.That(redacted, Is.EqualTo("XXXXXX"));
        }

        [Test]
        public void NumbersAreRedacted()
        {
            var input = "123789";
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input);

            Assert.That(redacted, Is.EqualTo("######"));
        }

        [Test]
        public void NonAlphanumericsAreNotRedacted()
        {
            var input = @"!%&( +~?<|{]\";
            var attribute = new RedactAttribute();

            var redacted = attribute.Redact(input);

            Assert.That(redacted, Is.EqualTo(input));
        }

        [Test]
        public void OnlyPartsMatchingRedactPatternAreRedacted()
        {
            var input = "123-45-6789";
            var attribute = new RedactAttribute(@"\d{3}-\d{2}-");

            var redacted = attribute.Redact(input);

            Assert.That(redacted, Is.EqualTo("###-##-6789"));
        }
    }
}