using System;
using System.Collections.Concurrent;
using System.Linq;
using DamageMeter.Skills.Skill;

namespace DamageMeter.Skills
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

        public long FirstHit
        {
            get
            {
                lock (Skills)
                {
                    long firstHit = 0;
                    foreach (var skill in Skills)
                    {
                        if (firstHit == 0)
                        {
                            firstHit = skill.Value.FirstHit;
                        }
                        else if (skill.Value.FirstHit < firstHit && skill.Value.FirstHit != 0)
                        {
                            firstHit = skill.Value.FirstHit;
                        }
                    }
                    return firstHit;
                }
            }
        }

        public long LastHit
        {
            get
            {
                lock (Skills)
                {
                    long lastHit = 0;
                    foreach (var skill in Skills)
                    {
                        if (lastHit == 0)
                        {
                            lastHit = skill.Value.LastHit;
                        }
                        else
                        {
                            if (skill.Value.LastHit > lastHit)
                            {
                                lastHit = skill.Value.LastHit;
                            }
                        }
                    }
                    return lastHit;
                }
            }
        }


        public long Interval => LastHit - FirstHit;

        public long Dps
        {
            get
            {
                if (Interval == 0)
                {
                    return 0;
                }
                return Damage/Interval;
            }
        }

        public ConcurrentDictionary<Skill.Skill, SkillStats> Skills { get; set; } =
            new ConcurrentDictionary<Skill.Skill, SkillStats>();

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

        public long Mana
        {
            get
            {
                long mana = 0;
                lock (Skills)
                {
                    mana += Skills.Sum(skill => skill.Value.Mana);
                }
                return mana;
            }
        }

        public long Heal
        {
            get
            {
                long heal = 0;
                lock (Skills)
                {
                    heal += Skills.Sum(skill => skill.Value.Heal);
                }
                return heal;
            }
        }

        public double CritRate
        {
            get
            {
                var hits = Hits;
                return hits == 0 ? 0 : Math.Round((double) Crits*100/hits, 1);
            }
        }

        public int Crits
        {
            get { return Skills.Sum(skill => skill.Value.Crits); }
        }

        public int Hits
        {
            get { return Skills.Sum(skill => skill.Value.Hits); }
        }

        public static SkillsStats operator +(SkillsStats c1, SkillsStats c2)
        {
            if (c1.PlayerInfo != c2.PlayerInfo)
            {
                throw new Exception("cannot add skillstats");
            }

            var skills = new SkillsStats(c1.PlayerInfo);
            skills.Skills =
                new ConcurrentDictionary<Skill.Skill, SkillStats>(skills.Skills.Concat(c1.Skills)
                    .ToDictionary(x => x.Key, x => x.Value));
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
    }
}