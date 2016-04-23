using System;
using Data;
using Tera.Game;
using System.Collections.Generic;

namespace DamageMeter
{
    public class Abnormality
    {
        private bool _buffRegistered;

        private bool _enduranceDebuffRegistered;
        private AbnormalityTracker _abnormalityTracker;
        public Abnormality(HotDot hotdot, EntityId source, EntityId target, int duration, int stack, long ticks, AbnormalityTracker abnormalityTracker)
        {
            HotDot = hotdot;
            Source = source;
            Target = target;
            Duration = duration/1000;
            Stack = stack == 0 ? 1 : stack;
            FirstHit = ticks/TimeSpan.TicksPerSecond;
            if (HotDot.Name == "") return;
            _abnormalityTracker = abnormalityTracker;
            RegisterBuff();
            RegisterEnduranceDebuff();
        }

        public HotDot HotDot { get; }
        public EntityId Source { get; }
        public int Stack { get; private set; }

        public EntityId Target { get; }

        public int Duration { get; private set; }

        public long LastApply { get; private set; }

        public long FirstHit { get; }

        public long TimeBeforeApply => DateTime.UtcNow.Ticks - LastApply - HotDot.Tick*10000000;

        public void Apply(int amount, bool critical, bool isHp, long time)
        {
            var skillResult =
                new SkillResult(
                true,
                amount,
                critical,
                isHp,
                HotDot.Id,
                Source,
                Target,
                _abnormalityTracker.EntityTracker
                );

            if (_abnormalityTracker.UpdateDamageTracker != null) _abnormalityTracker.UpdateDamageTracker(skillResult, time);
            LastApply = time;
        }

        public void ApplyBuffDebuff(long tick)
        {
            if (HotDot.Name == "") return;
            tick /= TimeSpan.TicksPerSecond;
            ApplyBuff(tick);
            ApplyEnduranceDebuff(tick);
        }

        private void ApplyEnduranceDebuff(long lastTicks)
        {
            if (HotDot.Type != "Endurance" || HotDot.Amount > 1) return;
            if (_enduranceDebuffRegistered == false) return;
            var entityGame = _abnormalityTracker.EntityTracker.GetOrPlaceholder(Target);
            Entity entity = null;
            var game = entityGame as NpcEntity;
            if (game != null)
            {
                var target = game;
                entity = new Entity(target);
            }

            if (entity == null)
            {
                return;
            }

            DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime[HotDot].End(lastTicks);
        }


        private void RegisterEnduranceDebuff()
        {
            if (HotDot.Type != "Endurance" || HotDot.Amount > 1) return;
            var entityGame = _abnormalityTracker.EntityTracker.GetOrPlaceholder(Target);
            Entity entity = null;
            var game = entityGame as NpcEntity;
            if (game != null)
            {
                var target = game;
                entity = new Entity(target);
            }

            if (entity == null)
            {
                return;
            }

            if (!DamageTracker.Instance.EntitiesStats.ContainsKey(entity))
            {
                DamageTracker.Instance.EntitiesStats.Add(entity, new EntityInfo());
            }

            if (!DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime.ContainsKey(HotDot))
            {
                var npcEntity = _abnormalityTracker.EntityTracker.GetOrPlaceholder(Source);
                if (!(npcEntity is UserEntity))
                {
                    return;
                }
                var user = (UserEntity) npcEntity;
                var abnormalityInitDuration = new AbnormalityDuration(user.RaceGenderClass.Class, FirstHit);
                DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime.Add(HotDot, abnormalityInitDuration);
                _enduranceDebuffRegistered = true;
                return;
            }

            DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime[HotDot].Start(FirstHit);
            _enduranceDebuffRegistered = true;
        }

        private void RegisterBuff()
        {
            if (HotDot.Type == "HPChange" || HotDot.Type == "MPChange") return;
            var userEntity = _abnormalityTracker.EntityTracker.GetOrNull(Target);

            if (!(userEntity is UserEntity))
            {
                return;
            }
            var player = _abnormalityTracker.PlayerTracker.GetOrUpdate((UserEntity) userEntity);

            if (!DamageTracker.Instance.UsersStats.ContainsKey(player))
            {
                DamageTracker.Instance.UsersStats.Add(player, new PlayerInfo(player));
            }

            if (!DamageTracker.Instance.UsersStats[player].AbnormalityTime.ContainsKey(HotDot))
            {
                var npcEntity = _abnormalityTracker.EntityTracker.GetOrPlaceholder(Source);
                PlayerClass playerClass;
                if (!(npcEntity is UserEntity))
                {
                    playerClass = PlayerClass.Common;
                }
                else
                {
                    playerClass = ((UserEntity) npcEntity).RaceGenderClass.Class;
                }
                var abnormalityInitDuration = new AbnormalityDuration(playerClass, FirstHit);
                DamageTracker.Instance.UsersStats[player].AbnormalityTime.Add(HotDot, abnormalityInitDuration);
                _buffRegistered = true;
                return;
            }

          
            DamageTracker.Instance.UsersStats[player].AbnormalityTime[HotDot].Start(FirstHit);
            _buffRegistered = true;
        }
        private void testerr(Dictionary<HotDot, AbnormalityDuration> testerror,long lastTicks)
        {
            testerror[HotDot].End(lastTicks);
        }

        private void ApplyBuff(long lastTicks)
        {
            if (HotDot.Type == "HPChange" || HotDot.Type == "MPChange") return;
            if (_buffRegistered == false) return;
            var userEntity = _abnormalityTracker.EntityTracker.GetOrNull(Target);
            if (!(userEntity is UserEntity))
            {
                return;
            }
            var player = _abnormalityTracker.PlayerTracker.GetOrUpdate((UserEntity)userEntity);
            //DamageTracker.Instance.UsersStats[player].AbnormalityTime[HotDot].End(lastTicks);
            var testerror = DamageTracker.Instance.UsersStats[player].AbnormalityTime;
            testerr(testerror,lastTicks);
        }

        public void Refresh(int stackCounter, int duration, long time)
        {
            Stack = stackCounter;
            Duration = duration/1000;
        }
    }
}