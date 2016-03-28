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

        public void AddAbnormality(EntityId target, EntityId source, int duration, int stack, int abnormalityId,
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

            if (_abnormalities[target].Where(x => x.HotDot.Id == abnormalityId).Count() == 0) //dont add existing abnormalities (!!! Big issue !!!!!! Abnormality Duration fucked up)
                _abnormalities[target].Add(new Abnormality(hotdot, source, target, duration, stack, ticks));
        }

        public void RefreshAbnormality(SAbnormalityRefresh message)
        {
            if (!_abnormalities.ContainsKey(message.TargetId))
            {
                return;
            }
            var abnormalityUser = _abnormalities[message.TargetId];
            foreach (var abnormality in abnormalityUser)
            {
                if (abnormality.HotDot.Id != message.AbnormalityId) continue;
                abnormality.Refresh(message.StackCounter, message.Duration, message.Time.Ticks);
                return;
            }
        }

        public bool AbnormalityExist(EntityId target, HotDot dot)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                return false;
            }
            var abnormalityTarget = _abnormalities[target];
            for(var i = 0; i < abnormalityTarget.Count; i++)
            {
                if(abnormalityTarget[i].HotDot == dot)
                {
                    return true;
                }
            }
            return false;
        }

        private void DeleteAbnormality(EntityId target, int abnormalityId, long ticks)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                return;
            }

            var abnormalityUser = _abnormalities[target];

            for (var i = 0; i < abnormalityUser.Count; i++)
            {
                if (abnormalityUser[i].HotDot.Id == abnormalityId)
                {
                    abnormalityUser[i].ApplyBuffDebuff(ticks);
                    abnormalityUser.Remove(abnormalityUser[i]);
                }
            }

            if (abnormalityUser.Count == 0)
            {
                _abnormalities.Remove(target);
                return;
            }
            _abnormalities[target] = abnormalityUser;
        }

        public void DeleteAbnormality(SAbnormalityEnd message)
        {
            DeleteAbnormality(message.TargetId, message.AbnormalityId, message.Time.Ticks);
        }
        public void DeleteAbnormality(SDespawnNpc message)
        {
            DeleteAbnormality(message.Npc, message.Time.Ticks);
        }

        public void DeleteAbnormality(SNpcStatus message)
        {
            DeleteAbnormality(message.Npc, 8888888, message.Time.Ticks);
        }

        public void DeleteAbnormality(SDespawnUser message)
        {
            DeleteAbnormality(message.User, message.Time.Ticks);
        }

        private void DeleteAbnormality(EntityId entity, long ticks)
        {
            if (!_abnormalities.ContainsKey(entity))
            {
                return;
            }
            foreach (var abno in _abnormalities[entity])
            {
                abno.ApplyBuffDebuff(ticks);
            }
            _abnormalities.Remove(entity);
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

        public void Update(SCreatureChangeHp message)
        {
            Update(message.TargetId, message.SourceId, message.HpChange, message.Type, message.Critical == 1, true,
                message.Time.Ticks);
        }
    }
}