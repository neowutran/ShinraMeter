using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.TeraDpsApi
{
    public class SkillLog
    {
        public string skillId;
        public string skillHits;
        public string skillTotalDamage;
        public string skillCritRate;
        public string skillDamagePercent;
        public string skillHighestCrit;
        public string skillLowestCrit;
        public string skillAverageCrit;
        public string skillAverageWhite;
        public override string ToString()
        {
            string message = "";
            message += skillId + ";";
            message += skillHits + ";";
            message += skillTotalDamage + ";";
            message += skillCritRate + ";";
            message += skillDamagePercent + ";";
            message += skillHighestCrit + ";";
            message += skillLowestCrit + ";";
            message += skillAverageCrit + ";";
            message += skillAverageWhite + ";";

            return message;
        }

    }
}
