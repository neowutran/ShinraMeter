using System;
using System.Collections.Generic;
using System.Linq;
using Tera.Game;

namespace DamageMeter
{
    public class AbnormalityDuration : ICloneable
    {
        private List<Duration> _listDuration = new List<Duration>();

        public AbnormalityDuration(PlayerClass playerClass, long start)
        {
            InitialPlayerClass = playerClass;
            Start(start);
        }

        private AbnormalityDuration(PlayerClass playerClass)
        {
            InitialPlayerClass = playerClass;
        }

        public PlayerClass InitialPlayerClass { get; }

        public object Clone()
        {
            var newListDuration = _listDuration.Select(duration => (Duration) duration.Clone()).ToList();
            var abnormalityDuration = new AbnormalityDuration(InitialPlayerClass)
            {
                _listDuration = newListDuration
            };
            return abnormalityDuration;
        }

        public long Duration(long begin, long end)
        {
            long totalDuration = 0;
            foreach (var duration in _listDuration)
            {
                if (begin > duration.End || end < duration.Begin)
                {
                    continue;
                }

                var abnormalityBegin = duration.Begin;
                var abnormalityEnd = duration.End;

                if (begin > abnormalityBegin)
                {
                    abnormalityBegin = begin;
                }

                if (end < abnormalityEnd)
                {
                    abnormalityEnd = end;
                }

                totalDuration += abnormalityEnd - abnormalityBegin;
            }
            return totalDuration;
        }

        public void Start(long start)
        {
            if (_listDuration.Count != 0) {
                if (!Ended())
                {
                    //Console.WriteLine("Can't restart something that has not been ended yet");
                    return;
                }
           }
            _listDuration.Add(new Duration(start, long.MaxValue));
        }

        public void End(long end)
        {
            if (Ended())
            {
                //Console.WriteLine("Can't end something that has already been ended");
                return;
            }

            _listDuration[_listDuration.Count - 1].End = end;
        }

        public long LastStart()
        {
            return _listDuration[_listDuration.Count - 1].Begin;
        }

        public long LastEnd()
        {
            return _listDuration[_listDuration.Count - 1].End;
        }

        public int Count(long begin, long end) {
            int count = 0;
            foreach (var duration in _listDuration)
            {
                if (begin > duration.End || end < duration.Begin)
                {
                    continue;
                }
                count++;
            }
            return count;
        }


        public bool Ended()
        {
            return _listDuration[_listDuration.Count - 1].End != long.MaxValue;
        }
    }
}