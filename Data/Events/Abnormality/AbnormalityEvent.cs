using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Events.Abnormality
{
    public class AbnormalityEvent : Event
    {
        public int Id { get; set; }
        public AbnormalityTargetType Target { get; set; }
        public AbnormalityTriggerType Trigger { get; set; }
        public AbnormalityEvent(bool inGame, int id, AbnormalityTargetType target, AbnormalityTriggerType trigger): base(inGame)
        {
            Id = id;
            Target = target;
            Trigger = trigger;
        }
    }
}
