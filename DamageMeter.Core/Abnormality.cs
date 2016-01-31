using System;
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
        }

        public HotDot HotDot { get; }
        public EntityId Source { get; }
        public int Stack { get; private set; }

        public EntityId Target { get; }

        public int Duration { get; private set; }

        public long LastApply { get; private set; }

        public long FirstHit { get; private set; }

        public long TimeBeforeApply => DateTime.UtcNow.Ticks - LastApply - HotDot.Tick*10000000;

        public void Apply(int amount, bool critical, bool isHp, long time)
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
            DamageTracker.Instance.Update(skillResult, time);

            NetworkController.Instance.CheckUpdateUi();
            LastApply = time;
        }

        public void ApplyEnduranceDebuff(long lastTicks)
        {
            if (HotDot.Type != "Endurance") return;
            foreach (var entityStats in DamageTracker.Instance.EntitiesStats)
            {
                if (entityStats.Key.Id != Target) continue;
                var entity = entityStats.Key;
                if (!DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime.ContainsKey(HotDot))
                {
                    var npcEntity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(Source);
                    if (!(npcEntity is UserEntity))
                    {
                        return;
                    }
                    var user = (UserEntity) npcEntity;
                    var abnormalityInitDuration = new AbnormalityDuration
                    {
                        InitialPlayerClass = user.RaceGenderClass.Class,
                        Duration = 0
                    };
                    DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime.Add(HotDot, abnormalityInitDuration);
                }
                DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime[HotDot].Duration += lastTicks - FirstHit;
                FirstHit = lastTicks;
                return;
            }
        }


        public void Refresh(int stackCounter, int duration, long time)
        {
            Stack = stackCounter;
            Duration = duration/1000;
            ApplyEnduranceDebuff(time);
        }
    }
}