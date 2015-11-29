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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Entity) obj);
        }

        private void SetName()
        {
            Name = BasicTeraData.Instance.MonsterDatabase.Get(_npcArea + "" + _npcId);
            if (!BasicTeraData.Instance.MonsterDatabase.IsBoss(_npcArea + "" + _npcId))
            {
                Id = new EntityId(0);
            }
        }


        public override string ToString()
        {
            return Name;
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