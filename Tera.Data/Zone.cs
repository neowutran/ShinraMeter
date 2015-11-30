using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Data
{
    public class Zone
    {
        public string Area { get; private set; }
        public string AreaName { get; private set; }
        public Dictionary<string, Monster> Monsters { get; private set; }  
        public Zone(string area, string areaName)
        {
            Area = area;
            AreaName = areaName;
            if (string.IsNullOrEmpty(areaName))
            {
                AreaName = area;
            }
            Monsters = new Dictionary<string, Monster>();
        }
    }
}
