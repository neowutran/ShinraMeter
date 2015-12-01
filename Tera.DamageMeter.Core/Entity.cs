using System;
using Tera.Data;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class Entity : IEquatable<object>
    {
        private readonly ushort _npcArea;
        private readonly uint _npcId;


        public Entity(uint modelId, EntityId id, uint npcId, ushort npcArea)
        {
            ModelId = modelId;
            Id = id;
            _npcId = npcId;
            _npcArea = npcArea;
            SetName();
        }

        public Entity(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }


        public uint ModelId { get; }

        public EntityId Id { get; private set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return Name;
                }
                return Name + " " + DamageTracker.Instance.GetInterval(this);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Entity) obj);
        }

        public bool IsBoss()
        {
            return BasicTeraData.Instance.MonsterDatabase.IsBoss(_npcArea.ToString(), _npcId.ToString());
        }

        private void SetName()
        {
            Name = BasicTeraData.Instance.MonsterDatabase.GetMonsterName(_npcArea.ToString(), _npcId.ToString());
               AreaName = BasicTeraData.Instance.MonsterDatabase.GetAreaName(_npcArea.ToString());
            if (!BasicTeraData.Instance.MonsterDatabase.IsBoss(_npcArea.ToString(), _npcId.ToString()))
            {
                Id = new EntityId(0);
            }
        }

        public string AreaName { get; private set; }


        public override string ToString()
        {
            return Name + ": "+AreaName ;
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