using System;

namespace Tera.Game
{
    public class Player
    {
        private UserEntity _user;

        public Player(UserEntity user)
        {
            _user = user;
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