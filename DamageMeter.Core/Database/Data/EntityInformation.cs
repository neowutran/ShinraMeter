using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Database.Data
{
    public class EntityInformation
    {

            public EntityInformation(long total_damage, long beginTime, long endTime)
        {
            TotalDamage = total_damage;
            BeginTime = beginTime;
            EndTime = endTime;
        }
        public long TotalDamage { get; }
        public long BeginTime { get; }
        public long EndTime { get; }
    }
}
