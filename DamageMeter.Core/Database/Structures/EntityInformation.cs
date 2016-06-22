using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class EntityInformation
    {

            public EntityInformation(NpcEntity entity, long total_damage, long beginTime, long endTime)
        {
            Entity = entity;
            TotalDamage = total_damage;
            BeginTime = beginTime;
            EndTime = endTime;
        }

        public NpcEntity Entity { get; }
        public long TotalDamage { get; }
        public long BeginTime { get; }
        public long EndTime { get; }

        public long Interval => EndTime - BeginTime;
    }
}
