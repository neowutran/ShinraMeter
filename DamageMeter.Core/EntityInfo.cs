using System;
using System.Collections.Generic;
using System.Linq;
using Tera.Game;

namespace DamageMeter
{
    public class EntityInfo : ICloneable
    {
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
                LastAggro = LastAggro
            };
            return newEntityInfo;
        }
    }
}