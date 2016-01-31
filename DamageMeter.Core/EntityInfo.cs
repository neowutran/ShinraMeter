using System;
using System.Collections.Generic;
using System.Linq;
using Data;

namespace DamageMeter
{
    public class EntityInfo : ICloneable
    {
        public Dictionary<HotDot, AbnormalityDuration> AbnormalityTime = new Dictionary<HotDot, AbnormalityDuration>();
        public long TotalDamage { get; set; }
        public long FirstHit { get; set; }
        public long LastHit { get; set; }

        public long Interval => LastHit - FirstHit;

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