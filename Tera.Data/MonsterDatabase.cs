using System.Collections.Concurrent;
using System.IO;

namespace Tera.Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class MonsterDatabase
    {
        private readonly ConcurrentDictionary<string, string> _monsterData =
            new ConcurrentDictionary<string, string>();

        private ZoneDatabase _zoneDatabase;

        public MonsterDatabase(string filename, string filenameZone)
        {
            _zoneDatabase = new ZoneDatabase(filenameZone);
            var reader = new StreamReader(File.OpenRead(filename));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split(';');
                var zone = values[0];
                var id = zone + "" + values[1];
                var name = values[2]+": "+_zoneDatabase.Get(zone);
 
                _monsterData.TryAdd(id, name);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public string Get(string monsterId)
        {
            string monsterName;
            _monsterData.TryGetValue(monsterId, out monsterName);
            return monsterName ?? (monsterId);
        }
    }
}