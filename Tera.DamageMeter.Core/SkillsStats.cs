using System;
using System.Collections.Generic;
using System.Linq;

namespace Tera.DamageMeter
{
    public class SkillsStats
    {
        public SkillsStats()
        {
        }


        public SkillsStats(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        public Dictionary<Skill, SkillStats> Skills { get; set; } =
            new Dictionary<Skill, SkillStats>();

        public PlayerInfo PlayerInfo { get; }


        public long Damage
        {
            get
            {
                long damage = 0;
                lock (Skills)
                {
                    damage += Skills.Sum(skill => skill.Value.Damage);
                }
                return damage;
            }
        }

        public double CritRate
        {
            get
            {
                var crit = 0;
                var hits = 0;
                foreach (var skill in Skills)
                {
                    crit += skill.Value.Crits;
                    hits += skill.Value.Hits;
                }

                if (hits == 0)
                {
                    return 0;
                }
                return Math.Round((double) crit*100/hits, 1);
            }
        }
    }
}