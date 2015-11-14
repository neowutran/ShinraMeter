using System;

namespace Tera.Game
{
    public struct Angle
    {
        private readonly ushort _raw;

        public Angle(ushort raw)
            : this()
        {
            _raw = raw;
        }

        public double Radians
        {
            get { return _raw*(2*Math.PI/0x10000); }
        }
    }
}