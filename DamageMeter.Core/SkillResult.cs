using System;
using Data;
using Tera.Game;
using Tera.Game.Messages;
using System.Diagnostics;

namespace DamageMeter
{
    public class SkillResult
    {
        public SkillResult(EachSkillResultServerMessage message, EntityTracker entityRegistry, PlayerTracker playerTracker, SkillDatabase skillDatabase)
        {
            Time = message.Time;
            Amount = message.Amount;
            IsCritical = message.IsCritical;
            IsHp = message.IsHp;
            IsHeal = message.IsHeal;
            SkillId = message.SkillId;
            Abnormality = false;

            Source = entityRegistry.GetOrPlaceholder(message.Source);
            Target = entityRegistry.GetOrPlaceholder(message.Target);
            var userNpc = UserEntity.ForEntity(Source);
            var npc = (NpcEntity)userNpc["npc"];
            var sourceUser = userNpc["user"] as UserEntity; // Attribute damage dealt by owned entities to the owner
            var targetUser = Target as UserEntity; // But don't attribute damage received by owned entities to the owner

            if (sourceUser != null)
            {
                Skill = skillDatabase.Get(sourceUser, message);
                if (Skill == null && npc != null)
                {
                    Skill = new UserSkill(message.SkillId, sourceUser.RaceGenderClass, npc.Info.Name, null,"",
                            skillDatabase.GetSkillByPetName(npc.Info.Name, sourceUser.RaceGenderClass)?.IconName ?? "");
                }
                SourcePlayer = playerTracker.Get(sourceUser.PlayerId);
                if (Skill == null)
                    Skill = new UserSkill(message.SkillId, sourceUser.RaceGenderClass, "Unknown");
            }
            if (targetUser != null)
            {
                TargetPlayer = playerTracker.Get(targetUser.PlayerId);
            }
        }
        public SkillResult(int amount, bool isCritical, bool isHp, bool isHeal, HotDot hotdot, EntityId source, EntityId target, DateTime time,
            EntityTracker entityRegistry, PlayerTracker playerTracker)
        {
            Time = time;
            Amount = amount;
            IsCritical = isCritical;
            IsHp = isHp;
            IsHeal = isHeal;
            SkillId = hotdot.Id;
            Abnormality = true;

            Source = entityRegistry.GetOrPlaceholder(source);
            Target = entityRegistry.GetOrPlaceholder(target);
            var userNpc = UserEntity.ForEntity(Source);
            var sourceUser = userNpc["user"] as UserEntity; // Attribute damage dealt by owned entities to the owner
            var targetUser = Target as UserEntity; // But don't attribute damage received by owned entities to the owner

            PlayerClass pclass = PlayerClass.Common;
            if (sourceUser != null)
            {
                SourcePlayer = playerTracker.Get(sourceUser.PlayerId);
                pclass = SourcePlayer.RaceGenderClass.Class;
            }
            Skill = new UserSkill(hotdot.Id, pclass,
                hotdot.Name, "DOT", null, hotdot.IconName);

            if (targetUser != null)
            {
                playerTracker.Get(targetUser.PlayerId);
            }
        }

        public DateTime Time { get; private set; }
        public bool Abnormality { get; }
        public int Amount { get; }
        public Tera.Game.Entity Source { get; }
        public Tera.Game.Entity Target { get; }
        public bool IsCritical { get; private set; }
        public bool IsHp { get; }
        public bool IsHeal { get; private set; }

        public int SkillId { get; private set; }
        public Skill Skill { get; }
        public string SkillName => Skill?.Name ?? SkillId.ToString();
        public string SkillNameDetailed
            => $"{Skill?.Name ?? SkillId.ToString()} {(IsChained != null ? (bool)IsChained ? "[C]" : null : null)} {(string.IsNullOrEmpty(Skill?.Detail) ? null : $"({Skill.Detail})")}".Replace("  ", " ");
        public bool? IsChained => Skill.IsChained;
        public int Damage { get { return IsHeal||!IsHp ? 0 : Amount; } }
        public int Heal => IsHp && IsHeal ? Amount : 0;
        public int Mana => !IsHp ? Amount : 0;


        public Player SourcePlayer { get; }
        public Player TargetPlayer { get; private set; }
        public override string ToString()
        {
            return $"{SkillName}({SkillId}) [{Amount}]";
        }
    }
}