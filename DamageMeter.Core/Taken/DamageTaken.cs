using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Taken
{
    public class DamageTaken
    {

        public long Damage
        {
            get
            {
                return _damage;
            }
            set
            {
                _damage += value;
                Hits++;
            }
        }

        public int Hits { get; private set; }

        public DamageTaken()
        {
            
        }

        private long _damage = 0;
    }
}
