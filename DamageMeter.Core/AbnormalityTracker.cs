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
            AddAbnormality(message.TargetId, message.SourceId, message.Duration, message.Stack, message.AbnormalityId);
        }

        private void AddAbnormality(EntityId target, EntityId source, int duration, int stack, int abnormalityId)
        {
            if (!_abnormalities.ContainsKey(target))
            {
                _abnormalities.Add(target, new List<Abnormality>());
            }
            var hotdot = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityId);
            _abnormalities[target].Add(new Abnormality(hotdot, source, target, duration, stack));
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
                abnormality.Refresh(message.StackCounter, message.Duration);
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
            _abnormalities.Remove(message.Npc);
        }


        public void Update(SPlayerChangeMp message)
        {
            Update(message.TargetId, message.SourceId, message.MpChange, message.Type, message.Critical == 1, false);
        }

        private void Update(EntityId target, EntityId source, int change, int type, bool critical, bool isHp)
        {
            //  Console.WriteLine(";isHp:" + isHp + ";amount:" + change + ";type:" + type);

            // SystemHot/Dot 
            if (
                (int) HotDotDatabase.HotDot.SystemHot == type ||
                (int) HotDotDatabase.HotDot.CrystalHpHot == type ||
                (int) HotDotDatabase.HotDot.NaturalMpRegen == type ||
                (int) HotDotDatabase.HotDot.StuffMpHot == type
                )
            {
                /*
                var skillResult = NetworkController.Instance.ForgeSkillResult(
                   true,
                   change,
                   critical,
                   isHp,
                   type*-1, //unknow ID
                   source,
                   target);


                DamageTracker.Instance.Update(skillResult);

                NetworkController.Instance.CheckUpdateUi();
                */
                return;
            }


            if (!_abnormalities.ContainsKey(target))
            {
                //   Console.WriteLine("ERROR 1: HPCHANGE= " + change);
                return;
            }

            var abnormalities = _abnormalities[target];
            abnormalities = abnormalities.OrderByDescending(o => o.TimeBeforeApply).ToList();

            foreach (var abnormality in abnormalities)
            {
                //     Console.WriteLine("Check 0 : HPCHANGE= " + change + ";id:"+abnormality.HotDot.Id+";source:"+source+";abno source:"+abnormality.Source);


                if (abnormality.Source != source && abnormality.Source != abnormality.Target)
                {
                    continue;
                }

                //  Console.WriteLine("Check 1 : HPCHANGE= " + change);

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

                abnormality.Apply(change, critical, isHp);
                return;
            }
            //Console.WriteLine("ERROR 2: HPCHANGE= " + change);
        }


        public void Update(SCreatureChangeHp message)
        {
            Update(message.TargetId, message.SourceId, message.HpChange, message.Type, message.Critical == 1, true);
        }
    }
}