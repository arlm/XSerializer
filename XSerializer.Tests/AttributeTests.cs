using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Xml.Serialization;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class AttributeTests
    {
        [Test]
        public void XmlAttributeDeserializesIntoProperty()
        {
            var xml = @"<AttributeContainer SomeValue=""abc""></AttributeContainer>";

            var serializer = new CustomSerializer<AttributeContainer>(null, TestXmlSerializerOptions.Empty);

            var container = (AttributeContainer)serializer.DeserializeObject(xml);

            Assert.That(container.SomeValue, Is.EqualTo("abc"));
        }

        [Test]
        public void TheXmlTextAttributeDefinedInInterfaceIsUsed()
        {
            var serializer = new XmlSerializer<HasXmlTextAttribute>(x => x.ShouldUseAttributeDefinedInInterface());

            var item = new HasXmlTextAttribute { Foo = "abc" };

            var xml = serializer.Serialize(item);

            Assert.That(xml, Does.Not.Contain("<Foo>abc</Foo>"));
            Assert.That(xml, Does.Contain(">abc</HasXmlTextAttribute>"));
        }

        [Test]
        public void TheXmlAttributeAttributeDefinedInInterfaceIsUsed()
        {
            var serializer = new XmlSerializer<HasXmlAttributeAttribute>(x => x.ShouldUseAttributeDefinedInInterface());

            var item = new HasXmlAttributeAttribute { Foo = "abc" };

            var xml = serializer.Serialize(item);
            Console.WriteLine(xml);
            Assert.That(xml, Does.Not.Contain("<Foo>abc</Foo>"));
            Assert.That(xml, Does.Contain(@"Foo=""abc"""));
        }

        [Test]
        public void TheXmlElementAttributeDefinedInInterfaceIsUsed()
        {
            var serializer = new XmlSerializer<HasXmlElementAttribute>(x => x.ShouldUseAttributeDefinedInInterface());

            var item = new HasXmlElementAttribute { Foo = "abc" };

            var xml = serializer.Serialize(item);

            Assert.That(xml, Does.Not.Contain("<Foo>abc</Foo>"));
            Assert.That(xml, Does.Contain("<Bar>abc</Bar>"));
        }

        [Test]
        public void TheXmlElementAttributeDecoratingAListPropertyDefinedInInterfaceIsUsed()
        {
            var serializer = new XmlSerializer<HasListXmlElementAttribute>(x => x.ShouldUseAttributeDefinedInInterface());

            var item = new HasListXmlElementAttribute
            {
                Bazes = new List<Baz> { new Baz { Qux = "abc" }, new Baz { Qux = "xyz" } }
            };

            var xml = serializer.Serialize(item);

            Assert.That(xml, Does.Not.Contain("<Bazes>"));
            Assert.That(xml, Does.Not.Contain("<Baz>"));
            Assert.That(xml, Does.Contain("<Bar>"));
        }

        [Test]
        public void TheXmlArrayAttributeDefinedInInterfaceIsUsed()
        {
            var serializer = new XmlSerializer<HasXmlArrayAttribute>(x => x.ShouldUseAttributeDefinedInInterface());

            var item = new HasXmlArrayAttribute
            {
                Bazes = new List<Baz> { new Baz { Qux = "abc" }, new Baz { Qux = "xyz" } }
            };

            var xml = serializer.Serialize(item);

            Assert.That(xml, Does.Not.Contain("<Bazes>"));
            Assert.That(xml, Does.Contain("<Bars>"));
        }

        [Test]
        public void TheXmlArrayItemAttributeDefinedInInterfaceIsUsed()
        {
            var serializer = new XmlSerializer<HasXmlArrayItemAttribute>(x => x.ShouldUseAttributeDefinedInInterface());

            var item = new HasXmlArrayItemAttribute
            {
                Bazes = new List<Baz> { new Baz { Qux = "abc" }, new Baz { Qux = "xyz" } }
            };

            var xml = serializer.Serialize(item);

            Assert.That(xml, Does.Not.Contain("<Baz>"));
            Assert.That(xml, Does.Contain("<Bar>"));
        }

        [Test]
        public void ForXmlTheRedactAttributeDefinedInInterfaceIsUsed()
        {
            var serializer = new XmlSerializer<HasRedactAttribute>(x =>
                x.ShouldUseAttributeDefinedInInterface());

            var item = new HasRedactAttribute
            {
                Foo = "abc"
            };

            var xml = serializer.Serialize(item);

            Assert.That(xml, Does.Not.Contain("<Foo>abc</Foo>"));
            Assert.That(xml, Does.Contain("<Foo>XXX</Foo>"));
        }

        [Test]
        public void ForXmlTheEncryptAttributeDefinedInInterfaceIsUsed()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer = new XmlSerializer<HasEncryptAttribute>(x =>
                x.ShouldUseAttributeDefinedInInterface()
                .WithEncryptionMechanism(encryptionMechanism));

            var item = new HasEncryptAttribute
            {
                Foo = "abc"
            };

            var xml = serializer.Serialize(item);

            Assert.That(xml, Does.Not.Contain("<Foo>abc</Foo>"));
            var encryptedValue = encryptionMechanism.Encrypt(@"abc", null, new SerializationState());
            Assert.That(xml, Does.Contain(string.Format("<Foo>{0}</Foo>", encryptedValue)));
        }

        [Test]
        public void ForJsonTheEncryptAttributeDefinedInInterfaceIsUsed()
        {
            IEncryptionMechanism encryptionMechanism = new Base64EncryptionMechanism();

            var serializer =
                new JsonSerializer<HasEncryptAttribute>(new JsonSerializerConfiguration
                {
                    ShouldUseAttributeDefinedInInterface = true,
                    EncryptionMechanism = encryptionMechanism
                });

            var item = new HasEncryptAttribute
            {
                Foo = "abc"
            };

            var json = serializer.Serialize(item);

            Assert.That(json, Does.Not.Contain(@"""Foo"":""abc"""));
            var encryptedValue = encryptionMechanism.Encrypt(@"""abc""", null, new SerializationState());
            Assert.That(json, Does.Contain(string.Format(@"""Foo"":""{0}""", encryptedValue)));
        }

        [Test]
        public void TheJsonMappingAttributeDefinedInInterfaceIsUsed()
        {
            var serializer =
                new JsonSerializer<HasJsonMappingAttribute>(new JsonSerializerConfiguration
                {
                    ShouldUseAttributeDefinedInInterface = true
                });

            var item = serializer.Deserialize(@"{""Grault"":{""Thud"":""abc""}}");

            Assert.That(item.Grault, Is.InstanceOf<Garply>());
        }

        public class VerifyAttributeRetrievalBehavior
        {
            [Test]
            public void DisableWithConfiguration()
            {
                var serializer = new XmlSerializer<HasXmlElementAttribute>();

                var item = new HasXmlElementAttribute { Foo = "abc" };

                var xml = serializer.Serialize(item);

                Assert.That(xml, Does.Contain("<Foo>abc</Foo>"));
                Assert.That(xml, Does.Not.Contain("<Bar>abc</Bar>"));
            }

            [Test]
            public void IgnoreWithExplicitImplementation()
            {
                var serializer = new XmlSerializer<RemovingBehavior>(x => x.ShouldUseAttributeDefinedInInterface());

                var item = new RemovingBehavior { Foo = "abc" };

                var xml = serializer.Serialize(item);

                Assert.That(xml, Does.Contain("<Foo>abc</Foo>"));
                Assert.That(xml, Does.Not.Contain("<Bar>abc</Bar>"));
            }

            [Test]
            public void ChangeWithClassPropertyDecoration()
            {
                var serializer = new XmlSerializer<OverridingBehavior>(x => x.ShouldUseAttributeDefinedInInterface());

                var item = new OverridingBehavior { Foo = "abc" };

                var xml = serializer.Serialize(item);

                Assert.That(xml, Does.Contain("<Baz>abc</Baz>"));
                Assert.That(xml, Does.Not.Contain("<Foo>abc</Foo>"));
            }
        }

        public class AttributeContainer
        {
            [XmlAttribute]
            public string SomeValue { get; set; }
        }

        public interface IHasXmlTextAttribute
        {
            [XmlText]
            string Foo { get; set; }
        }

        public class HasXmlTextAttribute : IHasXmlTextAttribute
        {
            public string Foo { get; set; }
        }

        public interface IHasXmlAttributeAttribute
        {
            [XmlAttribute]
            string Foo { get; set; }
        }

        public class HasXmlAttributeAttribute : IHasXmlAttributeAttribute
        {
            public string Foo { get; set; }
        }

        public interface IHasXmlElementAttribute
        {
            [XmlElement("Bar")]
            string Foo { get; set; }
        }

        public class HasXmlElementAttribute : IHasXmlElementAttribute
        {
            public string Foo { get; set; }
        }

        public interface IHasListXmlElementAttribute
        {
            [XmlElement("Bar")]
            List<Baz> Bazes { get; set; }
        }

        public class HasListXmlElementAttribute : IHasListXmlElementAttribute
        {
            public List<Baz> Bazes { get; set; }
        }

        public class Baz
        {
            [XmlText]
            public string Qux { get; set; }
        }

        public interface IHasXmlArrayAttribute
        {
            [XmlArray("Bars")]
            List<Baz> Bazes { get; set; }
        }

        public class HasXmlArrayAttribute : IHasXmlArrayAttribute
        {
            public List<Baz> Bazes { get; set; }
        }

        public interface IHasXmlArrayItemAttribute
        {
            [XmlArrayItem("Bar")]
            List<Baz> Bazes { get; set; }
        }

        public class HasXmlArrayItemAttribute : IHasXmlArrayItemAttribute
        {
            public List<Baz> Bazes { get; set; }
        }

        public interface IHasEncryptAttribute
        {
            [Encrypt]
            string Foo { get; set; }
        }

        public class HasEncryptAttribute : IHasEncryptAttribute
        {
            public string Foo { get; set; }
        }

        public interface IHasRedactAttribute
        {
            [Redact]
            string Foo { get; set; }
        }

        public class HasRedactAttribute : IHasRedactAttribute
        {
            public string Foo { get; set; }
        }

        public interface IHasJsonMappingAttribute
        {
            [JsonMapping(typeof(Garply))]
            Grault Grault { get; set; }
        }

        public class HasJsonMappingAttribute : IHasJsonMappingAttribute
        {
            public Grault Grault { get; set; }
        }

        public class Grault
        {
            public string Thud { get; set; }
        }

        public class Garply : Grault
        {
        }

        public class RemovingBehavior : IHasXmlElementAttribute
        {
            public string Foo { get; set; }
            string IHasXmlElementAttribute.Foo { get { return Foo; } set { Foo = value; } }
        }

        public class OverridingBehavior : IHasXmlElementAttribute
        {
            [XmlElement("Baz")]
            public string Foo { get; set; }
        }
    }
}
