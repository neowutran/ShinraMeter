using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class PlayerDamageDealt
    {
        public PlayerDamageDealt(long amount, long beginTime, long endTime, long critic, long hit, Player source,
            EntityId target)
        {
            Amount = amount;
            BeginTime = beginTime;
            EndTime = endTime;
            Critic = critic;
            Hit = hit;
            Source = source;
            Target = target;
        }

        public long Amount { get; }
        public long BeginTime { get; }

        public long EndTime { get; }
        public long Critic { get; }
        public long Hit { get; }
        public Player Source { get; }

        public EntityId Target { get; }

        public double CritRate => Critic*100/Hit;
        public long Interval => EndTime - BeginTime;
    }
}