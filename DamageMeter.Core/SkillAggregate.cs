using DamageMeter.Database.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter
{
    public class SkillAggregate
    {

        public List<Tera.Game.Skill> Skills { get; }
        public string Name { get; }

        private Skills _skillsData {get;}
        private bool _timed;
        private PlayerDealt _playerDealt;
        private EntityInformation _entityInformation;

        public SkillAggregate(Tera.Game.Skill skill, Skills skillsData, PlayerDealt playerDealt, EntityInformation entityInformation, bool timed, Database.Database.Type type)
        {
            _playerDealt = playerDealt;
            _timed = timed;
            Type = type;
            _entityInformation = entityInformation;
            Skills = new List<Tera.Game.Skill>();
            Name = skill.ShortName;
            _skillsData = skillsData;
            Skills.Add(skill);
        }

        public Database.Database.Type Type { get; }

        public bool Add(Tera.Game.Skill skill)
        {
            if (skill.ShortName == Name)
            {
                foreach(var sk in Skills)
                {
                    if(skill.Id == sk.Id && skill.IsHotDot == sk.IsHotDot)
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
                         select _skillsData.Amount(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            return result.Sum();
        }

        public long Amount(int skillId)
        {
            return _skillsData.Amount(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double BiggestCrit()
        {
            var result = from skill in Skills
                         select _skillsData.BiggestCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            if (result.Count() == 0) return 0;
            return result.Max();
        }

        public double BiggestCrit(int skillId)
        {
            return _skillsData.BiggestCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double DamagePercent()
        {
            return Amount() / _playerDealt.Amount;
        }

        public long DamagePercent(int skillId)
        {
            return _skillsData.Amount(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed) / Amount();
        }

        public double Hits()
        {
            var result = from skill in Skills
                         select _skillsData.Hits(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            return result.Sum();
        }

        public double Hits(int skillId)
        {
            return _skillsData.Hits(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double White()
        {
            var result = from skill in Skills
                         select _skillsData.White(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            if (result.Count() == 0) return 0;
            return result.Sum();
        }

        public double White(int skillId)
        {
            return _skillsData.White(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double Crits()
        {
            var result = from skill in Skills
                         select _skillsData.Crits(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            if (result.Count() == 0) return 0;
            return result.Sum();
        }

        public double Crits(int skillId)
        {
            return _skillsData.Crits(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double CritRate()
        {
            return Crits() / Hits();
        }

        public double CritRate(int skillId)
        {
            return _skillsData.CritRate(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double BiggestHit()
        {
            var result = from skill in Skills
                         select _skillsData.BiggestHit(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            if (result.Count() == 0) return 0;
            return result.Max();
        }

        public double BiggestHit(int skillId)
        {
            return _skillsData.BiggestHit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double Avg()
        {
            return Amount() / Hits();
        }

        public double Avg(int skillId)
        {
            return _skillsData.Average(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long AmountWhite()
        {
            var result = from skill in Skills
                         select _skillsData.AmountWhite(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            if (result.Count() == 0) return 0;
            return result.Sum();
        }

        public long AmountWhite(int skillId)
        {
            return _skillsData.AmountWhite(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public long AmountCrit()
        {
            var result = from skill in Skills
                         select _skillsData.AmountCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skill.Id, _timed);
            if (result.Count() == 0) return 0;
            return result.Sum();
        }

        public long AmountCrit(int skillId)
        {
            return _skillsData.AmountCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double AvgCrit()
        {
            var crits = Crits();
            if (crits == 0) return 0;
            return AmountCrit() / crits;
        }

        public double AvgCrit(int skillId)
        {
            return _skillsData.AverageCrit(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double AvgWhite(int skillId)
        {
            return _skillsData.AverageWhite(_playerDealt.Source.User.Id, _entityInformation.Entity, skillId, _timed);
        }

        public double AvgWhite()
        {
            var white = White();
            if (white == 0) return 0;
            return AmountWhite() / white;
        }
        

        public string Id()
        {
            var result = "";
            for(int i = 0; i < Skills.Count; i++)
            {
                result += Skills.ElementAt(i);
                if(i < Skills.Count -1)
                {
                    result += ",";
                }
            }
            return result;
        }

    }
}
