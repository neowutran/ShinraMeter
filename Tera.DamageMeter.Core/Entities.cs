using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.DamageMeter
{
    public class Entities
    {
        public Dictionary<Entity, SkillsStats> EntitiesStats = new Dictionary<Entity, SkillsStats>();

        public long Damage
        {
            get
            {
                lock (EntitiesStats)
                {
                    return EntitiesStats.Sum(entityStats => entityStats.Value.Damage);
                }
            }
        }

        public Dictionary<Skill,SkillStats> AllSkills
        {
            get
            {
                Dictionary<Skill, SkillStats> skills = new Dictionary<Skill, SkillStats>();

                lock (EntitiesStats)
                {
                    foreach (var entities in EntitiesStats)
                    {
                        foreach (var skill in entities.Value.Skills)
                        {
                            if (skills.ContainsKey(skill.Key))
                            {
                                skills[skill.Key] += skill.Value;
                            }
                            else
                            {
                                skills[skill.Key] = skill.Value;
                            }
                        }
                    }
                }
                return skills;

            }
        }

        public double CritRate
        {
            get
            {
                var hits = Hits;
                return hits == 0 ? 0 : Math.Round((double)Crits * 100 / hits, 1);
            }
        }
        public int Crits
        {
            get
            {
                lock (EntitiesStats)
                {
                    return EntitiesStats.Sum(skills => skills.Value.Crits);
                }
            }
        }

        public int Hits
        {
            get
            {
                lock (EntitiesStats)
                {
                    return EntitiesStats.Sum(skills => skills.Value.Hits);
                }
            }
        }
    }
}
