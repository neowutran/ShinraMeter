using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Events
{
    public abstract class Event
    {

        public bool InGame { get; set; }
        public Event(bool inGame)
        {
            InGame = inGame;
        }
    }
}
