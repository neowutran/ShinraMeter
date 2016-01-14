using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class Utils
    {

        public static long Now()
        {
            return DateTime.UtcNow.Ticks / 10000000;
        }

    }
}
