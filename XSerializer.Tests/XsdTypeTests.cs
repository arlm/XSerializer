using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Xml.Serialization;

namespace XSerializer.Tests
{
    public class XsdTypeTests : ObjectToXmlWithDefaultComparison
    {
        protected override IEnumerable<TestCaseData> GetTestCaseData()
        {
            yield return new TestCaseData(new Foo { Bar = true }, typeof(Foo)).SetName("bool");
            yield return new TestCaseData(new Foo { Bar = (byte)123 }, typeof(Foo)).SetName("byte");
            yield return new TestCaseData(new Foo { Bar = (sbyte)123 }, typeof(Foo)).SetName("sbyte");
            yield return new TestCaseData(new Foo { Bar = (short)123 }, typeof(Foo)).SetName("short");
            yield return new TestCaseData(new Foo { Bar = (ushort)123 }, typeof(Foo)).SetName("ushort");
            yield return new TestCaseData(new Foo { Bar = (int)123 }, typeof(Foo)).SetName("int");
            yield return new TestCaseData(new Foo { Bar = (uint)123 }, typeof(Foo)).SetName("uint");
            yield return new TestCaseData(new Foo { Bar = (long)123 }, typeof(Foo)).SetName("long");
            yield return new TestCaseData(new Foo { Bar = (ulong)123 }, typeof(Foo)).SetName("ulong");
            yield return new TestCaseData(new Foo { Bar = 123.45F }, typeof(Foo)).SetName("float");
            yield return new TestCaseData(new Foo { Bar = 123.45 }, typeof(Foo)).SetName("double");
            yield return new TestCaseData(new Foo { Bar = 123.45M }, typeof(Foo)).SetName("decimal");
            yield return new TestCaseData(new Foo { Bar = "abc" }, typeof(Foo)).SetName("string");
            yield return new TestCaseData(new Foo { Bar = DateTime.Now }, typeof(Foo)).SetName("DateTime");
            yield return new TestCaseData(new Foo { Bar = new Bar() }, typeof(Foo)).SetName("Custom Class");
        }

        protected override bool AlwaysEmitTypes
        {
            get
            {
                return true;
            }
        }

        [XmlInclude(typeof(Bar))]
        public class Foo
        {
            public object Bar { get; set; }
        }

        public class Bar
        {
        }
    }
}
