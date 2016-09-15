using System.Collections.Generic;
using System.Linq;
using Data;
using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class Skills
    {
        private readonly Dictionary<string, object> _caching = new Dictionary<string, object>();

        public Skills(Dictionary<Entity, Dictionary<Entity, List<Skill>>> sourceTargetSkill,
            Dictionary<Entity, Dictionary<Entity, List<Skill>>> targetSourceSkill,
            Dictionary<Entity, Dictionary<Entity, Dictionary<int, List<Skill>>>> sourceTargetIdSkill,
            Dictionary<Entity, Dictionary<int, List<Skill>>> sourceIdSkill
            )
        {
            SourceTargetSkill = sourceTargetSkill;
            TargetSourceSkill = targetSourceSkill;
            SourceIdSkill = sourceIdSkill;
            SourceTargetIdSkill = sourceTargetIdSkill;
        }


        private Dictionary<Entity, Dictionary<Entity, List<Skill>>> SourceTargetSkill { get; }
        private Dictionary<Entity, Dictionary<Entity, List<Skill>>> TargetSourceSkill { get; }
        private Dictionary<Entity, Dictionary<int, List<Skill>>> SourceIdSkill { get; }
        private Dictionary<Entity, Dictionary<Entity, Dictionary<int, List<Skill>>>> SourceTargetIdSkill { get; }


        public long DamageReceived(Entity target, Entity source, bool timed)
        {
            var sourceString = source?.Id.ToString() ?? "";
            var key = "damage_received/" + target + "/" + sourceString + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];
            IEnumerable<long> result = new List<long>();

            if (TargetSourceSkill.ContainsKey(target))
            {
                if (!timed && source != null)
                 {
                    if (TargetSourceSkill[target].ContainsKey(source))
                    {
                        result = from skills in TargetSourceSkill[target][source]
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

        public int HitsReceived(Entity target, Entity source, bool timed)
        {
            var sourceString = source?.Id.ToString() ?? "";
            var key = "hits_received/" + target + "/" + sourceString + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            IEnumerable<Skill> result= new List<Skill>();
            if (TargetSourceSkill.ContainsKey(target))
            {
                if (!timed && source != null)
                {
                    if (TargetSourceSkill[target].ContainsKey(source))
                    {
                        result = from skills in TargetSourceSkill[target][source]
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

        public long BiggestCrit(Entity source, Entity target, bool timed)
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
                result = from skills in SourceTargetSkill[source][target]
                    where skills.Type == Database.Type.Damage
                    where skills.Critic
                    select skills.Amount;
            }
            var max = result.Concat(new []{(long)0}).Max();
            _caching.Add(key, max);
            return max;
        }

        public IEnumerable<Tera.Game.Skill> SkillsIdBySource(UserEntity source, Entity target, bool timed)
        {
            IEnumerable<Tera.Game.Skill> result;


            if (timed || target == null)
            {
                result = from skills in SourceTargetSkill[source].Values
                    from skill in skills
                    select
                        SkillResult.GetSkill(source, skill.Pet, skill.SkillId, skill.HotDot,
                            NetworkController.Instance.EntityTracker, BasicTeraData.Instance.SkillDatabase,
                            BasicTeraData.Instance.HotDotDatabase, BasicTeraData.Instance.PetSkillDatabase);

                return result.Distinct();
            }


            result = from skills in SourceTargetSkill[source][target]
                select
                    SkillResult.GetSkill(source, skills.Pet, skills.SkillId, skills.HotDot,
                        NetworkController.Instance.EntityTracker, BasicTeraData.Instance.SkillDatabase,
                        BasicTeraData.Instance.HotDotDatabase, BasicTeraData.Instance.PetSkillDatabase);
            return result.Distinct();
        }


        public IEnumerable<KeyValuePair<Entity, Tera.Game.Skill>> SkillsIdByTarget(Entity target)
        {
            if (!TargetSourceSkill.ContainsKey(target))
            {
                return new List<KeyValuePair<Entity, Tera.Game.Skill>>();
            }

            var result = from skills in TargetSourceSkill[target].Values
                from skill in skills
                select
                    new KeyValuePair<Entity, Tera.Game.Skill>( skill.Source(), SkillResult.GetSkill(skill.Source(), skill.Pet, skill.SkillId, skill.HotDot,
                        NetworkController.Instance.EntityTracker, BasicTeraData.Instance.SkillDatabase,
                        BasicTeraData.Instance.HotDotDatabase, BasicTeraData.Instance.PetSkillDatabase));
            return result.Where(x => x.Value != null).Distinct();
        }

        public bool Type(Entity source, Entity target, int skillid, NpcInfo pet, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var name = "";
            if (pet != null)
            {
                name = pet.Name;
            }
            var key = "type/" + source + "/" + targetString + "/" + name + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (bool) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            IEnumerable<Database.Type> result;
            if (pet == null)
            {
                result = from skills in dataSource
                         where
                             skills.Pet == null && skills.Type == type
                         select skills.Type;
            }
            else
            {
                result = from skills in dataSource
                    where
                        skills.Pet != null &&
                        skills.Pet.Name == pet.Name && skills.Type == type
                    select skills.Type;
            }

            var typeExist = result.Count();

            _caching[key] = typeExist != 0;
            return typeExist != 0;
        }


        public long Amount(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "amount/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                       where skills.Type == type
                select skills.Amount;

            var sum = result.Sum();
            _caching[key] = sum;
            return sum;
        }

        private IEnumerable<Skill> DataSource(Entity source, Entity target, int skillid, bool timed)
        {

            return timed || target == null
                ? SourceIdSkill[source][skillid]
                : SourceTargetIdSkill[source][target][skillid];
        }


        public long AmountWhite(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "amount_white/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false && skills.Type == type
                select skills.Amount;

            long sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Sum();
            _caching[key] = sum;
            return sum;
        }

        public long AmountCrit(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "amount_crit/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic && skills.Type == type
                select skills.Amount;
            long sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Sum();
            _caching[key] = sum;
            return sum;
        }

        public double AverageCrit(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "average_crit/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (double) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic && skills.Type == type
                select skills.Amount;

            double sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Average();
            _caching[key] = sum;
            return sum;
        }

        public double AverageWhite(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "average_white/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (double) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false && skills.Type == type
                select skills.Amount;
            double sum = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) sum = enumerable.Average();
            _caching[key] = sum;
            return sum;
        }


        public double Average(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "average/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (double) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Type == type
                select skills.Amount;

            var average = result.Average();
            _caching[key] = average;
            return average;
        }

        public int CritRate(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "critrate/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Type == type
                select skills.Critic;

            var enumerable = result as bool[] ?? result.ToArray();
            var crit = enumerable.Count(x => x)*100/enumerable.Length;
            _caching[key] = crit;
            return crit;
        }

        public long BiggestCrit(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "biggest_crit/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic && skills.Type == type
                select skills.Amount;

            long max = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) max = enumerable.Max();
            _caching[key] = max;
            return max;
        }

        public long BiggestWhite(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "biggest_white/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false && skills.Type == type
                select skills.Amount;

            long max = 0;
            var enumerable = result as long[] ?? result.ToArray();
            if (enumerable.Length != 0) max = enumerable.Max();
            _caching[key] = max;
            return max;
        }

        public long BiggestHit(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "biggest_hit/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (long) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Type == type
                select skills.Amount;

            var max = result.Max();
            _caching[key] = max;
            return max;
        }


        public int Crits(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "crits/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic && skills.Type == type
                select skills.Critic;

            var crit = result.Count();
            _caching[key] = crit;
            return crit;
        }

        public List<Skill> GetSkillsDealt(Entity source, Entity target, bool timed)
        {
            IEnumerable<Skill> result;

            if (timed || target == null)
            {
                result = from skills in SourceTargetSkill[source].Values
                    from skill in skills
                    select skill;
                return result.ToList();
            }

            result = from skills in SourceTargetSkill[source][target]
                select skills;
            return result.ToList();
        }

        public List<Skill> GetSkillsReceived(Entity target, bool timed)
        {
            if (!TargetSourceSkill.ContainsKey(target))
            {
                return new List<Skill>();
            }

            var result = from skills in TargetSourceSkill[target].Values
                from skill in skills
                select skill;
            return result.ToList();
        }

        public int White(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "white/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic == false && skills.Type == type
                select skills.Critic;

            var white = result.Count();
            _caching[key] = white;
            return white;
        }


        public long LowestCrit(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                where skills.Critic && skills.Type == type
                select skills.Amount;
            var enumerable = result as long[] ?? result.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Min();
        }

        public int Hits(Entity source, Entity target, int skillid, bool timed, Database.Type type)
        {
            var targetString = target?.Id.ToString() ?? "";
            var key = "hits/" + source + "/" + targetString + "/" + skillid + "/" + type + "/" + timed;
            if (_caching.ContainsKey(key)) return (int) _caching[key];

            var dataSource = DataSource(source, target, skillid, timed);
            var result = from skills in dataSource
                         where skills.Type == type
                select skills;

            var hits = result.Count();
            _caching[key] = hits;
            return hits;
        }
    }
}