using System.Collections.Generic;
using Tera.Game;

namespace Data.Events.Abnormality
{
    public class AbnormalityEvent : Event
    {
        public AbnormalityEvent(bool inGame, bool active, int priority, List<BlackListItem> areaBossBlackList, Dictionary<int, int> ids, List<HotDot.Types> types,
            AbnormalityTargetType target, AbnormalityTriggerType trigger, int remainingSecondsBeforeTrigger, int rewarnTimeoutSecounds, bool outOfCombat, List<PlayerClass> ignoreClasses) 
                : base(inGame, active, priority, areaBossBlackList, outOfCombat, ignoreClasses)
        {
            Types = types;
            Ids = ids;
            Target = target;
            Trigger = trigger;
            RemainingSecondBeforeTrigger = remainingSecondsBeforeTrigger;
            RewarnTimeoutSeconds = rewarnTimeoutSecounds;
        }

        public Dictionary<int, int> Ids { get; set; }


        public int RemainingSecondBeforeTrigger { get; set; }
        public int RewarnTimeoutSeconds { get; set; }
        public List<HotDot.Types> Types { get; set; }
        public AbnormalityTargetType Target { get; set; }
        public AbnormalityTriggerType Trigger { get; set; }
    }
}