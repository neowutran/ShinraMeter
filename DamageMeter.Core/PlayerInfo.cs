using System;
using System.Collections.Generic;
using System.Linq;
using DamageMeter.Dealt;
using DamageMeter.Taken;
using Tera.Game;

namespace DamageMeter
{
    public class PlayerInfo : ICloneable, IEquatable<object>
    {
        public PlayerInfo(Player user)
        {
            Player = user;
            Received = new EntitiesTaken();
            Dealt = new EntitiesDealt(this);
        }


        public Player Player { get; }

        public string Name => Player.Name;

        public PlayerClass Class => Player.Class;

        public EntitiesTaken Received { get; private set; }
        public EntitiesDealt Dealt { get; private set; }


        public object Clone()
        {
            var clone = new PlayerInfo(Player)
            {
                Received = (EntitiesTaken) Received.Clone(),
                Dealt = (EntitiesDealt) Dealt.Clone(),
            };
            clone.Dealt.SetPlayerInfo(clone);
            return clone;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PlayerInfo) obj);
        }

        public bool Equals(PlayerInfo other)
        {
            return Player.Equals(other.Player);
        }

        public static bool operator ==(PlayerInfo a, PlayerInfo b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(PlayerInfo a, PlayerInfo b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Player.GetHashCode();
        }

        public bool IsHealer => Player.IsHealer;
    }
}