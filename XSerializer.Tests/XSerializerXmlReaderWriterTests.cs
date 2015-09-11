using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class XSerializerXmlReaderWriterTests
    {
        [Test]
        public void CanEncryptAndDecryptAComplexElementValue()
        {
            var sb = new StringBuilder();
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();
            var options = new TestSerializeOptions { EncryptionMechanism = encryptionMechanism, SerializationState = new SerializationState() };

            using (var stringWriter = new StringWriter(sb))
            {
                using (var writer = new XSerializerXmlTextWriter(stringWriter, options))
                {
                    writer.WriteStartElement("foo");
                    writer.IsEncryptionEnabled = true;

                    writer.WriteStartElement("bar");

                    writer.WriteStartAttribute("baz");
                    writer.WriteValue("123");
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("qux");
                    writer.WriteValue("abc");
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // </bar>

                    writer.WriteStartElement("rab");

                    writer.WriteStartAttribute("zab");
                    writer.WriteValue("789");
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("xuq");
                    writer.WriteValue("xyz");
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // </rab>

                    writer.IsEncryptionEnabled = false;
                    writer.WriteEndElement(); // </foo>
                }
            }

            Func<string, string> e = x => encryptionMechanism.Encrypt(x, null, options.SerializationState);

            // reference xml:
            // <foo><bar baz="123"><qux>abc</qux></bar><rab zab="789"><xuq>xyz</xuq></rab></foo>

            var xml = sb.ToString();

            var expectedXml =
                "<foo>"
                + e(@"<bar baz=""123""><qux>abc</qux></bar><rab zab=""789""><xuq>xyz</xuq></rab>")
                + "</foo>";

            Assert.That(xml, Is.EqualTo(expectedXml));

            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    using (var reader = new XSerializerXmlReader(xmlReader, encryptionMechanism, null, options.SerializationState))
                    {
                        reader.Read(); // None -> <foo>
                        reader.IsDecryptionEnabled = true;

                        reader.Read(); // <foo> -> <bar>

                        Assert.That(reader.GetAttribute("baz"), Is.EqualTo("123"));

                        reader.Read(); // <bar> -> <qux>

                        Assert.That(reader.ReadString(), Is.EqualTo("abc")); // <qux> -> "abc" -> </qux>

                        reader.Read(); // </qux> -> </bar>
                        reader.Read(); // </bar> -> <rab>

                        Assert.That(reader.GetAttribute("zab"), Is.EqualTo("789"));

                        reader.Read(); // <rab> -> <xuq>

                        Assert.That(reader.ReadString(), Is.EqualTo("xyz")); // <xuq> -> "xyz" -> </xuq>

                        reader.Read(); // </xuq> -> </rab>

                        reader.Read(); // </rab> -> </foo>

                        reader.IsDecryptionEnabled = false;
                        reader.Read(); // </foo> -> None

                        Assert.That(reader.NodeType, Is.EqualTo(XmlNodeType.None));
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this instance [can encrypt and decrypt multiple complex element values each with attributes].
        /// </summary>
        [Test]
        public void CanEncryptAndDecryptMultipleComplexElementValuesEachWithAttributes()
        {
            var sb = new StringBuilder();
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();
            var options = new TestSerializeOptions { EncryptionMechanism = encryptionMechanism, SerializationState = new SerializationState() };

            using (var stringWriter = new StringWriter(sb))
            {
                using (var writer = new XSerializerXmlTextWriter(stringWriter, options))
                {
                    writer.WriteStartElement("foo");

                    writer.WriteStartElement("bar");
                    writer.IsEncryptionEnabled = true;

                    writer.WriteStartAttribute("baz");
                    writer.WriteValue("123");
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("qux");
                    writer.WriteValue("abc");
                    writer.WriteEndElement();

                    writer.IsEncryptionEnabled = false;
                    writer.WriteEndElement(); // </bar>

                    writer.WriteStartElement("rab");
                    writer.IsEncryptionEnabled = true;

                    writer.WriteStartAttribute("zab");
                    writer.WriteValue("789");
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("xuq");
                    writer.WriteValue("xyz");
                    writer.WriteEndElement();

                    writer.IsEncryptionEnabled = false;
                    writer.WriteEndElement(); // </rab>

                    writer.WriteEndElement(); // </foo>
                }
            }

            Func<string, string> e = x => encryptionMechanism.Encrypt(x, null, options.SerializationState);

            // reference xml:
            // <foo><bar baz="123"><qux>abc</qux></bar><rab zab="789"><xuq>xyz</xuq></rab></foo>

            var xml = sb.ToString();

            var expectedXml =
                "<foo>" +
                @"<bar baz=""" + e("123") + @""">"
                + e("<qux>abc</qux>")
                + "</bar>" +
                @"<rab zab=""" + e("789") + @""">"
                + e("<xuq>xyz</xuq>")
                + "</rab>"
                + "</foo>";

            Assert.That(xml, Is.EqualTo(expectedXml));

            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    using (var reader = new XSerializerXmlReader(xmlReader, encryptionMechanism, null, options.SerializationState))
                    {
                        reader.Read(); // None -> <foo>

                        reader.Read(); // <foo> -> <bar>
                        reader.IsDecryptionEnabled = true;

                        Assert.That(reader.GetAttribute("baz"), Is.EqualTo("123"));

                        reader.Read(); // <bar> -> <qux>

                        Assert.That(reader.ReadString(), Is.EqualTo("abc")); // <qux> -> "abc" -> </qux>

                        reader.IsDecryptionEnabled = false;

                        reader.Read(); // </qux> -> </bar>
                        reader.Read(); // </bar> -> <rab>
                        
                        reader.IsDecryptionEnabled = true;

                        Assert.That(reader.GetAttribute("zab"), Is.EqualTo("789"));

                        reader.Read(); // <rab> -> <xuq>

                        Assert.That(reader.ReadString(), Is.EqualTo("xyz")); // <xuq> -> "xyz" -> </xuq>

                        reader.Read(); // </xuq> -> </rab>

                        reader.IsDecryptionEnabled = false;
                        reader.Read(); // </rab> -> </foo>

                        reader.Read(); // </foo> -> None

                        Assert.That(reader.NodeType, Is.EqualTo(XmlNodeType.None));
                    }
                }
            }
        }

        [Test]
        public void CanEncryptAndDecryptIndividualAttributeValues()
        {
            var sb = new StringBuilder();
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();
            var options = new TestSerializeOptions { EncryptionMechanism = encryptionMechanism, SerializationState = new SerializationState() };

            using (var stringWriter = new StringWriter(sb))
            {
                using (var writer = new XSerializerXmlTextWriter(stringWriter, options))
                {
                    writer.WriteStartElement("foo");

                    writer.WriteStartElement("bar");

                    writer.WriteStartAttribute("baz");
                    writer.IsEncryptionEnabled = true;
                    writer.WriteValue("123");
                    writer.IsEncryptionEnabled = false;
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("qux");
                    writer.WriteValue("abc");
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // </bar>

                    writer.WriteStartElement("rab");

                    writer.WriteStartAttribute("zab");
                    writer.IsEncryptionEnabled = true;
                    writer.WriteValue("789");
                    writer.IsEncryptionEnabled = false;
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("xuq");
                    writer.WriteValue("xyz");
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // </rab>

                    writer.WriteEndElement(); // </foo>
                }
            }

            Func<string, string> e = x => encryptionMechanism.Encrypt(x, null, options.SerializationState);

            // reference xml:
            // <foo><bar baz="123"><qux>abc</qux></bar><rab zab="789"><xuq>xyz</xuq></rab></foo>

            var xml = sb.ToString();

            var expectedXml =
                "<foo>" +
                @"<bar baz=""" + e("123") + @""">"
                + "<qux>abc</qux>"
                + "</bar>" +
                @"<rab zab=""" + e("789") + @""">"
                + "<xuq>xyz</xuq>"
                + "</rab>"
                + "</foo>";

            Assert.That(xml, Is.EqualTo(expectedXml));

            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    using (var reader = new XSerializerXmlReader(xmlReader, encryptionMechanism, options.EncryptKey, options.SerializationState))
                    {
                        reader.Read(); // None -> <foo>

                        reader.Read(); // <foo> -> <bar>

                        reader.IsDecryptionEnabled = true;
                        Assert.That(reader.GetAttribute("baz"), Is.EqualTo("123"));
                        reader.IsDecryptionEnabled = false;

                        reader.Read(); // <bar> -> <qux>

                        Assert.That(reader.ReadString(), Is.EqualTo("abc")); // <qux> -> "abc" -> </qux>


                        reader.Read(); // </qux> -> </bar>
                        reader.Read(); // </bar> -> <rab>

                        reader.IsDecryptionEnabled = true;
                        Assert.That(reader.GetAttribute("zab"), Is.EqualTo("789"));
                        reader.IsDecryptionEnabled = false;

                        reader.Read(); // <rab> -> <xuq>

                        Assert.That(reader.ReadString(), Is.EqualTo("xyz")); // <xuq> -> "xyz" -> </xuq>

                        reader.Read(); // </xuq> -> </rab>

                        reader.Read(); // </rab> -> </foo>

                        reader.Read(); // </foo> -> None

                        Assert.That(reader.NodeType, Is.EqualTo(XmlNodeType.None));
                    }
                }
            }
        }

        [Test]
        public void CanEncryptAndDecryptIndividualElementValues()
        {
            var sb = new StringBuilder();
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();
            var options = new TestSerializeOptions { EncryptionMechanism = encryptionMechanism, SerializationState = new SerializationState() };

            using (var stringWriter = new StringWriter(sb))
            {
                using (var writer = new XSerializerXmlTextWriter(stringWriter, options))
                {
                    writer.WriteStartElement("foo");

                    writer.WriteStartElement("bar");

                    writer.WriteStartAttribute("baz");
                    writer.WriteValue("123");
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("qux");
                    writer.IsEncryptionEnabled = true;
                    writer.WriteValue("abc");
                    writer.IsEncryptionEnabled = false;
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // </bar>

                    writer.WriteStartElement("rab");

                    writer.WriteStartAttribute("zab");
                    writer.WriteValue("789");
                    writer.WriteEndAttribute();

                    writer.WriteStartElement("xuq");
                    writer.IsEncryptionEnabled = true;
                    writer.WriteValue("xyz");
                    writer.IsEncryptionEnabled = false;
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // </rab>

                    writer.WriteEndElement(); // </foo>
                }
            }

            Func<string, string> e = x => encryptionMechanism.Encrypt(x, null, options.SerializationState);

            // reference xml:
            // <foo><bar baz="123"><qux>abc</qux></bar><rab zab="789"><xuq>xyz</xuq></rab></foo>

            var xml = sb.ToString();

            var expectedXml =
                "<foo>" +
                @"<bar baz=""123"">"
                + "<qux>" + e("abc") + "</qux>"
                + "</bar>" +
                @"<rab zab=""789"">"
                + "<xuq>" + e("xyz") + "</xuq>"
                + "</rab>"
                + "</foo>";

            Assert.That(xml, Is.EqualTo(expectedXml));

            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    using (var reader = new XSerializerXmlReader(xmlReader, encryptionMechanism, options.EncryptKey, options.SerializationState))
                    {
                        reader.Read(); // None -> <foo>

                        reader.Read(); // <foo> -> <bar>

                        Assert.That(reader.GetAttribute("baz"), Is.EqualTo("123"));

                        reader.Read(); // <bar> -> <qux>

                        reader.IsDecryptionEnabled = true;
                        Assert.That(reader.ReadString(), Is.EqualTo("abc")); // <qux> -> "abc" -> </qux>
                        reader.IsDecryptionEnabled = false;

                        reader.Read(); // </qux> -> </bar>
                        reader.Read(); // </bar> -> <rab>

                        Assert.That(reader.GetAttribute("zab"), Is.EqualTo("789"));

                        reader.Read(); // <rab> -> <xuq>

                        reader.IsDecryptionEnabled = true;
                        Assert.That(reader.ReadString(), Is.EqualTo("xyz")); // <xuq> -> "xyz" -> </xuq>
                        reader.IsDecryptionEnabled = false;

                        reader.Read(); // </xuq> -> </rab>

                        reader.Read(); // </rab> -> </foo>

                        reader.Read(); // </foo> -> None

                        Assert.That(reader.NodeType, Is.EqualTo(XmlNodeType.None));
                    }
                }
            }
        }
    }
}
