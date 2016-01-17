using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game;

namespace DamageMeter
{
    public class Abnormality
    {

        public HotDot HotDot { get; }
        public EntityId Source { get; }
        public int Stack { get; private set; }

        public EntityId Target { get; }

        public int Duration { get; private set; }

        public long LastApply { get; private set; }

        public long TimeBeforeApply => (Utils.Now() - LastApply) - HotDot.Tick;

        public Abnormality(HotDot hotdot, EntityId source, EntityId target, int duration, int stack)
        {
            HotDot = hotdot;
            Source = source;
            Target = target;
            Duration = duration / 1000;
            Stack = stack == 0 ? 1 : stack;
        }

        

        public void Apply(int amount, bool critical, bool isHp )
        {
          
           //     Console.WriteLine("dot:"+HotDot.Name+";amount:" + amount + ";Hp:" + isHp);
                var skillResult = NetworkController.Instance.ForgeSkillResult(
                    true,
                    amount,
                    critical,
                    isHp,
                    HotDot.Id,
                    Source,
                    Target);
                DamageTracker.Instance.Update(skillResult);

            NetworkController.Instance.CheckUpdateUi();
            LastApply = Utils.Now();
        }

    
        public void Refresh(int stackCounter, int duration)
        {
            Stack = stackCounter;
            Duration = duration / 1000;
        }

    }
}
