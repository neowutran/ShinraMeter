using System;
using Data;
using Tera.Game;

namespace DamageMeter
{
    public class SkillResult
    {
        public SkillResult(bool abnormality, int amount, bool isCritical, bool isHp, int skillId, EntityId source,
            EntityId target, EntityTracker entityRegistry,
            PlayerTracker playerTracker)
        {
            Amount = amount;
            IsCritical = isCritical;
            IsHp = isHp;
            SkillId = skillId;
            Abnormality = abnormality;
            Source = entityRegistry.GetOrPlaceholder(source);
            Target = entityRegistry.GetOrPlaceholder(target);
            var userNpc = UserEntity.ForEntity(Source);
            var npc = (NpcEntity) userNpc["npc"];
            var sourceUser = userNpc["user"] as UserEntity; // Attribute damage dealt by owned entities to the owner
            var targetUser = Target as UserEntity; // But don't attribute damage received by owned entities to the owner

            if (abnormality)
            {
                var hotdot = BasicTeraData.Instance.HotDotDatabase.Get(skillId);
                if (hotdot == null)
                {
                    return;
                }
                Skill = new UserSkill(skillId, PlayerClass.Common,
                    hotdot.Name, "DOT", null);
            }

            if (sourceUser != null)
            {
                SourcePlayer = playerTracker.Get(sourceUser.PlayerId);
                if (!abnormality)
                {
                    Skill = BasicTeraData.Instance.SkillDatabase.Get(sourceUser.RaceGenderClass.Class, skillId);
                    if (Skill == null && npc != null)
                    {
                        var petName = BasicTeraData.Instance.MonsterDatabase.GetMonsterName(npc.NpcArea.ToString(),
                            npc.NpcId.ToString());
                        Skill = new UserSkill(skillId, sourceUser.RaceGenderClass.Class, petName,
                            BasicTeraData.Instance.PetSkillDatabase.Get(petName, skillId), null);
                    }
                }
            }

            if (targetUser != null)
            {
                TargetPlayer = playerTracker.Get(targetUser.PlayerId);
            }
        }

        public bool Abnormality { get; }
        public int Amount { get; }
        public Tera.Game.Entity Source { get; }
        public Tera.Game.Entity Target { get; }
        public bool IsCritical { get; private set; }
        public bool IsHp { get; }

        public int SkillId { get; private set; }
        public UserSkill Skill { get; }

        public int Damage
        {
            get
            {
                if (IsHp && Amount < 0)
                {
                    return Math.Abs(Amount);
                }
                return 0;
            }
        }

        public int Heal => IsHp && Amount > 0 ? Amount : 0;

        public int Mana => !IsHp ? Amount : 0;


        public Player SourcePlayer { get; }
        public Player TargetPlayer { get; private set; }
    }
}