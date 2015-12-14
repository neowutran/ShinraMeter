using System;

namespace Tera.Game
{
    public class Player: IEquatable<object>
    {
        private UserEntity _user;

        public Player(UserEntity user)
        {
            _user = user;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Player)obj);
        }

        public bool Equals(Player other)
        {
            return Name.Equals(other.Name) && PlayerId.Equals(other.PlayerId);
        }

        public static bool operator ==(Player a, Player b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Player a, Player b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ PlayerId.GetHashCode();
        }


        public uint PlayerId => User.PlayerId;

        public string Name => User.Name;

        public string GuildName => User.GuildName;

        public RaceGenderClass RaceGenderClass => User.RaceGenderClass;

        public PlayerClass Class => RaceGenderClass.Class;

        public UserEntity User
        {
            get { return _user; }
            set
            {
                if (_user.PlayerId != value.PlayerId)
                    throw new ArgumentException("Users must represent the same Player");
                _user = value;
            }
        }

        public override string ToString()
        {
            return $"{Class} {Name} [{GuildName}]";
        }
    }
}