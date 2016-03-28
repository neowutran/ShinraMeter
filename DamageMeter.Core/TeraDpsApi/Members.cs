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
        public int playerDeaths;
        public double playerDeathDuration;

        public string playerName;
        public long playerDps;
        public string playerClass;
        public long playerTotalDamage;
        public double playerTotalDamagePercentage;
        public double playerAverageCritRate;
        public List<KeyValuePair<int, long>> buffUptime = new List<KeyValuePair<int, long>>();
        public List<SkillLog> skillLog = new List<SkillLog>();
    }
}
