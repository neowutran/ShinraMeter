using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.TeraDpsApi
{
    public class EncounterBase
    {

        public int areaId;
        public int bossId;
        public string server;
        public List<KeyValuePair<int, long>> debuffUptime = new List<KeyValuePair<int, long>>();
        public List<Members> members = new List<Members>();

    }
}
