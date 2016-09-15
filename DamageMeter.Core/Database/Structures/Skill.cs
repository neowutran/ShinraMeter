using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class Skill
    {
        public Skill(long amount, Database.Type type, Entity target, Player targetPlayer, Entity source, Player sourcePlayer, int skillId, bool hotdot,
            bool critic, long time, NpcInfo pet, HitDirection direction)
        {
            Amount = amount;
            Type = type;
            EntityTarget = target;
            EntitySource = source;
            PlayerTarget = targetPlayer;
            PlayerSource = sourcePlayer;
            SkillId = skillId;
            Critic = critic;
            HotDot = hotdot;
            Time = time;
            Pet = pet;
            Direction = direction;
        }

        public bool SourceIsPlayer => PlayerSource != null;
        public bool TargetIsPlayer => PlayerTarget != null;

        public HitDirection Direction { get; }

        public NpcInfo Pet { get; }

        public bool HotDot { get; }
        public long Amount { get; }
        public Database.Type Type { get; }
        private Entity EntityTarget { get; }
        public Entity Source()
        {
            if (SourceIsPlayer)
            {
                return PlayerSource.User;
            }
            return EntitySource;
        }

        public Entity Target()
        {
            if (TargetIsPlayer)
            {
                return PlayerTarget.User;
            }
            return EntityTarget;
        }
        private Player PlayerTarget { get; }
        private Player PlayerSource { get; }

        private Entity EntitySource { get; }
        public int SkillId { get; }
        public bool Critic { get; }
        public long Time { get; }
    }
}