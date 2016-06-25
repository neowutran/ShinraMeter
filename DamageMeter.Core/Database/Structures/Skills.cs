using System.Collections.Generic;
using System.Linq;
using Data;
using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class Skills
    {
        private readonly Dictionary<string, object> _caching = new Dictionary<string, object>();

        public Skills(Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> sourceTargetSkill,
            Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> targetSourceSkill,
            Dictionary<EntityId, Dictionary<EntityId, Dictionary<int, List<Skill>>>> sourceTargetIdSkill,
            Dictionary<EntityId, Dictionary<int, List<Skill>>> sourceIdSkill
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


        public long DamageReceived(EntityId target, Entity source, bool timed)
        {
            var sourceString = source?.Id.ToString() ?? "";
            var key = "damage_received/" + target + "/" + sourceString + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];
            IEnumerable<long> result = new List<long>();

            if (TargetSourceSkill.ContainsKey(target))
            {
                if (!timed && source != null)
                 {
                    if (TargetSourceSkill[target].ContainsKey(source.Id))
                    {
                        result = from skills in TargetSourceSkill[target][source.Id]
                            where skills.Type == Database.Type.Damage
                            select skills.Amount;
                    }
                }
                else
                {
                    result = from skills in TargetSourceSkill[target].Values
                        from skill in skills
                        where skill.Type == Database.Type.Damage
                        select skill.Amount;
                }

            }

            var sum = result.Sum();
            _caching.Add(key, sum);
            return sum;
        }

        public int HitsReceived(EntityId target, Entity source, bool timed)
        {
            var sourceString = source?.Id.ToString() ?? "";
            var key = "hits_received/" + target + "/" + sourceString + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            IEnumerable<Skill> result= new List<Skill>();
            if (TargetSourceSkill.ContainsKey(target))
            {
                if (!timed && source != null)
                {
                    if (TargetSourceSkill[target].ContainsKey(source.Id))
                    {
                        result = from skills in TargetSourceSkill[target][source.Id]
                            where skills.Type == Database.Type.Damage
                            select skills;
                    }
                }
                else
                {
                    result = from skills in TargetSourceSkill[target].Values
                        from skill in skills
                        where skill.Type == Database.Type.Damage
                        select skill;
                }
            }

            var count = result.Count();
            _caching.Add(key, count);
            return count;
        }

        public long BiggestCrit(EntityId source, Entity target, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "biggest_crit/" + source + "/" + targetString + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            IEnumerable<long> result;

            if (timed || target == null)
            {
                result = from skills in SourceTargetSkill[source].Values
                    from skill in skills
                    where skill.Type == Database.Type.Damage
                    where skill.Critic
                    select skill.Amount;
            }
            else
            {
                result = from skills in SourceTargetSkill[source][target.Id]
                    where skills.Type == Database.Type.Damage
                    where skills.Critic
                    select skills.Amount;
            }
            var max = result.Count();
            _caching.Add(key, max);
            return max;
        }

        public IEnumerable<Tera.Game.Skill> SkillsId(UserEntity source, Entity target, bool timed)
        {
            IEnumerable<Tera.Game.Skill> result;


            if (timed || target == null)
            {
                result = from skills in SourceTargetSkill[source.Id].Values
                    from skill in skills
                    select
                        SkillResult.GetSkill(source.Id, skill.PetSource, skill.SkillId, skill.HotDot,
                            NetworkController.Instance.EntityTracker, BasicTeraData.Instance.SkillDatabase,
                            BasicTeraData.Instance.HotDotDatabase, BasicTeraData.Instance.PetSkillDatabase);

                return result.Distinct();
            }


            result = from skills in SourceTargetSkill[source.Id][target.Id]
                select
                    SkillResult.GetSkill(source.Id, skills.PetSource, skills.SkillId, skills.HotDot,
                        NetworkController.Instance.EntityTracker, BasicTeraData.Instance.SkillDatabase,
                        BasicTeraData.Instance.HotDotDatabase, BasicTeraData.Instance.PetSkillDatabase);
            return result.Distinct();
        }

        public Database.Type Type(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "type/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (Database.Type) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                select skills.Type;

            var type = result.First();
            _caching[key] = type;
            return type;
        }


        public long Amount(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "amount/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                select skills.Amount;

            var sum = result.Sum();
            _caching[key] = sum;
            return sum;
        }

        private IEnumerable<Skill> DataSource(EntityId source, Entity target, int skillid, bool timed)
        {
            return timed || target == null
                ? SourceIdSkill[source][skillid]
                : SourceTargetIdSkill[source][target.Id][skillid];
        }


        public long AmountWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "amount_white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false
                select skills.Amount;

            long sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Sum();
            _caching[key] = sum;
            return sum;
        }

        public long AmountCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "amount_crit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic
                select skills.Amount;
            long sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Sum();
            _caching[key] = sum;
            return sum;
        }

        public double AverageCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "average_crit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (double) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic
                select skills.Amount;

            double sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Average();
            _caching[key] = sum;
            return sum;
        }

        public double AverageWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "average_white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (double) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false
                select skills.Amount;
            double sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Average();
            _caching[key] = sum;
            return sum;
        }


        public double Average(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "average/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (double) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                select skills.Amount;

            var average = result.Average();
            _caching[key] = average;
            return average;
        }

        public int CritRate(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "critrate/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                select skills.Critic;

            var enumerable = result as bool[] ?? result.ToArray();
            var crit = enumerable.Count(x => x)*100/enumerable.Length;
            _caching[key] = crit;
            return crit;
        }

        public long BiggestCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "biggest_crit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic
                select skills.Amount;

            long max = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) max = enumerable.Max();
            _caching[key] = max;
            return max;
        }

        public long BiggestWhite(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "biggest_white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false
                select skills.Amount;

            long max = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) max = enumerable.Max();
            _caching[key] = max;
            return max;
        }

        public long BiggestHit(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "biggest_hit/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                select skills.Amount;

            var max = result.Max();
            _caching[key] = max;
            return max;
        }


        public int Crits(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "crits/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic
                select skills.Critic;

            var crit = result.Count();
            _caching[key] = crit;
            return crit;
        }

        public List<Skill> GetSkills(EntityId source, Entity target, bool timed, long beginTime, long endTime)
        {
            IEnumerable<Skill> result;

            if (timed || target == null)
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
            var targetString = target?.Id.ToString() ?? "";
            var key = "white/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false
                select skills.Critic;

            var white = result.Count();
            _caching[key] = white;
            return white;
        }


        public double LowestCrit(EntityId source, Entity target, int skillid, bool timed)
        {
            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic
                select skills.Amount;
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Min();
        }

        public int Hits(EntityId source, Entity target, int skillid, bool timed)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "hits/" + source + "/" + targetString + "/" + skillid + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                select skills;

            var hits = result.Count();
            _caching[key] = hits;
            return hits;
        }
    }
}