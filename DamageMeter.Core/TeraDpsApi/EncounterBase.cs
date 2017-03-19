using System.Collections.Generic;
using DamageMeter.AutoUpdate;

namespace DamageMeter.TeraDpsApi
{
    public class EncounterBase
    {
        public long encounterUnixEpoch;
        public string areaId;
        public string bossId;
        public string fightDuration;
        public string meterName = "ShinraMeter";
        public string meterVersion = UpdateManager.Version;
        public string partyDps;
        public string uploader; //zero-based index of uploader in members list
        public List<KeyValuePair<string, string>> debuffUptime = new List<KeyValuePair<string, string>>();
        public List<Members> members = new List<Members>();
    }
}