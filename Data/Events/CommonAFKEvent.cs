using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Events
{
    public class CommonAFKEvent : Event
    {
        public CommonAFKEvent(bool active) : base(false, active, 0, new Dictionary<int, int>())
        {
        }
    }
}
