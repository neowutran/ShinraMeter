using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class Skill
    {
        public Skill(long amount, Database.Type type, EntityId target, EntityId source, int skill_id, bool hotdot,
            bool critic, long time)
        {
            Amount = amount;
            Type = type;
            Target = target;
            Source = source;
            SkillId = skill_id;
            Critic = critic;
            HotDot = hotdot;
            Time = time;
        }

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