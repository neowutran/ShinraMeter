using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class Skills
    {

        public Skills(Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> sourceTargetSkill, Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> targetSourceSkill,
            Dictionary<EntityId, Dictionary<EntityId, Dictionary<int, List<Skill>>>> sourceTargetIdSkill, Dictionary<EntityId, Dictionary<int, List<Skill>>> sourceIdSkill
            )
        {
            SourceTargetSkill = sourceTargetSkill;
            TargetSourceSkill = targetSourceSkill;
            SourceIdSkill = sourceIdSkill;
            SourceTargetIdSkill = sourceTargetIdSkill;
        }


        private Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> SourceTargetSkill { get; }
        private Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> TargetSourceSkill { get; }
        private Dictionary<EntityId, Dictionary<int, List<Skill>>> SourceIdSkill { get; }
        private Dictionary<EntityId, Dictionary<EntityId, Dictionary<int, List<Skill>>>> SourceTargetIdSkill { get; }



        public long DamageReceived(EntityId target, EntityId source, bool timed)
        {
            IEnumerable<long> result = null;
            if (!timed)
            {
                result = from skills in TargetSourceSkill[target][source]
                         where skills.Type == Database.Type.Damage
                         select skills.Amount;
            }

            result = from skills in TargetSourceSkill[target].Values
                     from skill in skills
                     where skill.Type == Database.Type.Damage
                     select skill.Amount;
            return result.Sum();
        }

        public int HitsReceived(EntityId target, EntityId source, bool timed)
        {
            IEnumerable<Skill> result = null;

            if (!timed)
            {
                result = from skills in TargetSourceSkill[target][source]
                         where skills.Type == Database.Type.Damage
                         select skills;
                return result.Count();
            }
            result = from skills in TargetSourceSkill[target].Values
                     from skill in skills
                     where skill.Type == Database.Type.Damage
                     select skill;
            return result.Count();

        }

        public long BiggestCrit(EntityId source, Entity target, bool timed)
        {
            IEnumerable<long> result = null;

            if(timed || target == null)
            {

                result = from skills in SourceTargetSkill[source].Values
                         from skill in skills
                         where skill.Type == Database.Type.Damage
                         where skill.Critic == true
                         select skill.Amount;
                return result.Max();
            }

        
                result = from skills in SourceTargetSkill[source][target.Id]
                         where skills.Type == Database.Type.Damage
                         where skills.Critic == true
                         select skills.Amount;
                return result.Max();


        }

        public IEnumerable<Tera.Game.Skill> SkillsId(UserEntity source, Entity target, bool timed)
        {

            IEnumerable<Tera.Game.Skill> result = null;

            if (timed || target == null)
            {
                result = from skills in SourceTargetSkill[source.Id].Values
                         from skill in skills
                         select Data.BasicTeraData.Instance.SkillDatabase.GetOrNull(source, skill.SkillId);

                return result.Distinct();
            }

           
                result = from skills in SourceTargetSkill[source.Id][target.Id]
                         select Data.BasicTeraData.Instance.SkillDatabase.GetOrNull(source, skills.SkillId);
                return result.Distinct();
            

      

        }

        public Database.Type Type(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Type;
            return result.First();
        }


        public long Amount(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Amount;
            return result.Sum();
        }

        private List<Skill> DataSource(EntityId source, Entity target, int skillid ,  bool timed)
        {
            return (timed || target == null) ? SourceIdSkill[source][skillid] : SourceTargetIdSkill[source][target.Id][skillid];
        }


        public long AmountWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Amount;
            return result.Sum();
        }

        public long AmountCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;
            return result.Sum();
        }

        public double AverageCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;
            return result.Average();
        }

        public double AverageWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Amount;
            return result.Average();
        }


        public double Average(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Amount;
            return result.Average();
        }

        public double CritRate(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Critic;
            return result.Count(x => x == true) / result.Count();
        }

        public double BiggestCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;
            return result.Max();

        }

        public double BiggestWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Amount;
            return result.Max();
        }

        public double BiggestHit(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Amount;
            return result.Max();
        }


        public double Crits(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Critic;
            return result.Count();

        }

        public List<Skill> GetSkills(EntityId source, Entity target, bool timed, long beginTime, long endTime)
        {
            IEnumerable<Skill> result = null;

            if(timed || target == null)
            {
                result = from skills in SourceTargetSkill[source].Values
                         from skill in skills
                         select skill;
                return result.ToList();
            }

         
                result = from skills in SourceTargetSkill[source][target.Id]
                         select skills;
                return result.ToList();
            

         

        }


        public double White(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Critic;
            return result.Count();

        }


        public double LowestCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;
            return result.Min();

        }

        public double Hits(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills;
            return result.Count();

        }

    }
}
