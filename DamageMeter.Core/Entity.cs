using System;
using Data;
using Tera.Game;

namespace DamageMeter
{
    public class Entity : IEquatable<object>
    {
        public NpcEntity NpcE;
        public Entity(NpcEntity npce)
        {
            NpcE = npce;
            Name = NpcE.Info.Name;
            Id = (NpcE.Info.Boss) ? NpcE.Id : new EntityId(0);
        }


        public Entity(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public EntityId Id { get; private set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return Name;
                }
                return Name + " " + DamageTracker.Instance.Interval(this);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Entity) obj);
        }

        public bool IsBoss => NpcE?.Info.Boss ?? false;

        public override string ToString()
        {
            var name = Name;
            if (!string.IsNullOrEmpty(NpcE?.Info.Area))
            {
                name += ": " + NpcE.Info.Area;
            }
            return name + "";
        }


        public bool Equals(Entity other)
        {
            return Name.Equals(other.Name) && Id == other.Id;
        }

        public static bool operator ==(Entity a, Entity b)
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

        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Id.GetHashCode();
        }
    }
}