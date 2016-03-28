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

        public int areaId;
        public int bossId;
        public long fightDuration;
        public string meterName =  "ShinraMeter";
        public string meterVersion = UpdateManager.Version;
        public double partyDps;
        public List<KeyValuePair<int, long>> debuffUptime = new List<KeyValuePair<int, long>>();
        public List<Members> members = new List<Members>();

    }
}
