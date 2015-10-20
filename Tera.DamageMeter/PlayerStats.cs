using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game;

namespace Tera.DamageMeter
{
    public class Stats
    {
        public long Damage { get; set; }
        public long Heal { get; set; }
        public int Hits { get; set; }
        public int Crits { get; set; }
    }

    public class PlayerStats
    {
        public User User { get; private set; }

        public string Name { get { return User.Name; } }
        public PlayerClass Class { get { return User.Class; } }

        public Stats Received { get; private set; }
        public Stats Dealt { get; private set; }

        public PlayerStats(User user)
        {
            User = user;
            Received = new Stats();
            Dealt = new Stats();
        }
    }
}
