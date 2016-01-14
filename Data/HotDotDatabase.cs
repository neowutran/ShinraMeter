using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Data
{
    public class HotDotDatabase
    {
        private readonly Dictionary<int, HotDot> _hotdots =
            new Dictionary<int, HotDot>();


        public HotDotDatabase(string folder, string language)
        {

            var reader = new StreamReader(File.OpenRead(folder+"hotdot-"+language+".tsv"));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split('\t');
                var id = int.Parse(values[0]);
                var effectId = int.Parse(values[1]);
                var hp = double.Parse(values[2], CultureInfo.InvariantCulture);
                var mp = double.Parse(values[3], CultureInfo.InvariantCulture);
                var method = (HotDot.DotType)Enum.Parse(typeof(HotDot.DotType), values[4]);
                var time = int.Parse(values[5]);
                var tick = int.Parse(values[6]);
                var name = values[7];
                _hotdots[id] = new HotDot(id, effectId, hp, mp, method, time, tick, name);
            }
        }

        public HotDot Get(int skillId)
        {
            return !_hotdots.ContainsKey(skillId) ? null : _hotdots[skillId];
        }
    }
}