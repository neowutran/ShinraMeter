using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game;

namespace DamageMeter
{
    public class Abnormality
    {

        public HotDot HotDot { get; }
        public EntityId Source { get; }
        public int Stack { get; private set; }

        private long _startTime;
        private int _timeLeft;

        public int Duration { get; private set; }

        public Abnormality(HotDot hotdot, EntityId source, int duration, int stack)
        {
            HotDot = hotdot;
            Source = source;
            Duration = duration / 1000;
            _timeLeft = Duration;
            Stack = stack == 0 ? 1 : stack;
            Apply(1);
        }

        public void Update()
        {
            if (_timeLeft < HotDot.Tick)
            {
                return;
            }
            var timeCounter = (int) (Utils.Now() - _startTime);
            var numberTick = timeCounter / HotDot.Tick;
            if (numberTick != 0)
            {
                Apply(numberTick);
            }

        }

        private void Apply(int numberTick)
        {
            _startTime = Utils.Now();
            _timeLeft -= HotDot.Tick*numberTick;

            if (HotDot.Method == HotDot.DotType.abs)
            {
                var amountHp = numberTick*Stack*HotDot.Hp;
                var amountMp = numberTick*Stack*HotDot.Mp;

            
                Console.WriteLine("dot:"+HotDot.Name+";amount HP:" + amountHp + ";amount MP:" + amountMp);
                
                //TODO add damage / heal
            }
            else
            {
                //TODO Percentage, need to know target max HP / MP etc
            }
        }

    
        public void Refresh(int stackCounter, int duration)
        {
            Stack = stackCounter;
            Duration = duration / 1000;
        }

    }
}
