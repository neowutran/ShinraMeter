using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace Data.Events
{
    public abstract class Event
    {
        public Dictionary<EntityId, DateTime> NextChecks { get; set; }
        public bool InGame { get; set; }
        public bool Active { get; set; }

        public Dictionary<int, int> AreaBossBlackList { get; set; }

        public int Priority { get; set; }
        public Event(bool inGame, bool active, int priority, Dictionary<int,int> areaBossBlackList)
        {
            NextChecks = new Dictionary<EntityId, DateTime>();
            InGame = inGame;
            Active = active;
            Priority = priority;
            AreaBossBlackList = areaBossBlackList;
        }
    }
}
