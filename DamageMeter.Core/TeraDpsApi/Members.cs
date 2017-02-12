using System.Collections.Generic;

namespace DamageMeter.TeraDpsApi
{
    public class Members
    {
        public string guild;
        public string aggro;
        public string healCrit;
        public string playerAverageCritRate;
        public string playerClass;
        public string playerDeathDuration;
        public string playerDeaths;
        public string playerDps;
        public string playerName;
        public string playerServer;
        public string playerTotalDamage;
        public string playerTotalDamagePercentage;
        public List<KeyValuePair<string, string>> buffUptime = new List<KeyValuePair<string, string>>();
        public List<SkillLog> skillLog = new List<SkillLog>();
    }
}