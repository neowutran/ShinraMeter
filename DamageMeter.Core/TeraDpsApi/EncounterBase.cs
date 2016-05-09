using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamageMeter.AutoUpdate;

namespace DamageMeter.TeraDpsApi
{
    public class EncounterBase
    {

        public string areaId;
        public string bossId;
        public string fightDuration;
        public string meterName =  "ShinraMeter";
        public string meterVersion = UpdateManager.Version;
        public string partyDps;
        public List<KeyValuePair<string, string>> debuffUptime = new List<KeyValuePair<string, string>>();
        public List<Members> members = new List<Members>();

        public override string ToString()
        {
            string message = "";
            message += areaId + ";";
            message += bossId + ";";
            message += fightDuration + ";";
            message += meterName + ";";
            message += meterVersion + ";";
            message += partyDps + ";";

            message += "[";
            foreach(var debuff in debuffUptime)
            {
                message += debuff.Key + ":" + debuff.Value + ";";
            }
            message += "]";

            foreach (var member in members)
            {
                message += "["+members+"]";
            }
            return message;
        }

    }
}
