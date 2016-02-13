using System;
using System.Collections.Generic;
using System.Linq;
using DamageMeter.Skills;
using DamageMeter.Skills.Skill;

namespace DamageMeter.Dealt
{
    public class EntitiesDealt : ICloneable
    {
        private Dictionary<long, Dictionary<Entity, SkillsStats>> _entitiesStats =
            new Dictionary<long, Dictionary<Entity, SkillsStats>>();

        public PlayerInfo PlayerInfo;

        public EntitiesDealt(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        public long FirstHit
        {
            get
            {
                if (_entitiesStats.Count == 0)
                {
                    return 0;
                }
                if (NetworkController.Instance.Encounter == null)
                {
                    var list = _entitiesStats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                    return (from element in list where element.Value.Any(stats => stats.Value.Damage > 0) select element.Key).FirstOrDefault();
                }
                return GetFirstHit(NetworkController.Instance.Encounter);
            }
        }

        public long LastHit
        {
            get
            {
                if (_entitiesStats.Count == 0)
                {
                    return 0;
                }
                if (NetworkController.Instance.Encounter == null)
                {
                    var list = _entitiesStats.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                    return (from element in list where element.Value.Any(stats => stats.Value.Damage > 0) select element.Key).FirstOrDefault();
                }
                return GetLastHit(NetworkController.Instance.Encounter);
                
            }
        }

        public long Interval => LastHit - FirstHit;

        public long Dps
        {
            get
            {
                if (Interval == 0)
                {
                    return Damage;
                }

                return Damage/Interval;
            }
        }


        public long Damage
        {
            get
            {
                if (NetworkController.Instance.Encounter == null)
                {
                    return _entitiesStats.Sum(timedStats => timedStats.Value.Sum(stat => stat.Value.Damage));
                }
                if (ContainsEntity(NetworkController.Instance.Encounter))
                {
                    return
                        _entitiesStats.Sum(
                            timedStats =>
                                timedStats.Value.Where(stats => stats.Key == NetworkController.Instance.Encounter)
                                    .Sum(stats => stats.Value.Damage));
                }
                return 0;
            }
        }

        public Dictionary<long, Dictionary<Skill, SkillStats>> AllSkills
        {
            get
            {
                var skills = new Dictionary<long, Dictionary<Skill, SkillStats>>();
                foreach (var timedStats in _entitiesStats)
                {
                    skills.Add(timedStats.Key, new Dictionary<Skill, SkillStats>());
                    foreach (var stats in timedStats.Value)
                    {
                        foreach (var skillStats in stats.Value.Skills)
                        {
                            if (!skills[timedStats.Key].ContainsKey(skillStats.Key))
                            {
                                skills[timedStats.Key].Add(skillStats.Key, skillStats.Value);
                            }
                            else
                            {
                                skills[timedStats.Key][skillStats.Key] += skillStats.Value;
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
                return hits == 0 ? 0 : Math.Round((double) Crits*100/hits, 1);
            }
        }

        public int Crits
        {
            get
            {
                if (NetworkController.Instance.Encounter == null || PlayerInfo.IsHealer())
                {
                    return _entitiesStats.Sum(skills => skills.Value.Sum(stat => stat.Value.Crits));
                }
                if (ContainsEntity(NetworkController.Instance.Encounter))
                {
                    return
                        _entitiesStats.Sum(
                            timedStats =>
                                timedStats.Value.Where(stats => stats.Key == NetworkController.Instance.Encounter)
                                    .Sum(stats => stats.Value.Crits));
                }
                return 0;
            }
        }

        public int Hits
        {
            get
            {
                if (NetworkController.Instance.Encounter == null || PlayerInfo.IsHealer())
                {
                    return _entitiesStats.Sum(skills => skills.Value.Sum(stat => stat.Value.Hits));
                }
                if (ContainsEntity(NetworkController.Instance.Encounter))
                {
                    return
                        _entitiesStats.Sum(
                            timedStats =>
                                timedStats.Value.Where(stats => stats.Key == NetworkController.Instance.Encounter)
                                    .Sum(stats => stats.Value.Hits));
                }
                return 0;
            }
        }

        public object Clone()
        {
            var clone = new EntitiesDealt(PlayerInfo)
            {
                _entitiesStats =
                    _entitiesStats.ToDictionary(i => i.Key,
                        i => i.Value.ToDictionary(j => j.Key, j => (SkillsStats) j.Value.Clone()))
            };

            return clone;
        }

        private long GetFirstHit(Entity entity)
        {
            long firstHit = 0;
            foreach (var timedStat in _entitiesStats)
            {
                foreach (var stat in timedStat.Value)
                {
                    if (stat.Key == entity && stat.Value.Damage > 0 && (stat.Value.FirstHit < firstHit || firstHit == 0))
                    {
                        firstHit = stat.Value.FirstHit;
                        
                    }
                }
            }
            return firstHit;
        }

        public void RemoveEntity(Entity entity)
        {
            foreach (var timedStats in _entitiesStats)
            {
                timedStats.Value.Remove(entity);
            }
        }


        public void AddStats(long time, Entity target, SkillsStats stats)
        {
            var roundedTime = time/TimeSpan.TicksPerSecond;
            if (!_entitiesStats.ContainsKey(roundedTime))
            {
                var statsDictionnary = new Dictionary<Entity, SkillsStats> {{target, stats}};
                _entitiesStats.Add(roundedTime, statsDictionnary);
                return;
            }

            if (!_entitiesStats[roundedTime].ContainsKey(target))
            {
                _entitiesStats[roundedTime].Add(target, stats);
                return;
            }

            _entitiesStats[roundedTime][target] += stats;
        }


        public Dictionary<Skill, SkillStats> GetSkills(long time, Entity target)
        {
            var roundedTime = time/TimeSpan.TicksPerSecond;
            if (!_entitiesStats.ContainsKey(roundedTime))
            {
                return null;
            }

            if (!_entitiesStats[roundedTime].ContainsKey(target))
            {
                return null;
            }

            return _entitiesStats[roundedTime][target].Skills;
        }

        public Dictionary<long, Dictionary<Skill, SkillStats>> GetSkills(Entity target)
        {
            
            var stats = new Dictionary<long, Dictionary<Skill, SkillStats>>();
            foreach (var timedStats in _entitiesStats)
            {
                if (!_entitiesStats[timedStats.Key].ContainsKey(target))
                {
                    continue;
                }

                stats.Add(timedStats.Key, new Dictionary<Skill, SkillStats>());
                foreach (var skillStats in timedStats.Value[target].Skills)
                {

                    if (!stats[timedStats.Key].ContainsKey(skillStats.Key))
                    {
                        stats[timedStats.Key].Add(skillStats.Key, skillStats.Value);
                    }
                    else
                    {
                        stats[timedStats.Key][skillStats.Key] += skillStats.Value;
                    }
                }
            }

            return stats;
            
        }

        private long GetLastHit(Entity entity)
        {
            long lastHit = 0;
            foreach (var timedStat in _entitiesStats)
            {
                foreach (var stat in timedStat.Value)
                {
                    if (stat.Key == entity && stat.Value.Damage > 0 && (stat.Value.LastHit > lastHit))
                    {
                        lastHit = stat.Value.LastHit;
                    }
                }
            }
            return lastHit;
        }

        public bool ContainsEntity(Entity entity)
        {
            return _entitiesStats.Any(timedstats => timedstats.Value.Any(entities => entities.Key == entity));
        }

        public double DamageFraction(long totalDamage)
        {
            return totalDamage == 0 ? 0 : Math.Round((double) Damage*100/totalDamage, 1);
        }

        public void SetPlayerInfo(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
            foreach (var entityStats in _entitiesStats)
            {
                foreach (var stats in entityStats.Value)
                {
                    stats.Value.SetPlayerInfo(playerInfo);
                }
            }
        }
    }
}