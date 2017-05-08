using System.Collections.Generic;

namespace Data.Events
{
    public class CooldownEvent : Event
    {
        public CooldownEvent(bool inGame, bool active, int priority, int skillId, bool onlyResetted) : base(inGame, active, priority, new List<BlackListItem>())
        {
            SkillId = skillId;
            OnlyResetted = onlyResetted;
        }

        public int SkillId { get; set; }
        public bool OnlyResetted { get; set; }
    }
}