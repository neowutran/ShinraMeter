using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Database.Data
{
    public class Skills
    {

        public Skills(Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> sourceTargetSkill, Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> targetSourceSkill)
        {
            SourceTargetSkill = sourceTargetSkill;
            TargetSourceSkill = targetSourceSkill;
        }


        public Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> SourceTargetSkill { get; }
        public Dictionary<EntityId, Dictionary<EntityId, List<Skill>>> TargetSourceSkill { get; }

        public long DamageReceived(EntityId target)
        {
           return TargetSourceSkill[target].Sum(x => x.Value.Sum(y => y.Amount));
        }

        public long DamageReceived(EntityId target, EntityId source)
        {
            return TargetSourceSkill[target][source].Sum(x => x.Amount);

        }

        public int HitsReceived(EntityId target)
        {
            return TargetSourceSkill[target].Sum(x => x.Value.Count());
        }

        public int HitsReceived(EntityId target, EntityId source)
        {
            return TargetSourceSkill[target][source].Count();
        }

        public long BiggestCrit(EntityId source)
        {
            return SourceTargetSkill[source].OrderByDescending(x => x.Value.All(y => y.Type == Database.Type.Damage)).Max(x => x.Value.Max(y => y.Amount));
        }


        public long BiggestCrit(EntityId source, EntityId target)
        {
            return SourceTargetSkill[source][target].OrderByDescending(x => x.Type == Database.Type.Damage).Max(x => x.Amount);
        }
    }
}
