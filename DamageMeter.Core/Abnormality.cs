using System;
using System.Windows.Forms.VisualStyles;
using Data;
using Tera.Game;

namespace DamageMeter
{
    public class Abnormality
    {
        public Abnormality(HotDot hotdot, EntityId source, EntityId target, int duration, int stack, long ticks)
        {
            HotDot = hotdot;
            Source = source;
            Target = target;
            Duration = duration/1000;
            Stack = stack == 0 ? 1 : stack;
            FirstHit = ticks;
            Console.WriteLine("DOT:id="+HotDot.Id+"; Duration:"+Duration);
        }

        public HotDot HotDot { get; }
        public EntityId Source { get; }
        public int Stack { get; private set; }

        public EntityId Target { get; }

        public int Duration { get; private set; }

        public long LastApply { get; private set; }

        public long FirstHit { get; private set; }

        public long TimeBeforeApply => (Utils.Now() - LastApply) - HotDot.Tick;

        public void Apply(int amount, bool critical, bool isHp)
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

        public void ApplyRemove(long lastTicks)
        {
            if (HotDot.Id != 27160) return;
            Console.WriteLine("Appli remove:"+HotDot.Id);
            foreach (var entityStats in DamageTracker.Instance.EntitiesStats)
            {
                if (entityStats.Key.Id != Target) continue;
                var entity = entityStats.Key;
                DamageTracker.Instance.EntitiesStats[entity].VolleyOfCurse += (lastTicks - FirstHit)/ 10000000;
                Console.WriteLine("Constit debuff: "+DamageTracker.Instance.EntitiesStats[entity].VolleyOfCurse+";"+ DamageTracker.Instance.EntitiesStats[entity].Interval + "%="+ (DamageTracker.Instance.EntitiesStats[entity].VolleyOfCurse * 100) / DamageTracker.Instance.EntitiesStats[entity].Interval);
                return;
            }
            
        }


        public void Refresh(int stackCounter, int duration)
        {
            Stack = stackCounter;
            Duration = duration/1000;
        }
    }
}