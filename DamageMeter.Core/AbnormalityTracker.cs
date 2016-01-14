using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class AbnormalityTracker
    {

        private static AbnormalityTracker _instance;
        public static AbnormalityTracker Instance => _instance ?? (_instance = new AbnormalityTracker());

        private readonly Dictionary<EntityId,List<Abnormality>> _abnormalities = new Dictionary<EntityId, List<Abnormality>>(); 

        private AbnormalityTracker()
        {
        }

        public void AddAbnormality(SAbnormalityBegin message)
        {
            var target = message.TargetId;
            if (!_abnormalities.ContainsKey(target))
            {
                _abnormalities.Add(target,new List<Abnormality>());
            }
            var hotdot = BasicTeraData.Instance.HotDotDatabase.Get(message.AbnormalityId);
            if (hotdot == null)
            {
                Console.WriteLine("UNKNOW DOT");
                return;
            }
           
            _abnormalities[target].Add(new Abnormality(hotdot, message.SourceId, message.Duration, message.Stack));
            
        }

        public void RefreshAbnormality(SAbnormalityRefresh message)
        {
            var hotdot = BasicTeraData.Instance.HotDotDatabase.Get(message.AbnormalityId);
            if (hotdot == null)
            {
                Console.WriteLine("UNKNOW DOT");
                return;
            }
            var abnormalityUser = _abnormalities[message.TargetId];
            foreach (var abnormality in abnormalityUser)
            {
                if (abnormality.HotDot.Id != message.AbnormalityId) continue;
                abnormality.Refresh(message.StackCounter, message.Duration);
                break;
            }
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

        public void Update()
        {
            foreach (var abnormality in _abnormalities.SelectMany(userAbnormality => userAbnormality.Value))
            {
                abnormality.Update();
            }
        }
    }
}
