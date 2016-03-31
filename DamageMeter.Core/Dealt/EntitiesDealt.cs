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

        public long FirstHit(Entity currentBoss)
        {
           
                if (_entitiesStats.Count == 0)
                {
                    return 0;
                }
                if (currentBoss == null)
                {
                    var list =  _entitiesStats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                    return
                        (from element in list
                            where element.Value.Any(stats => stats.Value.Damage > 0)
                            select element.Key).FirstOrDefault();
                }
               return GetFirstHit(currentBoss);
            
        }

        public long LastHit(Entity currentBoss)
        {
         
                if (_entitiesStats.Count == 0)
                {
                    return 0;
                }
                if (currentBoss == null)
                {
                    var list = _entitiesStats.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                    return
                        (from element in list
                            where element.Value.Any(stats => stats.Value.Damage > 0)
                            select element.Key).FirstOrDefault();
                }
                return GetLastHit(currentBoss);
            
        }

        public long GlobalDps(Entity entity, bool timedEncounter, long interval)
        {
            if (interval == 0)
            {
                return Damage(entity, timedEncounter);
            }

            return Damage(entity, timedEncounter) / interval;
        }

        public long Dps(Entity entity, bool timedEncounter)
        {
       
                if (Interval(entity) == 0)
                {
                    return Damage(entity, timedEncounter);
                }

                return Damage(entity, timedEncounter) /Interval(entity);
            
        }

        public long Interval(Entity entity)
        {
            return LastHit(entity) - FirstHit(entity);
        }

     

        public long Damage(Entity currentBoss, bool timedEncounter)
        {
        
                if (currentBoss == null)
                {
                    return _entitiesStats.Sum(timedStats => timedStats.Value.Sum(stat => stat.Value.Damage));
                }
                if (ContainsEntity(currentBoss))
                {
                    if (!timedEncounter)
                    {
                     return
                        _entitiesStats.Sum(
                            timedStats =>
                                timedStats.Value.Where(stats => stats.Key == currentBoss)
                                    .Sum(stats => stats.Value.Damage));

                }

                    long damage = 0;
                    var lastHit = LastHit(currentBoss);
                    for (var i = FirstHit(currentBoss); i <= lastHit; i++)
                    {
                        if (!_entitiesStats.ContainsKey(i)) continue;
                        damage += _entitiesStats[i].Sum(stat => stat.Value.Damage);
                    }
                    return damage;
                    
                }
                return 0;
            
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

        public double CritRate(Entity entity, bool timedEncounter)
        {
            var hits = Hits(entity, timedEncounter);
            return hits == 0 ? 0 : Math.Round((double)Crits(entity, timedEncounter) * 100 / hits, 1);
        }


        public int Crits(Entity currentBoss, bool timedEncounter)
        {
            
                if (currentBoss == null || (PlayerInfo.IsHealer && !timedEncounter))
                {
                    return _entitiesStats.Sum(skills => skills.Value.Sum(stat => stat.Value.Crits));
                }
           
                if (!timedEncounter)
                {
                    return _entitiesStats.Sum(
                             timedStats =>
                                 timedStats.Value.Where(stats => stats.Key == currentBoss)
                                     .Sum(stats => stats.Value.Crits));
                }

                var crit = 0;
                var lastHit = GetLastHit(currentBoss);
                for (var i = GetFirstHit(currentBoss); i <= lastHit; i++)
                {
                    if (!_entitiesStats.ContainsKey(i)) continue;
                    crit += _entitiesStats[i].Sum(stat => stat.Value.Crits);
                }
                return crit;
                
            
        }
        public long DmgBiggestCrit(Entity currentBoss, bool timedEncounter)
        {
            
                if (currentBoss == null )
                    return _entitiesStats.SelectMany(x => x.Value).SelectMany(x => x.Value.Skills).Select(x => x.Value.DmgBiggestCrit).Concat(new long[] { 0 }).Max();
                if (!timedEncounter)
                    return _entitiesStats.SelectMany(x => x.Value).Where(stats => stats.Key == currentBoss).SelectMany(x => x.Value.Skills).Select(x => x.Value.DmgBiggestCrit).Concat(new long[] { 0 }).Max();
                return _entitiesStats.Where(x => x.Key >= FirstHit(currentBoss) && x.Key <= LastHit(currentBoss)).SelectMany(x => x.Value).SelectMany(x => x.Value.Skills).Select(x => x.Value.DmgBiggestCrit).Concat(new long[] { 0 }).Max();
            
        }

        public int Hits(Entity currentBoss, bool timedEncounter)
        {
          
                if (currentBoss == null || (PlayerInfo.IsHealer && !timedEncounter))
                {
                    return _entitiesStats.Sum(skills => skills.Value.Sum(stat => stat.Value.Hits));
                }
                
                if (!timedEncounter)
                {
                    return
                     _entitiesStats.Sum(
                         timedStats =>
                             timedStats.Value.Where(stats => stats.Key == currentBoss)
                                 .Sum(stats => stats.Value.Hits));
            }
                var hits = 0;
                var lastHit = GetLastHit(currentBoss);
                for (var i = GetFirstHit(currentBoss); i <= lastHit; i++)
                {
                    if (!_entitiesStats.ContainsKey(i)) continue;
                    hits += _entitiesStats[i].Sum(stat => stat.Value.Hits);
                }
                return hits;

            
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

        public long GetFirstHit(Entity entity)
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

        public Dictionary<long, Dictionary<Skill, SkillStats>> GetSkillsByTime(Entity target)
        {
            var firstHit = GetFirstHit(target);
            var lastHit = GetLastHit(target);
            
            var stats = new Dictionary<long, Dictionary<Skill, SkillStats>>();

            for (var i = firstHit; i <= lastHit; i++)
            {
                
                if (!_entitiesStats.ContainsKey(i)) continue;
                stats[i] = new Dictionary<Skill, SkillStats>();
                foreach (var skillsEntities in _entitiesStats[i])
                {
                    foreach (var skills in skillsEntities.Value.Skills)
                    {
                        if (!stats[i].ContainsKey(skills.Key))
                        {
                            stats[i].Add(skills.Key, skills.Value);
                        }
                        else
                        {
                            stats[i][skills.Key] += skills.Value;
                        }
                    }
                  
                    
                }
            }

            return stats;
        }

        public long GetLastHit(Entity entity)
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

        public double DamageFraction(Entity currentBoss, long totalDamage, bool timedEncounter)
        {
            return totalDamage == 0 ? 0 : Math.Round((double) Damage(currentBoss, timedEncounter) *100/totalDamage, 1);
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