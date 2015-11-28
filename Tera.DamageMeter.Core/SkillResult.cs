using System;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.DamageMeter
{
    public class SkillResult
    {
        public SkillResult(EachSkillResultServerMessage message, EntityTracker entityRegistry,
            PlayerTracker playerTracker, SkillDatabase skillDatabase)
        {
            Amount = message.Amount;
            IsCritical = message.IsCritical;
            IsHeal = message.IsHeal;
            IsMana = message.IsMana;
            SkillId = message.SkillId;

            Source = entityRegistry.GetOrPlaceholder(message.Source);
            Target = entityRegistry.GetOrPlaceholder(message.Target);
            var sourceUser = UserEntity.ForEntity(Source); // Attribute damage dealt by owned entities to the owner
            var targetUser = Target as UserEntity; // But don't attribute damage received by owned entities to the owner

            if (sourceUser != null)
            {
                Skill = skillDatabase.Get(sourceUser, message.SkillId);

                if (Skill == null)
                {
                    var skillid = message.SkillId/10;
                    skillid = skillid*10;
                    for (var i = 0; i < 10; i++)
                    {
                        Skill = skillDatabase.Get(sourceUser, skillid + i);
                        if (Skill != null)
                        {
                            break;
                        }
                    }
                }

                SourcePlayer = playerTracker.Get(sourceUser.PlayerId);
                if ((SourcePlayer.Name == "Yukikoo" || SourcePlayer.Name == "Javelot"))
                {
                    if (Skill != null)
                    {
                        Console.Write("skill name" + Skill.Name);
                    }
                    else
                    {
                        Console.Write("Skill id"+message.SkillId);
                    }
                    Console.WriteLine("Flags:"+message.Flags+";flags:"+Convert.ToString(message.FlagsDebug,2)+";isCrit:"+message.IsCritical+";Amount:"+message.Amount);
                }
            }

            if (targetUser != null)
            {
                TargetPlayer = playerTracker.Get(targetUser.PlayerId);
            }
        }

        public int Amount { get; }
        public Game.Entity Source { get; }
        public Game.Entity Target { get; }
        public bool IsCritical { get; private set; }
        public bool IsHeal { get; }

        public bool IsMana { get; }

        public int SkillId { get; private set; }
        public UserSkill Skill { get; }

        public int Damage
        {
            get
            {
                if (!IsMana && !IsHeal)
                {
                    return Amount;
                }
                return 0;
            }   
        }

        public int Heal => IsHeal ? Amount : 0;

        public int Mana => IsMana ? Amount : 0;


        public Player SourcePlayer { get; private set; }
        public Player TargetPlayer { get; private set; }
    }
}