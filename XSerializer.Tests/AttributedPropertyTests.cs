using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class AttributedPropertyTests
    {
        [Test]
        public void TwoXmlElementAttributedStringPropertiesArePossible()
        {
            var serializer = new XmlSerializer<TwoXmlElementAttributedStringProperties>();
            var wtf = new TwoXmlElementAttributedStringProperties { Foo = "abc", Bar = "xyz" };
            var xml = serializer.Serialize(wtf);
            var wtf2 = serializer.Deserialize(xml);
            Assert.That(wtf2, Has.PropertiesEqualTo(wtf));
        }

        public class TwoXmlElementAttributedStringProperties
        {
            [XmlElement("FOO")]
            public string Foo { get; set; }

            [XmlElement("BAR")]
            public string Bar { get; set; }
        }

        [TestCaseSource("TestCaseData")]
        public void CustomSerializerThrowsExceptionIfAndOnlyIfBclXmlSerializerDoes(Type type)
        {
            Exception thrownException;
            try
            {
                new System.Xml.Serialization.XmlSerializer(type);
                thrownException = null;
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            if (thrownException == null)
            {
                Assert.That(() => CustomSerializer.GetSerializer(type, null, TestXmlSerializerOptions.Empty), Throws.Nothing, "Type: " + type.Name + Environment.NewLine);
            }
            else
            {
                Assert.That(() => CustomSerializer.GetSerializer(type, null, TestXmlSerializerOptions.Empty), Throws.InstanceOf(thrownException.GetType()), "Type: " + type.Name + Environment.NewLine);
            }
        }

        private static IEnumerable<TestCaseData> TestCaseData
        {
            get { return GetTypes()
                .Select(t => new TestCaseData(t).SetName(
                    t.Name
                    .Replace("DerivedWith", "")
                    .Replace("Decoration", "")
                    .Replace("InheritingFromBaseWith", " inheriting from ")
                    .Replace("EmptyXmlElement", "Empty XmlElement")
                    .Replace("EmptyXmlAttribute", "Empty XmlAttribute")
                    .Replace("NonEmpty", "Non-empty")
                    .Replace("NoXml", "No attribute decoration")
                    .Replace("from N", "from n")
                    .Replace("from E", "from e")));
            }
        }

        private static IEnumerable<Type> GetTypes()
        {
            yield return typeof(DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlElementDecoration);
            yield return typeof(DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration);
            yield return typeof(DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithNoXmlDecoration);
            yield return typeof(DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlElementDecoration);
            yield return typeof(DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration);
            yield return typeof(DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithNoXmlDecoration);
            yield return typeof(DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithNoXmlDecoration);
            yield return typeof(DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithDifferentNonEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithDifferentNonEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithNoXmlDecoration);
            yield return typeof(DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithDifferentNonEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithDifferentNonEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNoXmlDecorationInheritingFromBaseWithEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNoXmlDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNoXmlDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration);
            yield return typeof(DerivedWithNoXmlDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration);
            yield return typeof(DerivedWithNoXmlDecorationInheritingFromBaseWithNoXmlDecoration);
        }

        #region Base Types

        public class BaseWithEmptyXmlElementDecoration
        {
            [XmlElement]
            public virtual string Foo { get; set; }
        }

        public class BaseWithEmptyXmlAttributeDecoration
        {
            [XmlAttribute]
            public virtual string Foo { get; set; }
        }

        public class BaseWithNonEmptyXmlElementDecoration
        {
            [XmlElement("Bar")]
            public virtual string Foo { get; set; }
        }

        public class BaseWithNonEmptyXmlAttributeDecoration
        {
            [XmlAttribute("Bar")]
            public virtual string Foo { get; set; }
        }

        public class BaseWithNoXmlDecoration
        {
            public virtual string Foo { get; set; }
        }

        #endregion

        #region DerivedWithEmptyXmlElementDecoration

        public class DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlElementDecoration : BaseWithEmptyXmlElementDecoration
        {
            [XmlElement]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration : BaseWithEmptyXmlAttributeDecoration
        {
            [XmlElement]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration : BaseWithNonEmptyXmlElementDecoration
        {
            [XmlElement]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration : BaseWithNonEmptyXmlAttributeDecoration
        {
            [XmlElement]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlElementDecorationInheritingFromBaseWithNoXmlDecoration : BaseWithNoXmlDecoration
        {
            [XmlElement]
            public override string Foo { get; set; }
        }

        #endregion

        #region DerivedWithEmptyXmlAttributeDecoration

        public class DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlElementDecoration : BaseWithEmptyXmlElementDecoration
        {
            [XmlAttribute]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration : BaseWithEmptyXmlAttributeDecoration
        {
            [XmlAttribute]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration : BaseWithNonEmptyXmlElementDecoration
        {
            [XmlAttribute]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration : BaseWithNonEmptyXmlAttributeDecoration
        {
            [XmlAttribute]
            public override string Foo { get; set; }
        }

        public class DerivedWithEmptyXmlAttributeDecorationInheritingFromBaseWithNoXmlDecoration : BaseWithNoXmlDecoration
        {
            [XmlAttribute]
            public override string Foo { get; set; }
        }

        #endregion

        #region DerivedWithNonEmptyXmlElementDecoration

        public class DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlElementDecoration : BaseWithEmptyXmlElementDecoration
        {
            [XmlElement("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration : BaseWithEmptyXmlAttributeDecoration
        {
            [XmlElement("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration : BaseWithNonEmptyXmlElementDecoration
        {
            [XmlElement("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration : BaseWithNonEmptyXmlAttributeDecoration
        {
            [XmlElement("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithNoXmlDecoration : BaseWithNoXmlDecoration
        {
            [XmlElement("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithDifferentNonEmptyXmlElementDecoration : BaseWithNonEmptyXmlElementDecoration
        {
            [XmlElement("Corge")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlElementDecorationInheritingFromBaseWithDifferentNonEmptyXmlAttributeDecoration : BaseWithNonEmptyXmlAttributeDecoration
        {
            [XmlElement("Corge")]
            public override string Foo { get; set; }
        }

        #endregion

        #region DerivedWithNonEmptyXmlAttributeDecoration

        public class DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlElementDecoration : BaseWithEmptyXmlElementDecoration
        {
            [XmlAttribute("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration : BaseWithEmptyXmlAttributeDecoration
        {
            [XmlAttribute("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration : BaseWithNonEmptyXmlElementDecoration
        {
            [XmlAttribute("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration : BaseWithNonEmptyXmlAttributeDecoration
        {
            [XmlAttribute("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithNoXmlDecoration : BaseWithNoXmlDecoration
        {
            [XmlAttribute("Bar")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithDifferentNonEmptyXmlElementDecoration : BaseWithNonEmptyXmlElementDecoration
        {
            [XmlAttribute("Corge")]
            public override string Foo { get; set; }
        }

        public class DerivedWithNonEmptyXmlAttributeDecorationInheritingFromBaseWithDifferentNonEmptyXmlAttributeDecoration : BaseWithNonEmptyXmlAttributeDecoration
        {
            [XmlAttribute("Corge")]
            public override string Foo { get; set; }
        }

        #endregion

        #region DerivedWithNoXmlDecoration

        public class DerivedWithNoXmlDecorationInheritingFromBaseWithEmptyXmlElementDecoration : BaseWithEmptyXmlElementDecoration
        {
            public override string Foo { get; set; }
        }

        public class DerivedWithNoXmlDecorationInheritingFromBaseWithEmptyXmlAttributeDecoration : BaseWithEmptyXmlAttributeDecoration
        {
            public override string Foo { get; set; }
        }

        public class DerivedWithNoXmlDecorationInheritingFromBaseWithNonEmptyXmlElementDecoration : BaseWithNonEmptyXmlElementDecoration
        {
            public override string Foo { get; set; }
        }

        public class DerivedWithNoXmlDecorationInheritingFromBaseWithNonEmptyXmlAttributeDecoration : BaseWithNonEmptyXmlAttributeDecoration
        {
            public override string Foo { get; set; }
        }

        public class DerivedWithNoXmlDecorationInheritingFromBaseWithNoXmlDecoration : BaseWithNoXmlDecoration
        {
            public override string Foo { get; set; }
        }

        #endregion
    }
}