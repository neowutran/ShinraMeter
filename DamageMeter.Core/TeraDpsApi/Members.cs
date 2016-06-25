using System.Collections.Generic;

namespace DamageMeter.TeraDpsApi
{
    public class Members
    {
        public string Aggro;
        public List<KeyValuePair<string, string>> BuffUptime = new List<KeyValuePair<string, string>>();
        public string HealCrit;
        public string PlayerAverageCritRate;
        public string PlayerClass;
        public string PlayerDeathDuration;
        public string PlayerDeaths;
        public string PlayerDps;

        public string PlayerName;
        public string PlayerServer;
        public string PlayerTotalDamage;
        public string PlayerTotalDamagePercentage;
        public List<SkillLog> SkillLog = new List<SkillLog>();
    }
}