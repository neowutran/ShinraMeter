using Data.Actions;
using Data.Events;
using System.Collections.Generic;

namespace DamageMeter.UI
{
    public class AfkEventViewModel : BaseEventViewModel
    {
        public override string Type => "AFK events";

        public AfkEventViewModel(CommonAFKEvent ev, List<Action> act) : base(ev, act)
        {
        }
    }
}