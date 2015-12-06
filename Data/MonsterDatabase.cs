using System.Collections.Generic;
using System.IO;

namespace Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class MonsterDatabase
    {
        private readonly Dictionary<string, Zone> _zonesData = new Dictionary<string, Zone>();

        public MonsterDatabase(string folder)
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.tsv"))
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var nameElements = filename.Split('-');
                var area = nameElements[0];
                var areaname = nameElements[1];
                var monsters = new StreamReader(File.OpenRead(file));
                var zone = new Zone(area, areaname);
                _zonesData.Add(area, zone);

                while (!monsters.EndOfStream)
                {
                    var line = monsters.ReadLine();
                    if (line == null) continue;
                    var values = line.Split('\t');
                    zone.Monsters.Add(values[0], new Monster(values[0], values[1], bool.Parse(values[2])));
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