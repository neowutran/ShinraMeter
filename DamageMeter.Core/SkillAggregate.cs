using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DamageMeter.Database.Structures;
using Tera.Game;
using Skill = Tera.Game.Skill;

namespace DamageMeter
{
    public class SkillAggregate
    {
        private readonly PlayerDealt _playerDealt;
        private readonly Entity _target;
        private readonly bool _timed;

        public SkillAggregate(Skill skill, Skills skillsData, EntityId source, Entity target, PlayerDealt playerDealt,
            bool timed, Database.Database.Type type)
        {
            _playerDealt = playerDealt;
            _timed = timed;
            _target = target;
            Type = type;
            Skills = new Dictionary<Skill, List<EntityId>>();
            Name = skill.ShortName;
            SkillsData = skillsData;
            Skills.Add(skill, new List<EntityId>{ source });
        }

        public Dictionary<Skill, List<EntityId>> Skills { get; }
        public string Name { get; }

        private Skills SkillsData { get; }

        private bool _playerDealtUnrelieable;

        public Database.Database.Type Type { get; }

        public bool Add(Skill skill, EntityId source)
        {
            if (skill.ShortName != Name) return false;
            if (Skills.Any(sk => skill.Id == sk.Key.Id && skill.IsHotDot == sk.Key.IsHotDot && sk.Value.Contains(source)))
            {
                return false;
            }

            if (Skills.ContainsKey(skill))
            {
                Skills[skill].Add(source);
                _playerDealtUnrelieable = true;
            }
            else
            {
                Skills.Add(skill, new List<EntityId> {source});
            }
            return true;
        }

        public long Amount()
        {
            var result = from skill in Skills
                         from source in skill.Value
                            select SkillsData.Amount(source, _target, skill.Key.Id, _timed);
            return result.Sum();
        }

        public long Amount(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.Amount(source, _target, skillId, _timed);
            return result.Sum();
        }

        public long BiggestCrit()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.BiggestCrit(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Max();
        }

        public long LowestCrit()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.LowestCrit(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Max();
        }

        public long BiggestCrit(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.BiggestCrit(source, _target, skillId, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Max();
        }

        public double DamagePercent()
        {
            if (_playerDealtUnrelieable)
            {
                throw  new Exception("Player Dealt unrelieable");
            }
            return Amount()*100/_playerDealt.Amount;
        }

        public long DamagePercent(int skillId)
        {
            if (_playerDealtUnrelieable)
            {
                throw new Exception("Player Dealt unrelieable");
            }
            return SkillsData.Amount(_playerDealt.Source.User.Id, _target, skillId, _timed)*100/Amount();
        }

        public long Hits()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.Hits(source, _target, skill.Key.Id, _timed);
            return result.Sum();
        }

        public long Hits(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.Hits(source, _target, skillId, _timed);
            return result.Sum();
        }

