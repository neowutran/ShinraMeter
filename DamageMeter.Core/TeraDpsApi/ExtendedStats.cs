using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;


namespace DamageMeter.TeraDpsApi
{
    class ExtendedStats
    {
        public Dictionary<HotDot,AbnormalityDuration> Debuffs = new Dictionary<HotDot, AbnormalityDuration>();
        public SortedDictionary<string, PlayerAbnormals> PlayerBuffs = new SortedDictionary<string, PlayerAbnormals>();
        public SortedDictionary<string, Dictionary<long, Dictionary<Skill, SkillStats>>> PlayerSkills = new SortedDictionary<string, Dictionary<long, Dictionary<Skill, SkillStats>>>();
        public EncounterBase BaseStats { get; set; }
        public NpcEntity Entity { get; set; }
        public long FirstTick { get; set; }
        public long LastTick { get; set; }
    }
}
