using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class AreaAllowed
    {
        public int AreaId { get; set; }
        public List<int> BossIds { get; set; }

        public AreaAllowed() { }
        public AreaAllowed (int areaId, List<int> bossIds = null)
        {
            AreaId = areaId;
            BossIds = new List<int>();
            if (bossIds != null){ BossIds = bossIds; }
        }
    }
}
