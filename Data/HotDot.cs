using System;

namespace Data
{
    public class HotDot : IEquatable<object>
    {
        public enum DotType
        {
            swch = 0, // switch on for noctineum ? other strange uses.
            seta = 1, // ?set abs stat value
            abs = 2, // each tick  HP +=HPChange ; MP += MPChange
            perc = 3, // each tick  HP += MaxHP*HPChange; MP += MaxMP*MPChange
            setp = 4  // ?set % stat value

        }

        public HotDot(int id, string type, double hp, double mp, double amount, DotType method, int time, int tick,
            string name)
        {
            Id = id;
            Type = type;
            Hp = hp;
            Mp = mp;
            Amount = amount;
            Method = method;
            Time = time;
            Tick = tick;
            Name = name;
        }

        public double Amount { get; }

        public int Id { get; }
        public string Type { get; }
        public double Hp { get; }
        public double Mp { get; }
        public DotType Method { get; }
        public int Time { get; }
        public int Tick { get; }
        public string Name { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((HotDot) obj);
        }


        public bool Equals(HotDot other)
        {
            return Id == other.Id;
        }

        public static bool operator ==(HotDot a, HotDot b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(HotDot a, HotDot b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Id.GetHashCode();
        }
    }
}