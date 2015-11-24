using System.Collections.Generic;
using System.IO;

namespace Tera.Game
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class MonsterDatabase
    {
        private readonly Dictionary<int, string> _monsterData =
            new Dictionary<int, string>();


        public MonsterDatabase(string filename)
        {
            var reader = new StreamReader(File.OpenRead(filename));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split(';');

                _monsterData.Add(int.Parse(values[0]), values[1]);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public string Get(int monsterId)
        {
            string monsterName;
            _monsterData.TryGetValue(monsterId, out monsterName);
            return monsterName ?? (monsterId.ToString());
        }
    }
}