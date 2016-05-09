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

        public override string ToString()
        {
            string message = "";
            message += playerServer + ";";
            message += playerDeaths + ";";
            message += playerDeathDuration + ";";
            message += aggro + ";";
            message += healCrit + ";";
            message += playerDps + ";";
            message += playerClass + ";";
            message += playerTotalDamage + ";";
            message += playerTotalDamagePercentage + ";";
            message += playerAverageCritRate + ";";

            foreach(var buff in buffUptime)
            {
                message += buff.Key + ":" + buff.Value + ";";
            }

            foreach(var skill in skillLog)
            {
                message += "[" + skill + "]";
            }

            return message;
        }
    }
}
