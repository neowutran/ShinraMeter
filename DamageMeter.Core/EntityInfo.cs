using System;
using System.Collections.Generic;
using System.Linq;
using Tera.Game;

namespace DamageMeter
{
    public class EntityInfo : ICloneable
    {
        public Dictionary<HotDot, AbnormalityDuration> AbnormalityTime = new Dictionary<HotDot, AbnormalityDuration>();
        public long FirstHit { get; set; }
        public long LastHit { get; set; }
        public Player LastAggro { get; set; }

        public long Interval => LastHit - FirstHit;

        public object Clone()
        {
            var newEntityInfo = new EntityInfo
            {
                FirstHit = FirstHit,
                LastHit = LastHit,
                AbnormalityTime = AbnormalityTime.ToDictionary(i => i.Key, i => (AbnormalityDuration)i.Value.Clone()),
                LastAggro = LastAggro
            };
            return newEntityInfo;
        }
    }
}