using DamageMeter.Database.Structures;
using Data;
using System;
using System.Collections.Generic;

namespace DamageMeter.UI
{
    public class DpsSource
    {
        // fields
        private long _prevAmount;
        private double _damageSum;
        private long _interval = TimeSpan.TicksPerSecond * BasicTeraData.Instance.WindowData.RealtimeGraphCMAseconds;

        // properties
        public ulong Id { get; }
        public Queue<Tuple<double, long>> Values { get; }
        public double Avg => Values.Count == 0 ? _damageSum : _damageSum * TimeSpan.TicksPerSecond / _interval;

        public double Dps { get; private set; }
        // ctor
        public DpsSource(ulong id)
        {
            Id = id;
            Values = new Queue<Tuple<double, long>>();
        }

        // methods
        public void Update(PlayerDamageDealt newValue, EntityInformation entityInfo)
        {
            switch (BasicTeraData.Instance.WindowData.GraphMode)
            {
                case GraphMode.CMA:
                    var newAmount = newValue.Amount;
                    // get the amount of damage done between this and previous time instants
                    var dmgDiff = Convert.ToDouble(newAmount - _prevAmount);

                    // update values
                    var now = DateTime.Now.Ticks;
                    _damageSum += dmgDiff;

                    // queue damage and time deltas
                    Values.Enqueue(new Tuple<double, long>(dmgDiff, now));

                    _prevAmount = newAmount;

                    if (Values.Count < BasicTeraData.Instance.WindowData.RealtimeGraphCMAseconds) return;

                    // remove first value pair in the queue and subtract it from total damage and total time
                    while (Values.Peek().Item2 < now - _interval)
                    {
                        var val = Values.Dequeue();
                        _damageSum -= val.Item1;
                    }
                    break;
                case GraphMode.DPS:
                    Dps = entityInfo.Interval == 0 ? newValue.Amount : newValue.Amount * TimeSpan.TicksPerSecond / entityInfo.Interval;
                    break;
            }
        }
    }
}