using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class MonsterDatabase
    {
        private readonly Dictionary<string, Zone> _zonesData = new Dictionary<string, Zone>();

        public MonsterDatabase(string folder,string language)
        {
            var xml = XDocument.Load(folder+"monsters-"+language+".xml");

            foreach (var zone in xml.Root.Elements("Zone"))
            {
                var id = zone.Attribute("id").Value;
                var name = zone.Attribute("name").Value;
                _zonesData.Add(id, new Zone(id, name));
                foreach (var monster in zone.Elements("Monster"))
                {
                    var monsterId = monster.Attribute("id").Value;
                    var monsterName = monster.Attribute("name").Value;
                    var isBoss = monster.Attribute("isBoss").Value;
                    var hp = monster.Attribute("hp").Value;
                    _zonesData[id].Monsters.Add(monsterId, new Monster(monsterId,monsterName,isBoss == "True", hp));
                }
            }
        }

        public string GetAreaName(string areaId)
        {
            Zone zone;
            _zonesData.TryGetValue(areaId, out zone);
            return zone == null ? areaId : zone.AreaName;
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public string GetMonsterName(string areaId, string monsterId)
        {
            Monster monster;
            Zone zone;
            _zonesData.TryGetValue(areaId, out zone);
            if (zone == null)
            {
                return monsterId;
            }
            zone.Monsters.TryGetValue(monsterId, out monster);
            if (monster == null)
            {
                return monsterId;
            }
            return string.IsNullOrEmpty(monster.Name) ? monsterId : monster.Name;
        }

        public bool IsBoss(string areaId, string monsterId)
        {
            Monster monster;
            Zone zone;
            _zonesData.TryGetValue(areaId, out zone);
            if (zone == null)
            {
                return false;
            }
            zone.Monsters.TryGetValue(monsterId, out monster);
            return monster != null && monster.Boss;
        }
    }
}