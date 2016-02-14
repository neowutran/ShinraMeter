using System;
using Data;
using Tera.Game;

namespace DamageMeter
{
    public class Abnormality
    {
        private bool _buffRegistered;

        private bool _enduranceDebuffRegistered;

        public Abnormality(HotDot hotdot, EntityId source, EntityId target, int duration, int stack, long ticks)
        {
            HotDot = hotdot;
            Source = source;
            Target = target;
            Duration = duration/1000;
            Stack = stack == 0 ? 1 : stack;
            FirstHit = ticks/TimeSpan.TicksPerSecond;
            if (HotDot.Name == "") return;
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
            var skillResult = NetworkController.Instance.ForgeSkillResult(
                true,
                amount,
                critical,
                isHp,
                HotDot.Id,
                Source,
                Target);
            DamageTracker.Instance.Update(skillResult, time);
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
            var entityGame = NetworkController.Instance.EntityTracker.GetOrPlaceholder(Target);
            Entity entity = null;
            var game = entityGame as NpcEntity;
            if (game != null)
            {
                var target = game;
                entity = new Entity(target.CategoryId, target.Id, target.NpcId, target.NpcArea);
            }

            if (entity == null)
            {
                return;
            }
            DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime[HotDot].ListDuration[
                DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime[HotDot].ListDuration.Count - 1].Update(
                    lastTicks);
        }


        private void RegisterEnduranceDebuff()
        {
            if (HotDot.Type != "Endurance" || HotDot.Amount > 1) return;
            var duration = new Duration(FirstHit, long.MaxValue);
            var entityGame = NetworkController.Instance.EntityTracker.GetOrPlaceholder(Target);
            Entity entity = null;
            var game = entityGame as NpcEntity;
            if (game != null)
            {
                var target = game;
                entity = new Entity(target.CategoryId, target.Id, target.NpcId, target.NpcArea);
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
                var npcEntity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(Source);
                if (!(npcEntity is UserEntity))
                {
                    return;
                }
                var user = (UserEntity) npcEntity;
                var abnormalityInitDuration = new AbnormalityDuration(user.RaceGenderClass.Class);
                abnormalityInitDuration.ListDuration.Add(duration);
                DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime.Add(HotDot, abnormalityInitDuration);
                _enduranceDebuffRegistered = true;
                return;
            }

            DamageTracker.Instance.EntitiesStats[entity].AbnormalityTime[HotDot].ListDuration.Add(duration);
            _enduranceDebuffRegistered = true;
        }

        private void RegisterBuff()
        {
            if (HotDot.Type == "HPChange" || HotDot.Type == "MPChange") return;
            var userEntity = NetworkController.Instance.EntityTracker.GetOrNull(Target);

            if (!(userEntity is UserEntity))
            {
                return;
            }
            var player = new Player((UserEntity) userEntity);

            if (!DamageTracker.Instance.UsersStats.ContainsKey(player))
            {
                DamageTracker.Instance.UsersStats.Add(player, new PlayerInfo(player));
            }

            var duration = new Duration(FirstHit, long.MaxValue);

            if (!DamageTracker.Instance.UsersStats[player].AbnormalityTime.ContainsKey(HotDot))
            {
                var npcEntity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(Source);
                PlayerClass playerClass;
                if (!(npcEntity is UserEntity))
                {
                    playerClass = PlayerClass.Common;
                }
                else
                {
                    playerClass = ((UserEntity) npcEntity).RaceGenderClass.Class;
                }
                var abnormalityInitDuration = new AbnormalityDuration(playerClass);
                abnormalityInitDuration.ListDuration.Add(duration);
                DamageTracker.Instance.UsersStats[player].AbnormalityTime.Add(HotDot, abnormalityInitDuration);
                _buffRegistered = true;
                return;
            }

            ////////// !! WARNING !!
            var count = DamageTracker.Instance.UsersStats[player].AbnormalityTime[HotDot].ListDuration.Count;
            if (count > 0)
            {
                if (DamageTracker.Instance.UsersStats[player].AbnormalityTime[HotDot].ListDuration[count - 1].End ==
                    long.MaxValue)
                {
                    return;
                }
            }
            ////////////

            DamageTracker.Instance.UsersStats[player].AbnormalityTime[HotDot].ListDuration.Add(duration);
            _buffRegistered = true;
        }

        private void ApplyBuff(long lastTicks)
        {
            if (HotDot.Type == "HPChange" || HotDot.Type == "MPChange") return;
            if (_buffRegistered == false) return;
            var userEntity = NetworkController.Instance.EntityTracker.GetOrNull(Target);
            if (!(userEntity is UserEntity))
            {
                return;
            }
            var player = new Player((UserEntity) userEntity);
            DamageTracker.Instance.UsersStats[player].AbnormalityTime[HotDot].ListDuration[
                DamageTracker.Instance.UsersStats[player].AbnormalityTime[HotDot].ListDuration.Count - 1].Update(
                    lastTicks);
        }

        public void Refresh(int stackCounter, int duration, long time)
        {
            Stack = stackCounter;
            Duration = duration/1000;
        }
    }
}