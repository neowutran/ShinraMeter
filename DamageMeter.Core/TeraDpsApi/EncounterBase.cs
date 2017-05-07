using System.Collections.Generic;
using DamageMeter.AutoUpdate;

namespace DamageMeter.TeraDpsApi
{
    public class EncounterBase
    {
        public string areaId;
        public string bossId;
        public List<List<object>> debuffDetail = new List<List<object>>();
        public List<KeyValuePair<string, string>> debuffUptime = new List<KeyValuePair<string, string>>();
        public long encounterUnixEpoch;
        public string fightDuration;
        public List<Members> members = new List<Members>();
        public string meterName = "ShinraMeter";
        public string meterVersion = UpdateManager.Version;
        public string partyDps;
        public string uploader; //zero-based index of uploader in members list
    }
}