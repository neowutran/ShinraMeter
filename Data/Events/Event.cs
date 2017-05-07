using System;
using System.Collections.Generic;
using Tera.Game;

namespace Data.Events
{
    public struct BlackListItem
    {
        public int AreaId;
        public int BossId;
    }

    public abstract class Event
    {
        protected Event(bool inGame, bool active, int priority, List<BlackListItem> areaBossBlackList)
        {
            NextChecks = new Dictionary<EntityId, DateTime>();
            InGame = inGame;
            Active = active;
            Priority = priority;
            AreaBossBlackList = areaBossBlackList;
        }

        public Dictionary<EntityId, DateTime> NextChecks { get; set; }
        public bool InGame { get; set; }
        public bool Active { get; set; }

        public List<BlackListItem> AreaBossBlackList { get; set; }

        public int Priority { get; set; }
    }
}