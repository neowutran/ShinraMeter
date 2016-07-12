using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class Skill
    {
        public Skill(long amount, Database.Type type, EntityId target, EntityId source, int skillId, bool hotdot,
            bool critic, long time, NpcInfo pet, HitDirection direction)
        {
            Amount = amount;
            Type = type;
            Target = target;
            Source = source;
            SkillId = skillId;
            Critic = critic;
            HotDot = hotdot;
            Time = time;
            Pet = pet;
            Direction = direction;
        }

        public HitDirection Direction { get; }

        public NpcInfo Pet { get; }

        public bool HotDot { get; }
        public long Amount { get; }
        public Database.Type Type { get; }
        public EntityId Target { get; }
        public EntityId Source { get; }
        public int SkillId { get; }
        public bool Critic { get; }
        public long Time { get; }
    }
}