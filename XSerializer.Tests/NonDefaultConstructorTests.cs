using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class NonDefaultConstructorTests
    {
        [Test]
        public void Foobar()
        {
            var foo = new Foo()
            {
                Thing = Tuple.Create(1),
                OtherThings = new List<Tuple<int>>
                {
                    Tuple.Create(2),
                    Tuple.Create(3)
                }
            };

            var serialzier = new XmlSerializer<Foo>(o => o.Indent());

            var xml = serialzier.Serialize(foo);

            var roundTrip = serialzier.Deserialize(xml);

            //var s = new XmlSerializer<int>();
            //var x = s.Serialize(123);
            //var i = s.Deserialize(x);

            //Assert.That(roundTrip.Item1, Is.EqualTo(tuple.Item1));
        }

        public class Foo
        {
            public Tuple<int> Thing { get; set; }
            public List<Tuple<int>> OtherThings { get; set; }
        }
    }
}