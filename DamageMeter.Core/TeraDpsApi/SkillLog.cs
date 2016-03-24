using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.TeraDpsApi
{
    public class SkillLog
    {
        public int skillId;
        public int skillHits;
        public long skillTotalDamage;
        public double skillCritRate;
        public double skillDamagePercent;
        public long skillHighestCrit;
        public long skillLowestCrit;
        public long skillAverageCrit;
        public long skillAverageWhite;

    }
}
