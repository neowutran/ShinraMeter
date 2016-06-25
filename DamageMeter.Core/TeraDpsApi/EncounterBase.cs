using System.Collections.Generic;
using DamageMeter.AutoUpdate;

namespace DamageMeter.TeraDpsApi
{
    public class EncounterBase
    {
        public string AreaId;
        public string BossId;
        public List<KeyValuePair<string, string>> DebuffUptime = new List<KeyValuePair<string, string>>();
        public string FightDuration;
        public List<Members> Members = new List<Members>();
        public string MeterName = "ShinraMeter";
        public string MeterVersion = UpdateManager.Version;
        public string PartyDps;
    }
}