using System;
using Tera.Data;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class Entity : IEquatable<object>
    {
        private readonly uint _npcId;
        private readonly ushort _npcType;


        public Entity(uint modelId, EntityId id, uint npcId, ushort npcType)
        {
            ModelId = modelId;
            Id = id;
            _npcId = npcId;
            _npcType = npcType;
            SetName();
        }

        public Entity(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public uint ModelId { get; }

        public EntityId Id { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Entity) obj);
        }

        private void SetName()
        {
            Name = BasicTeraData.Instance.MonsterDatabase.Get(_npcType + "" + _npcId);
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Entity other)
        {
            return Name.Equals(other.Name);
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
            return Name.GetHashCode();
        }
    }
}