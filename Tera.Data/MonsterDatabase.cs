using System.Collections.Generic;
using System.IO;

namespace Tera.Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class MonsterDatabase
    {
        private readonly Dictionary<uint, string> _monsterData =
            new Dictionary<uint, string>();


        public MonsterDatabase(string filename)
        {
            var reader = new StreamReader(File.OpenRead(filename));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split(';');

                _monsterData.Add(uint.Parse(values[0]), values[1]);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public string Get(uint monsterId)
        {
            string monsterName;
            _monsterData.TryGetValue(monsterId, out monsterName);
            if (monsterName == null)
            {
                monsterName = monsterId.ToString();
            }
            return monsterName;
        }
    }
}