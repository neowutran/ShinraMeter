using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class PlayerHealDealt
    {
        public PlayerHealDealt(long critic, long hit, Player source)
        {
            Critic = critic;
            Hit = hit;
            Source = source;
        }

     
        public long Critic { get; }
        public long Hit { get; }
        public Player Source { get; }
        public double CritRate => Hit==0?0:Critic*100/Hit;
    }
}