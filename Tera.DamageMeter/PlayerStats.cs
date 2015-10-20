using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game;

namespace Tera.DamageMeter
{
    public class PlayerStats
    {
        public User User { get;private set; }

        public string Name { get { return User.Name; } }
        public PlayerClass Class { get { return User.Class; } }

        public long Damage { get; set; }
        public long Heal { get; set; }
        public long DamageReceived { get; set; }
        public long HealReceived { get; set; }

        public PlayerStats(User user)
        {
            User = user;
        }
    }
}
