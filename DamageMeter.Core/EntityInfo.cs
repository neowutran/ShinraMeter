using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;

namespace DamageMeter
{
    public class EntityInfo: ICloneable
    {
        public long TotalDamage { get; set; }
        public long FirstHit { get; set; }
        public long LastHit { get; set; }

        public long Interval => LastHit - FirstHit;

        public Dictionary<HotDot, long> AbnormalityTime = new Dictionary<HotDot, long>(); 

        public object Clone()
        {
            var newEntityInfo = new EntityInfo
            {
                TotalDamage = TotalDamage,
                FirstHit = FirstHit,
                LastHit = LastHit,
                AbnormalityTime = AbnormalityTime.ToDictionary(i => i.Key, i => i.Value)
            };
            return newEntityInfo;
        }
    }
}
