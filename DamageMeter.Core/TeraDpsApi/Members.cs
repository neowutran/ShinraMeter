using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.TeraDpsApi
{
    public class Members
    {
        public string playerServer;
        public string playerDeaths;
        public string playerDeathDuration;
        public string aggro;
        public string healCrit;

        public string playerName;
        public string playerDps;
        public string playerClass;
        public string playerTotalDamage;
        public string playerTotalDamagePercentage;
        public string playerAverageCritRate;
        public List<KeyValuePair<string, string>> buffUptime = new List<KeyValuePair<string, string>>();
        public List<SkillLog> skillLog = new List<SkillLog>();
    }
}
