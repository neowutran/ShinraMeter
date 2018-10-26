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
        protected Event(bool inGame, bool active, int priority, List<BlackListItem> areaBossBlackList, bool outOfCombat = false, List<PlayerClass> ignoreClasses=null)
        {
            NextChecks = new Dictionary<EntityId, DateTime>();
            InGame = inGame;
            Active = active;
            Priority = priority;
            AreaBossBlackList = areaBossBlackList;
            OutOfCombat = outOfCombat;
            IgnoreClasses = ignoreClasses ?? new List<PlayerClass>();
        }

        public Dictionary<EntityId, DateTime> NextChecks { get; set; }
        public bool InGame { get; set; }
        public bool Active { get; set; }
        public bool OutOfCombat { get; set; }

        public List<BlackListItem> AreaBossBlackList { get; set; }
        public List<PlayerClass> IgnoreClasses { get; set; }

        public int Priority { get; set; }
    }
}