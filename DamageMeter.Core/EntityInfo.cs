using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class EntityInfo: ICloneable
    {
        public long TotalDamage { get; set; }
        public long FirstHit { get; set; }
        public long LastHit { get; set; }

        public long Interval => LastHit - FirstHit;

        public long VolleyOfCurse { get; set; }

        public object Clone()
        {
            var newEntityInfo = new EntityInfo
            {
                TotalDamage = TotalDamage,
                FirstHit = FirstHit,
                LastHit = LastHit
            };
            return newEntityInfo;
        }
    }
}
