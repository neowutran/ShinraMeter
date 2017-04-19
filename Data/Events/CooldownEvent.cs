using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Events
{
    public class CooldownEvent : Event
    {

        public int SkillId { get; set; }
        public bool OnlyResetted { get; set; }

        public CooldownEvent(bool inGame, bool active, int priority, int skillId, bool onlyResetted) : base(inGame, active, priority, new List<BlackListItem>())
        {
            SkillId = skillId;
            OnlyResetted = onlyResetted;
        }
    }
}
