using System.Collections.Generic;

namespace DamageMeter.TeraDpsApi
{
    public class Members
    {
        public string aggro;
        public List<List<object>> buffDetail = new List<List<object>>();
        public List<KeyValuePair<string, string>> buffUptime = new List<KeyValuePair<string, string>>();
        public string guild;
        public string healCrit;
        public string playerAverageCritRate;
        public string playerClass;
        public string playerDeathDuration;
        public string playerDeaths;
        public string playerDps;
        public uint playerId;
        public string playerName;
        public string playerServer;
        public string playerTotalDamage;
        public string playerTotalDamagePercentage;
        public List<SkillLog> skillLog = new List<SkillLog>();
        //public List<List<int>> skillCasts = new List<List<int>>();
    }
}