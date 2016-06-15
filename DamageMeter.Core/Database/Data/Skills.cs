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
    }
}
