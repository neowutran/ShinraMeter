using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Data
{
    public class HotDotDatabase
    {
        public enum HotDot
        {
            Dot = 131071,
            Hot = 65536,
          //  SystemHot = 655360, // natural regen
          //  CrystalHpHot = 196608,   Not 
          //  StuffMpHot = 262144,
          //  NaturalMpRegen = 0
        }

        private readonly Dictionary<int, Data.HotDot> _hotdots =
            new Dictionary<int, Data.HotDot>();


        public HotDotDatabase(string folder, string language)
        {
            var reader = new StreamReader(File.OpenRead(folder + "hotdot-" + language + ".tsv"));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split('\t');
                var id = int.Parse(values[0]);
                var effectId = int.Parse(values[1]);
                var hp = double.Parse(values[2], CultureInfo.InvariantCulture);
                var mp = double.Parse(values[3], CultureInfo.InvariantCulture);
                var method = (Data.HotDot.DotType) Enum.Parse(typeof (Data.HotDot.DotType), values[4]);
                var time = int.Parse(values[5]);
                var tick = int.Parse(values[6]);
                var name = values[7];
                _hotdots[id] = new Data.HotDot(id, effectId, hp, mp, method, time, tick, name);
            }
        }

        public Data.HotDot Get(int skillId)
        {
            if (!_hotdots.ContainsKey(skillId))
            {
                /*
                var name = "";
                switch (skillId)
                {
                    case (int) HotDot.CrystalHpHot*-1:
                        name = "Crystal regen HP";
                        break;
                    case (int) HotDot.NaturalMpRegen*-1:
                        name = "Natural regen";
                        break;
                    case (int) HotDot.SystemHot*-1:
                        name = "System hot/dot";
                        break;
                    case (int) HotDot.StuffMpHot*-1:
                        name = "Stuff regen MP";
                        break;
                }


                return new Data.HotDot(skillId, 0, 0, 0, Data.HotDot.DotType.abs, 999999, 1, name);
                */
                return null;
            }
            return _hotdots[skillId];
        }
    }
}