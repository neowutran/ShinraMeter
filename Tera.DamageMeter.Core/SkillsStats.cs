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

        public static SkillsStats operator +(SkillsStats c1, SkillsStats c2)
        {
            if (c1.PlayerInfo != c2.PlayerInfo)
            {
                throw new Exception("cannot add skillstats");
            }

            SkillsStats skills = new SkillsStats(c1.PlayerInfo);
            skills.Skills = skills.Skills.Concat(c1.Skills).ToDictionary(x => x.Key, x => x.Value);
            foreach (var skill in c2.Skills)
            {
                if (skills.Skills.ContainsKey(skill.Key))
                {
                    skills.Skills[skill.Key] += skill.Value;
                }
                else
                {
                    skills.Skills[skill.Key] = skill.Value;
                }
            }

            return skills;

        }


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
                var hits = Hits;
             return hits == 0 ? 0 : Math.Round((double) Crits*100/ hits, 1);
            }
        }

        public int Crits
        {
            get
            {
                return Skills.Sum(skill => skill.Value.Crits);
            }
        }

        public int Hits
        {
            get { return Skills.Sum(skill => skill.Value.Hits); }
        }
    }
}