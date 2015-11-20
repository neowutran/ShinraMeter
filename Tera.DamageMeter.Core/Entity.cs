using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Data;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class Entity : IEquatable<Object>
    {
        private readonly uint _modelId;
        private readonly EntityId _id;
        private readonly uint _npcId;
        private readonly ushort _npcType;
        private string _name;

        public string Name => _name;

        private void setName()
        {
            _name = BasicTeraData.Instance.MonsterDatabase.Get(_modelId);
        }

        public uint ModelId => _modelId;
        public EntityId Id => _id;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Entity)obj);
        }

        public bool Equals(Entity other)
        {
            return _modelId == other._modelId && _id == other._id && _name == other._name;
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
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
            return _modelId.GetHashCode() ^ _id.GetHashCode() ^ _name.GetHashCode();
        }

        public Entity(uint modelId, EntityId id,  uint npcId, ushort npcType)
        {
            _modelId = modelId;
            _id = id;
            _npcId = npcId;
            _npcType = npcType;
            setName();
            DebugPrint();
        }

        private void DebugPrint()
        {
            Console.WriteLine("name:" + _name + ";id:" + _id + ";npcid:" + _npcId +";npctype:"+_npcType+";modelId:"+_modelId);
        }

        public Entity(string name)
        {
          _name = name;
            DebugPrint();
        }
    }
}
