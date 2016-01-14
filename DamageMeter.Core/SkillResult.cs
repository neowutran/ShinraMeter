using Data;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter
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
            var userNpc = UserEntity.ForEntity(Source);
            var npc = (NpcEntity) userNpc["npc"];
            var sourceUser = userNpc["user"] as UserEntity; // Attribute damage dealt by owned entities to the owner
            var targetUser = Target as UserEntity; // But don't attribute damage received by owned entities to the owner

            if (sourceUser != null)
            {
                SourcePlayer = playerTracker.Get(sourceUser.PlayerId);
                Skill = BasicTeraData.Instance.SkillDatabase.Get(sourceUser.RaceGenderClass.Class, message.SkillId);
                if (Skill == null && npc != null)
                {

                    var petName = BasicTeraData.Instance.MonsterDatabase.GetMonsterName(npc.NpcArea.ToString(), npc.NpcId.ToString());
                    Skill = new UserSkill(message.SkillId, sourceUser.RaceGenderClass.Class,petName, BasicTeraData.Instance.PetSkillDatabase.Get(petName,message.SkillId), null);

                }
            }

            if (targetUser != null)
            {
                TargetPlayer = playerTracker.Get(targetUser.PlayerId);
            }
        }

        public int Amount { get; }
        public Tera.Game.Entity Source { get; }
        public Tera.Game.Entity Target { get; }
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