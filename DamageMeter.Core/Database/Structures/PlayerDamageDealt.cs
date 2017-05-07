using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class PlayerDamageDealt
    {
        public PlayerDamageDealt(long amount, long critAmount, long beginTime, long endTime, long critic, long hit,
            Player source)
        {
            Amount = amount;
            CritAmount = critAmount;
            BeginTime = beginTime;
            EndTime = endTime;
            Critic = critic;
            Hit = hit;
            Source = source;
        }

        public long Amount { get; }
        public long CritAmount { get; }
        public long BeginTime { get; }

        public long EndTime { get; }
        public long Critic { get; }
        public long Hit { get; }
        public Player Source { get; }

        public double CritRate => Hit == 0 ? 0 : Critic * 100 / Hit;
        public double CritDamageRate => Amount == 0 ? 0 : CritAmount * 100 / Amount;
        public long Interval => EndTime - BeginTime;
    }
}