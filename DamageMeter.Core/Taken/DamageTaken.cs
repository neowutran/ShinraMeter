using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Taken
{
    public class DamageTaken
    {

        public long Damage { get; private set; } = 0;

        public int Hits { get; private set; }

        public DamageTaken()
        {
            
        }

        public void AddDamage(long damage)
        {
            Damage += damage;
            Hits++;
        }
    }
}
