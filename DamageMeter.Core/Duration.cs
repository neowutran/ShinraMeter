using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class Duration: ICloneable
    {

        public long Begin { get; }
        public long End { get; private set; }

        public Duration(long begin, long end)
        {
            End = end;
            Begin = begin;
        }

        public void Update(long end)
        {
            End = end;
        }

        public object Clone()
        {
            return new Duration(Begin, End);
            
        }
    }
}
