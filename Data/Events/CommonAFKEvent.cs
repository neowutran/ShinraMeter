using System.Collections.Generic;

namespace Data.Events
{
    public class CommonAFKEvent : Event
    {
        public CommonAFKEvent(bool active) : base(false, active, 0, new List<BlackListItem>()) { }
    }
}