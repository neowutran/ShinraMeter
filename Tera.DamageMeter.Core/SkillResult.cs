using Tera.Data;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.DamageMeter
{
    public class SkillResult
    {
        public SkillResult(EachSkillResultServerMessage message, EntityTracker entityRegistry,
            PlayerTracker playerTracker)
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
                SourcePlayer = playerTracker.Get(sourceUser.PlayerId);
                Skill = BasicTeraData.Instance.SkillDatabase.Get(sourceUser.RaceGenderClass.Class, message.SkillId);

                /*
               
                if ((SourcePlayer.Name == "Yukikoo" || SourcePlayer.Name == "Yukikoolol" ||
                     SourcePlayer.Name == "Gorkie"))
                {
                    Console.Write("Source id:" + message.Source);
                    if (Skill != null)
                    {
                        Console.Write("skill name" + message.SkillId);
                    }
                    else
                    {
                        Console.Write("Skill id" + message.SkillId);
                    }

                    Console.Write(";Target:" + message.Target);


                    Console.WriteLine("Flags:" + message.Flags + ";flags:" + Convert.ToString(message.FlagsDebug, 2) +
                                      ";isCrit:" + message.IsCritical + ";Amount:" + message.Amount + ";HitId:" +
                                      message.HitId);
                    foreach (var byt in message.Unknow1)
                    {
                        Console.Write(Convert.ToString(byt, 16));
                    }
                    Console.WriteLine("-");
                    foreach (var byt in message.Unknow2)
                    {
                        Console.Write(Convert.ToString(byt, 16));
                    }
                    Console.WriteLine("#");
                }
                */
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


        public Player SourcePlayer { get; }
        public Player TargetPlayer { get; private set; }
    }
}