        public long White()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.White(source, _target, skill.Key.Id, _timed);
            var enumerable = result as int[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long White(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.White(source, _target, skill.Key.Id, _timed);
            var enumerable = result as int[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long Crits()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.Crits(source, _target, skill.Key.Id, _timed);
            var enumerable = result as int[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long Crits(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.Crits(source, _target, skill.Key.Id, _timed);
            var enumerable = result as int[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public double CritRate()
        {
            return Crits()*100/Hits();
        }

        public double CritRate(int skillId)
        {
            return Crits(skillId)*100/Hits(skillId);
        }

        public long BiggestHit()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.BiggestHit(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Max();
        }

        public long BiggestHit(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.BiggestHit(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Max();
        }

        public double Avg()
        {
            return Amount()/Hits();
        }

        public double Avg(int skillId)
        {
            return Amount(skillId)/Hits(skillId);
        }

        public long AmountWhite()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.AmountWhite(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long AmountWhite(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.AmountWhite(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long AmountCrit()
        {
            var result = from skill in Skills
                         from source in skill.Value
                         select SkillsData.AmountCrit(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long AmountCrit(int skillId)
        {
            var result = from skill in Skills
                         where skill.Key.Id == skillId
                         from source in skill.Value
                         select SkillsData.AmountCrit(source, _target, skill.Key.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public double AvgCrit()
        {
            var crits = Crits();
            if (crits == 0) return 0;
            return AmountCrit()/crits;
        }

        public double AvgCrit(int skillId)
        {
            var crits = Crits(skillId);
            if (crits == 0) return 0;
            return AmountCrit(skillId) / crits;
        }

        public double AvgWhite(int skillId)
        {
            var white = White(skillId);
            if (white == 0) return 0;
            return AmountWhite(skillId) / white;
        }

        public double AvgWhite()
        {
            var white = White();
            if (white == 0) return 0;
            return AmountWhite()/white;
        }


        public string Id()
        {
            var result = "";
            for (var i = 0; i < Skills.Count; i++)
            {
                result += Skills.ElementAt(i).Key.Id;
                if (i < Skills.Count - 1)
                {
                    result += ",";
                }
            }
            return result;
        }


        public static IEnumerable<SkillAggregate> GetAggregate(PlayerDealt playerDealt, Entity entity,
         Skills skillsData, bool timedEncounter, Database.Database.Type type)
        {

            if (type != Database.Database.Type.Damage)
            {
                timedEncounter = false;
            }

            if (!playerDealt.Source.IsHealer && type != Database.Database.Type.Damage)
            {
                var skills = skillsData.SkillsIdByTarget(playerDealt.Source.User);
                var skillsAggregate = new Dictionary<string, SkillAggregate>();
                foreach (var skill in skills)
                {
                    if (skill.Value == null) continue;
                    if (
                        skillsData.Type(skill.Key, playerDealt.Source.User, skill.Value.Id, false) !=
                        type)
                    {
                        continue;
                    }

                    if (!skillsAggregate.ContainsKey(skill.Value.ShortName))
                    {
                        skillsAggregate.Add(skill.Value.ShortName,
                            new SkillAggregate(skill.Value, skillsData, skill.Key, playerDealt.Source.User, playerDealt, false, type));
                        continue;
                    }
                    skillsAggregate[skill.Value.ShortName].Add(skill.Value, skill.Key);
                }
                return skillsAggregate.Values;
            }
            if (playerDealt.Source.IsHealer && type != Database.Database.Type.Damage)
            {
                var skills = skillsData.SkillsIdBySource(playerDealt.Source.User, null, true);
                var skillsAggregate = new Dictionary<string, SkillAggregate>();
                foreach (var skill in skills)
                {
                    if (skill == null) continue;
                    if (
                        skillsData.Type(playerDealt.Source.User.Id, null, skill.Id, true) !=
                        type)
                    {
                        continue;
                    }

                    if (!skillsAggregate.ContainsKey(skill.ShortName))
                    {
                        skillsAggregate.Add(skill.ShortName,
                            new SkillAggregate(skill, skillsData, playerDealt.Source.User.Id, null, playerDealt,
                                true, type));
                        continue;
                    }
                    skillsAggregate[skill.ShortName].Add(skill, playerDealt.Source.User.Id);
                }
                return skillsAggregate.Values;
            }
            else
            {

                var skills = skillsData.SkillsIdBySource(playerDealt.Source.User, entity, timedEncounter);
                var skillsAggregate = new Dictionary<string, SkillAggregate>();
                foreach (var skill in skills)
                {
                    if (skill == null) continue;
                    if (
                        skillsData.Type(playerDealt.Source.User.Id, entity, skill.Id, timedEncounter) !=
                        type)
                    {
                        continue;
                    }

                    if (!skillsAggregate.ContainsKey(skill.ShortName))
                    {
                        skillsAggregate.Add(skill.ShortName,
                            new SkillAggregate(skill, skillsData, playerDealt.Source.User.Id, entity, playerDealt,
                                timedEncounter, type));
                        continue;
                    }
                    skillsAggregate[skill.ShortName].Add(skill, playerDealt.Source.User.Id);
                }
                return skillsAggregate.Values;
            }
        }

    }
}