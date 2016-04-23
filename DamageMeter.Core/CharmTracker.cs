using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter
{
    public class CharmTracker
    {
        private readonly Dictionary<EntityId, List<uint>> _charms = new Dictionary<EntityId, List<uint>>();
        private AbnormalityTracker _abnormalityTracker;
        public CharmTracker(AbnormalityTracker tracker) { _abnormalityTracker = tracker; }

        internal void CharmAdd(EntityId target, uint charmId, byte status, long ticks)
        {
            if (status == 1)
            {
                if (!_charms.ContainsKey(target)) _charms[target] = new List<uint>();
                _charms[target].Add(charmId);
                _abnormalityTracker.AddAbnormality(target, new EntityId(0), 0, 0, (int)charmId, ticks);
                //Console.WriteLine(BitConverter.ToString(BitConverter.GetBytes(target.Id)) + " AAdd :" + charmId);
            }
            else
            {
                if (_charms.ContainsKey(target))
                    if (_charms[target].Contains(charmId)) _charms[target].Remove(charmId);
                _abnormalityTracker.DeleteAbnormality(target, (int)charmId, ticks);
                //Console.WriteLine(BitConverter.ToString(BitConverter.GetBytes(target.Id)) + " ADel :" + charmId);
            }
        }
        internal void CharmEnable(EntityId target, uint charmId, long ticks)
        {
            if (!_charms.ContainsKey(target)) _charms[target] = new List<uint>();
            _charms[target].Add(charmId);
            _abnormalityTracker.AddAbnormality(target, new EntityId(0), 0, 0, (int)charmId, ticks);
            //Console.WriteLine(BitConverter.ToString(BitConverter.GetBytes(target.Id))+" Enb :"+charmId);
        }

        internal void CharmReset(EntityId target, List<CharmStatus> charms, long ticks)
        {
            if (_charms.ContainsKey(target))
            {
                foreach (var charm in _charms[target])
                {
                    _abnormalityTracker.DeleteAbnormality(target, (int)charm, ticks);
                    //Console.WriteLine(BitConverter.ToString(BitConverter.GetBytes(target.Id)) + " reset :" + charm);
                }
            }
            _charms[target] = new List<uint>();
            foreach (var charm in charms)
            {
                if (charm.Status == 1)
                {
                    _abnormalityTracker.AddAbnormality(target, new EntityId(0), 0, 0, (int)charm.CharmId, ticks);
                    _charms[target].Add(charm.CharmId);
                    //Console.WriteLine($"{BitConverter.ToString(BitConverter.GetBytes(target.Id))} {charm.Status == 1} : {charm.CharmId}");
                }
            }
            if (_charms[target].Count() == 0) _charms.Remove(target);
        }

        internal void CharmDel(EntityId target, uint charmId, long ticks)
        {
            //Console.WriteLine(BitConverter.ToString(BitConverter.GetBytes(target.Id)) + " Del :" + charmId);
            if (_charms.ContainsKey(target))
                if (_charms[target].Contains(charmId)) _charms[target].Remove(charmId);
            _abnormalityTracker.DeleteAbnormality(target, (int)charmId, ticks);
        }
    }
}
