using System.Collections.Generic;
using System.Linq;
using DamageMeter.Database.Structures;
using Skill = Tera.Game.Skill;

namespace DamageMeter
{
    public class SkillAggregate
    {
        private readonly EntityInformation _entityInformation;
        private readonly PlayerDealt _playerDealt;
        private readonly bool _timed;

        public SkillAggregate(Skill skill, Skills skillsData, PlayerDealt playerDealt,
            EntityInformation entityInformation, bool timed, Database.Database.Type type)
        {
            _playerDealt = playerDealt;
            _timed = timed;
            Type = type;
            _entityInformation = entityInformation;
            Skills = new List<Skill>();
            Name = skill.ShortName;
            SkillsData = skillsData;
            Skills.Add(skill);
        }

        public List<Skill> Skills { get; }
        public string Name { get; }

        private Skills SkillsData { get; }

        public Database.Database.Type Type { get; }

        public bool Add(Skill skill)
        {
            if (skill.ShortName == Name)
            {
                foreach (var sk in Skills)
                {
                    if (skill.Id == sk.Id && skill.IsHotDot == sk.IsHotDot)
                    {
                        return false;
                    }
                }

                Skills.Add(skill);
                return true;
            }

            return false;
        }

        public long Amount()
        {
            var result = from skill in Skills
                select SkillsData.Amount(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            return result.Sum();
        }

        public long Amount(int skillId)
        {
            return SkillsData.Amount(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long BiggestCrit()
        {
            var result = from skill in Skills
                select SkillsData.BiggestCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Max();
        }

        public long BiggestCrit(int skillId)
        {
            return SkillsData.BiggestCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double DamagePercent()
        {
            return Amount()*100/_playerDealt.Amount;
        }

        public long DamagePercent(int skillId)
        {
            return SkillsData.Amount(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed)*100/Amount();
        }

        public long Hits()
        {
            var result = from skill in Skills
                select SkillsData.Hits(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            return result.Sum();
        }

        public long Hits(int skillId)
        {
            return SkillsData.Hits(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long White()
        {
            var result = from skill in Skills
                select SkillsData.White(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            var enumerable = result as int[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long White(int skillId)
        {
            return SkillsData.White(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long Crits()
        {
            var result = from skill in Skills
                select SkillsData.Crits(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            var enumerable = result as int[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long Crits(int skillId)
        {
            return SkillsData.Crits(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double CritRate()
        {
            return Crits()*100/Hits();
        }

        public double CritRate(int skillId)
        {
            return SkillsData.CritRate(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long BiggestHit()
        {
            var result = from skill in Skills
                select SkillsData.BiggestHit(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Max();
        }

        public long BiggestHit(int skillId)
        {
            return SkillsData.BiggestHit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double Avg()
        {
            return Amount()/Hits();
        }

        public double Avg(int skillId)
        {
            return SkillsData.Average(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long AmountWhite()
        {
            var result = from skill in Skills
                select SkillsData.AmountWhite(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long AmountWhite(int skillId)
        {
            return SkillsData.AmountWhite(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long AmountCrit()
        {
            var result = from skill in Skills
                select SkillsData.AmountCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Sum();
        }

        public long AmountCrit(int skillId)
        {
            return SkillsData.AmountCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double AvgCrit()
        {
            var crits = Crits();
            if (crits == 0) return 0;
            return AmountCrit()/crits;
        }

        public double AvgCrit(int skillId)
        {
            return SkillsData.AverageCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double AvgWhite(int skillId)
        {
            return SkillsData.AverageWhite(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
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
                result += Skills.ElementAt(i);
                if (i < Skills.Count - 1)
                {
                    result += ",";
                }
            }
            return result;
        }
    }
}