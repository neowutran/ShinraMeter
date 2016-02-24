using System.Collections.Generic;
using System.Xml.Linq;

namespace Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class MonsterDatabase
    {
        private readonly Dictionary<string, Zone> _zonesData = new Dictionary<string, Zone>();

        public MonsterDatabase(string folder, string language)
        {
            var isBossOverrideXml = XDocument.Load(folder + "monsters-override.xml");
            var bossOverride = new Dictionary<string, Dictionary<string, bool>>();
            var nameOverride = new Dictionary<string, Dictionary<string, string>>();
            foreach (var zone in isBossOverrideXml.Root.Elements("Zone"))
            {
                var id = zone.Attribute("id").Value;
                bossOverride.Add(id, new Dictionary<string, bool>());
                nameOverride.Add(id, new Dictionary<string, string>());
                foreach (var monster in zone.Elements("Monster"))
                {
                    var monsterId = monster.Attribute("id").Value;
                    var isBoss = monster.Attribute("isBoss");
                    if (isBoss != null)
                    {
                        var isBossString = isBoss.Value.ToLower();
                        bossOverride[id].Add(monsterId, isBossString == "true");
                    }
                    var bossName = monster.Attribute("name");
                    if (bossName == null) continue;
                    var nameOverrideString = bossName.Value;
                    nameOverride[id].Add(monsterId, nameOverrideString);
                }
            }

            var xml = XDocument.Load(folder + "monsters-" + language + ".xml");

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
                    isBoss = isBoss.ToLower();
                    var hp = monster.Attribute("hp").Value;
                    var isBossBool = isBoss == "true";
                    if (bossOverride.ContainsKey(id) && bossOverride[id].ContainsKey(monsterId))
                    {
                        isBossBool = bossOverride[id][monsterId];
                    }
                    if (nameOverride.ContainsKey(id) && nameOverride[id].ContainsKey(monsterId))
                    {
                        monsterName = nameOverride[id][monsterId];
                    }
                    _zonesData[id].Monsters.Add(monsterId, new Monster(monsterId, monsterName, isBossBool, hp));
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