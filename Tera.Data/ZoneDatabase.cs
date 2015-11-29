using System.Collections.Concurrent;
using System.IO;

namespace Tera.Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class ZoneDatabase
    {
        private readonly ConcurrentDictionary<string, string> _zoneData =
            new ConcurrentDictionary<string, string>();


        public ZoneDatabase(string filename)
        {
            var reader = new StreamReader(File.OpenRead(filename));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split(';');
                var zoneId = values[0];
                var zoneName = values[1];

                _zoneData.TryAdd(zoneId, zoneName);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public string Get(string zoneId)
        {
            string zoneName;
            _zoneData.TryGetValue(zoneId, out zoneName);
            return zoneName ?? (zoneId);
        }
    }
}