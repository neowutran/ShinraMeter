using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class Death
    {
        private AbnormalityDuration _death;

        public Death()
        {

        }

        public int Count(long begin, long end)
        {
            if(_death == null)
            {
                return 0;
            }
            return _death.Count(begin, end);
        }

        public long Duration(long begin, long end)
        {
            if(_death == null)
            {
                return 0;
            }
            return _death.Duration(begin, end);
        }

        public void Start(long begin)
        {
            if(_death == null)
            {
                _death = new AbnormalityDuration(Tera.Game.PlayerClass.Common, begin);
                return;
            }
            _death.Start(begin);
        }

        public void End(long begin)
        {
            if(_death == null)
            {
                return;
            }
            _death.End(begin);
        }


    }
}
