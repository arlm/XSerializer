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

    public class Foo2
    {
        private readonly string _corge;
        private readonly double _grault;
        private readonly List<Baz2> _bazes;
        private readonly Bar2 _bar;
        private readonly bool _garply;

        public Foo2(
            string corge,
            double grault,
            List<Baz2> bazes,
            Bar2 bar,
            bool garply)
        {
            _corge = corge;
            _grault = grault;
            _bazes = bazes;
            _bar = bar;
            _garply = garply;
        }

        public string Corge
        {
            get { return _corge; }
        }

        public double Grault
        {
            get { return _grault; }
        }

        public bool Garply
        {
            get { return _garply; }
        }

        public Bar2 Bar
        {
            get { return _bar; }
        }

        public List<Baz2> Bazes
        {
            get { return _bazes; }
        }
    }

    public class Bar2
    {
        private readonly DateTime _waldo;
        private readonly Guid _fred;

        public Bar2(DateTime waldo, Guid fred)
        {
            _waldo = waldo;
            _fred = fred;
        }

        public DateTime Waldo
        {
            get { return _waldo; }
        }

        public Guid Fred
        {
            get { return _fred; }
        }
    }

    public class Baz2
    {
        private readonly int _wibble;
        private readonly long _wobble;
        private readonly decimal _wubble;

        public Baz2(
            int wibble,
            long wobble,
            decimal wubble)
        {
            _wibble = wibble;
            _wobble = wobble;
            _wubble = wubble;
        }

        public int Wibble
        {
            get { return _wibble; }
        }

        public long Wobble
        {
            get { return _wobble; }
        }

        public decimal Wubble
        {
            get { return _wubble; }
        }
    }
}