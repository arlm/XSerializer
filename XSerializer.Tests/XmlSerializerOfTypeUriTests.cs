using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class XmlSerializerOfTypeUriTests
    {
        [TestCase("ftp://ftp.is.co.za/rfc/rfc1808.txt")]
        [TestCase("http://www.ietf.org/rfc/rfc2396.txt")]
        [TestCase("ldap://[2001:db8::7]/c=GB?objectClass?one")]
        [TestCase("mailto:John.Doe@example.com")]
        [TestCase("news:comp.infosystems.www.servers.unix")]
        [TestCase("tel:+1-816-555-1212")]
        [TestCase("telnet://192.0.2.16:80/")]
        [TestCase("urn:oasis:names:specification:docbook:dtd:xml:4.1.2")]
        public void VerifyThatAPropertyOfTypeTypeCanSuccessfullyRoundTrip(string uriString)
        {
            var serializer = new XmlSerializer<Thingy>(x => x.Indent());

            var blackHole = new Thingy { Uri = new Uri(uriString) };

            var xml = serializer.Serialize(blackHole);
            Console.WriteLine(xml);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Uri, Is.EqualTo(blackHole.Uri));
        }

        [TestCase("ftp://ftp.is.co.za/rfc/rfc1808.txt")]
        [TestCase("http://www.ietf.org/rfc/rfc2396.txt")]
        [TestCase("ldap://[2001:db8::7]/c=GB?objectClass?one")]
        [TestCase("mailto:John.Doe@example.com")]
        [TestCase("news:comp.infosystems.www.servers.unix")]
        [TestCase("tel:+1-816-555-1212")]
        [TestCase("telnet://192.0.2.16:80/")]
        [TestCase("urn:oasis:names:specification:docbook:dtd:xml:4.1.2")]
        public void VerifyThatATypeCanRoundTrip(string uriString)
        {
            var serializer = new XmlSerializer<Uri>(x => x.Indent());

            var uri = new Uri(uriString);

            var xml = serializer.Serialize(uri);
            Console.WriteLine(xml);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip, Is.EqualTo(uri));
        }

        [Test]
        public void ANullPropertyValueShouldNotSerializeAsAnEmptyElement()
        {
            var serializer = new XmlSerializer<Thingy>(x => x.Indent());

            var blackHole = new Thingy { Uri = null };

            var xml = serializer.Serialize(blackHole);

            Assert.That(xml, Is.Not.StringMatching(@"<Uri\s*/>"));
        }

        public class Thingy
        {
            public Uri Uri { get; set; }
        }
    }
}
