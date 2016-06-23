using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class PlayerDealt
    {

        public PlayerDealt(long amount, long beginTime, long endTime, long critic, long hit, Player source, EntityId target, Database.Type type)
        {
            Amount = amount;
            BeginTime = beginTime;
            EndTime = endTime;
            Critic = critic;
            Hit = hit;
            Source = source;
            Target = target;
            Type = type;    
        }

        public Database.Type Type { get; }

        public long Amount { get; }
        public long BeginTime { get; }

        public long EndTime { get; }
        public long Critic { get; }
        public long Hit { get; }
        public Player Source { get; }

        public EntityId Target { get; }

        public double CritRate => Critic / Hit;
        public long Interval => EndTime - BeginTime;

    }
}
