using System;
using Data;
using Tera.Game;

namespace DamageMeter
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

        public bool IsGroup { get; }

        public Entity(uint modelId, EntityId id, uint npcId, ushort npcArea, bool group) : this(modelId, id, npcId, npcArea)
        {
            IsGroup = group;
        }

        public Entity GetGroup()
        {
            return !IsBoss() ? null : new Entity(0,new EntityId(0), 0, 0, true);
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

        public string AreaName { get; private set; }

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
            if (!IsBoss())
            {
                Id = new EntityId(0);
            }
        }


        public override string ToString()
        {
            var name = Name;
            if (!string.IsNullOrEmpty(AreaName))
            {
                name += ": " + AreaName;
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