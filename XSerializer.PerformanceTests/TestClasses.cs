using System;
using System.Collections.Generic;

namespace XSerializer.Tests.Performance
{
    public class Foo
    {
        public string Corge { get; set; }
        public double Grault { get; set; }
        public bool Garply { get; set; }
        public Bar Bar { get; set; }
        public List<Baz> Bazes { get; set; }
    }

    public class Bar
    {
        public DateTime Waldo { get; set; }
        public Guid Fred { get; set; }
    }

    public class Baz
    {
        public int Wibble { get; set; }
        public long Wobble { get; set; }
        public decimal Wubble { get; set; }
    }
}