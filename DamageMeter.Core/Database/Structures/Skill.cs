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
            Source  = PlayerSource?.User ?? EntitySource;
            Target = PlayerTarget?.User ?? EntityTarget;
            
        }

        public HitDirection Direction { get; }

        public NpcInfo Pet { get; }

        public bool HotDot { get; }
        public long Amount { get; }
        public Database.Type Type { get; }
        private Entity EntityTarget { get; }
        public Entity Source { get; private set; }

        public Entity Target { get; private set; }
        private Player PlayerTarget { get; }
        private Player PlayerSource { get; }

        private Entity EntitySource { get; }
        public int SkillId { get; }
        public bool Critic { get; }
        public long Time { get; }
    }
}