using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.DamageMeter
{
    public class SkillResult
    {
        public int Amount { get; private set; }
        public Entity Source { get; private set; }
        public Entity Target { get; private set; }
        public bool IsCritical { get; private set; }
        public bool IsHeal { get; private set; }

        public int SkillId { get; private set; }
        public Skill Skill { get; private set; }
        public int Damage { get { return IsHeal ? 0 : Amount; } }
        public int Heal { get { return IsHeal ? Amount : 0; } }

        // Attribute damage dealt by owned entities to the owner
        public User SourceUser { get { return User.ForEntity(Source); } }
        // But don't attribute damage received by owned entities to the owner
        public User TargetUser { get { return Target as User; } }

        public SkillResult(EachSkillResultServerMessage message, EntityRegistry entityRegistry, SkillDatabase skillDatabase)
        {
            Amount = message.Amount;
            Source = entityRegistry.GetOrPlaceholder(message.Source);
            Target = entityRegistry.GetOrPlaceholder(message.Target);
            IsCritical = message.IsCritical;
            IsHeal = message.IsHeal;
            SkillId = message.SkillId;

            if (SourceUser != null)
                Skill = skillDatabase.Get(SourceUser, message.SkillId);
        }
    }
}
