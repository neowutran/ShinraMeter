using System.Collections.Generic;
using System.Linq;
using Data;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class AbnormalityTracker
    {
        private static AbnormalityTracker _instance;

        private readonly Dictionary<EntityId, List<Abnormality>> _abnormalities =
            new Dictionary<EntityId, List<Abnormality>>();

        private AbnormalityTracker()
        {
        }

        public static AbnormalityTracker Instance => _instance ?? (_instance = new AbnormalityTracker());

        public void AddAbnormality(SAbnormalityBegin message)
        {
            AddAbnormality(message.TargetId, message.SourceId, message.Duration, message.Stack, message.AbnormalityId,
                message.Time.Ticks);
        }

        private void AddAbnormality(EntityId target, EntityId source, int duration, int stack, int abnormalityId,
            long ticks)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                _abnormalities.Add(target, new List<Abnormality>());
            }
            var hotdot = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityId);
            if (hotdot == null)
            {
                return;
            }
            _abnormalities[target].Add(new Abnormality(hotdot, source, target, duration, stack, ticks));
        }

        public void RefreshAbnormality(SAbnormalityRefresh message)
        {
            if (!_abnormalities.ContainsKey(message.TargetId))
            {
                //  AddAbnormality(message.TargetId, message.SourceId, message.Duration, message.StackCounter, message.AbnormalityId);
                //TODO ADD ABNORMALITY, too lazy to do it now, NEED A SOURCE, WITHOUT SOURCE, RETURN
                return;
            }
            var abnormalityUser = _abnormalities[message.TargetId];
            foreach (var abnormality in abnormalityUser)
            {
                if (abnormality.HotDot.Id != message.AbnormalityId) continue;
                abnormality.Refresh(message.StackCounter, message.Duration, message.Time.Ticks);

                return;
            }

            //TODO ADD ABNORMALITY, too lazy to do it now (aka: Gameforge star event, go farm), NEED A SOURCE, WITHOUT SOURCE, RETURN
        }

        public void DeleteAbnormality(SAbnormalityEnd message)
        {
            if (!_abnormalities.ContainsKey(message.TargetId))
            {
                return;
            }

            var abnormalityUser = _abnormalities[message.TargetId];

            for (var i = 0; i < abnormalityUser.Count; i++)
            {
                if (abnormalityUser[i].HotDot.Id == message.AbnormalityId)
                {
                    abnormalityUser[i].ApplyEnduranceDebuff(message.Time.Ticks);
                    abnormalityUser.Remove(abnormalityUser[i]);
                }
            }

            if (abnormalityUser.Count == 0)
            {
                _abnormalities.Remove(message.TargetId);
                return;
            }
            _abnormalities[message.TargetId] = abnormalityUser;
        }

        public void DeleteAbnormality(SDespawnNpc message)
        {
            if (!_abnormalities.ContainsKey(message.Npc))
            {
                return;
            }
            foreach (var abno in _abnormalities[message.Npc])
            {
                abno.ApplyEnduranceDebuff(message.Time.Ticks);
            }
            _abnormalities.Remove(message.Npc);
        }


        public void Update(SPlayerChangeMp message)
        {
            Update(message.TargetId, message.SourceId, message.MpChange, message.Type, message.Critical == 1, false,
                message.Time.Ticks);
        }

        private void Update(EntityId target, EntityId source, int change, int type, bool critical, bool isHp, long time)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                return;
            }

            var abnormalities = _abnormalities[target];
            abnormalities = abnormalities.OrderByDescending(o => o.TimeBeforeApply).ToList();

            foreach (var abnormality in abnormalities)
            {
                if (abnormality.Source != source && abnormality.Source != abnormality.Target)
                {
                    continue;
                }

                if (isHp)
                {
                    if ((!(abnormality.HotDot.Hp > 0) || change <= 0) &&
                        (!(abnormality.HotDot.Hp < 0) || change >= 0)
                        ) continue;
                }
                else
                {
                    if ((!(abnormality.HotDot.Mp > 0) || change <= 0) &&
                        (!(abnormality.HotDot.Mp < 0) || change >= 0)
                        ) continue;
                }

                if ((int) HotDotDatabase.HotDot.Dot != type && (int) HotDotDatabase.HotDot.Hot != type)
                {
                    continue;
                }

                abnormality.Apply(change, critical, isHp, time);
                return;
            }
        }

        public void ApplyEnduranceDebuff(long time)
        {
            foreach (var abnormality in _abnormalities.SelectMany(abnormalityList => abnormalityList.Value))
            {
                abnormality.ApplyEnduranceDebuff(time);
            }
        }


        public void Update(SCreatureChangeHp message)
        {
            Update(message.TargetId, message.SourceId, message.HpChange, message.Type, message.Critical == 1, true,
                message.Time.Ticks);
        }
    }
}