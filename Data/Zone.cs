using System.Collections.Generic;

namespace Tera.Data
{
    public class Zone
    {
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

        public string Area { get; private set; }
        public string AreaName { get; private set; }
        public Dictionary<string, Monster> Monsters { get; private set; }
    }
}