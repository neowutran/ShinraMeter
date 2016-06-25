using System.Collections.Generic;
using Tera.Game;
using Tera.Game.Abnormality;
using Skill = DamageMeter.Database.Structures.Skill;

namespace DamageMeter.TeraDpsApi
{
    internal class ExtendedStats
    {
        public Dictionary<HotDot, AbnormalityDuration> Debuffs = new Dictionary<HotDot, AbnormalityDuration>();
        public SortedDictionary<string, PlayerAbnormals> PlayerBuffs = new SortedDictionary<string, PlayerAbnormals>();
        public SortedDictionary<string, List<Skill>> PlayerSkills = new SortedDictionary<string, List<Skill>>();
        public EncounterBase BaseStats { get; set; }
        public NpcEntity Entity { get; set; }
        public long FirstTick { get; set; }
        public long LastTick { get; set; }
    }
}