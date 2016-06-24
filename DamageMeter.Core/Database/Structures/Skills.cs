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

        private Dictionary<string, object> Caching = new Dictionary<string, object>();



        public long DamageReceived(EntityId target, EntityId source, bool timed)
        {
            var key = "damage_received/" + target + "/" + source + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

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

            var sum = result.Sum();
            Caching.Add(key,sum);
            return sum;
        }

        public int HitsReceived(EntityId target, EntityId source, bool timed)
        {
            var key = "hits_received/" + target + "/" + source + "/" + timed;
            if (Caching.ContainsKey(key)) return (int)Caching[key];

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

            var count = result.Count();
            Caching.Add(key, count);
            return count;

        }

        public long BiggestCrit(EntityId source, Entity target, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "biggest_crit/" + source + "/" + targetString + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

            IEnumerable<long> result = null;

            if (timed || target == null)
            {
                result = from skills in SourceTargetSkill[source].Values
                         from skill in skills
                         where skill.Type == Database.Type.Damage
                         where skill.Critic == true
                         select skill.Amount;
            }
            else
            {
                result = from skills in SourceTargetSkill[source][target.Id]
                         where skills.Type == Database.Type.Damage
                         where skills.Critic == true
                         select skills.Amount;
            }
            var max = result.Count();
            Caching.Add(key, max);
            return max;


        }

        public IEnumerable<Tera.Game.Skill> SkillsId(UserEntity source, Entity target, bool timed)
        {

            IEnumerable<Tera.Game.Skill> result = null;
            

            if (timed || target == null)
            {
                result = from skills in SourceTargetSkill[source.Id].Values
                         from skill in skills
                         select SkillResult.GetSkill(source.Id, skill.SkillId, skill.HotDot, NetworkController.Instance.EntityTracker,  Data.BasicTeraData.Instance.SkillDatabase, Data.BasicTeraData.Instance.HotDotDatabase, Data.BasicTeraData.Instance.PetSkillDatabase);

                return result.Distinct();
            }

           
                result = from skills in SourceTargetSkill[source.Id][target.Id]
                         select SkillResult.GetSkill(source.Id, skills.SkillId, skills.HotDot , NetworkController.Instance.EntityTracker, Data.BasicTeraData.Instance.SkillDatabase, Data.BasicTeraData.Instance.HotDotDatabase, Data.BasicTeraData.Instance.PetSkillDatabase);
            return result.Distinct();

        }

        public Database.Type Type(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "type/" + source + "/" + targetString + "/"+ skillid +"/" + timed;
            if (Caching.ContainsKey(key)) return (Database.Type)Caching[key];
            
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Type;

            var type = result.First();
            Caching[key] = type;
            return type;
        }


        public long Amount(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "amount/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Amount;

            var sum = result.Sum();
            Caching[key] = sum;
            return sum;
        }

        private List<Skill> DataSource(EntityId source, Entity target, int skillid ,  bool timed)
        {
            return (timed || target == null) ? SourceIdSkill[source][skillid] : SourceTargetIdSkill[source][target.Id][skillid];
        }


        public long AmountWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "amount_white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Amount;

            long sum = 0;
            if (result.Count() != 0) sum = result.Sum();
            Caching[key] = sum;
            return sum;
        }

        public long AmountCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "amount_crit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;
            long sum = 0;
            if (result.Count() != 0) sum = result.Sum();
            Caching[key] = sum;
            return sum;
        }

        public double AverageCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "average_crit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (double)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;

            double sum = 0;
            if (result.Count() != 0) sum = result.Average();
            Caching[key] = sum;
            return sum;
        }

        public double AverageWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "average_white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (double)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Amount;
            double sum = 0;
            if (result.Count() != 0) sum = result.Average();
            Caching[key] = sum;
            return sum;
        }


        public double Average(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "average/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (double)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Amount;

            var average = result.Average();
            Caching[key] = average;
            return average;
        }

        public int CritRate(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "critrate/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (int)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Critic;

            var crit = result.Count(x => x == true) / result.Count();
            Caching[key] = crit;
            return crit;
        }

        public long BiggestCrit(EntityId source, Entity target, int skillid, bool timed)
        {

            var targetString = target == null ? "" : target.Id.ToString();
            var key = "biggest_crit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;

            long max = 0;
            if (result.Count() != 0) max = result.Max();
            Caching[key] = max;
            return max;
        }

        public long BiggestWhite(EntityId source, Entity target, int skillid, bool timed)
        {

            var targetString = target == null ? "" : target.Id.ToString();
            var key = "biggest_white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Amount;

            long max = 0;
            if (result.Count() != 0) max = result.Max();
            Caching[key] = max;
            return max;
        }

        public long BiggestHit(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "biggest_hit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (long)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills.Amount;

            var max = result.Max();
            Caching[key] = max;
            return max;
        }


        public int Crits(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target == null ? "" : target.Id.ToString();
            var key = "crits/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (int)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Critic;

            var crit = result.Count();
            Caching[key] = crit;
            return crit;
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


        public int White(EntityId source, Entity target, int skillid, bool timed)
        {

            var targetString = target == null ? "" : target.Id.ToString();
            var key = "white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (int)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == false
                         select skills.Critic;

            var white = result.Count();
            Caching[key] = white;
            return white;
        }


        public double LowestCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Critic == true
                         select skills.Amount;
            if (result.Count() == 0) return 0;
            return result.Min();

        }

        public int Hits(EntityId source, Entity target, int skillid, bool timed)
        {

            var targetString = target == null ? "" : target.Id.ToString();
            var key = "hits/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (Caching.ContainsKey(key)) return (int)Caching[key];

            List<Skill> dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         select skills;

            var hits = result.Count();
            Caching[key] = hits;
            return hits;
        }

    }
}
