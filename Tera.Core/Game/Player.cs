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

        public uint PlayerId
        {
            get { return User.PlayerId; }
        }

        public string Name
        {
            get { return User.Name; }
        }

        public string GuildName
        {
            get { return User.GuildName; }
        }

        public RaceGenderClass RaceGenderClass
        {
            get { return User.RaceGenderClass; }
        }

        public PlayerClass Class
        {
            get { return RaceGenderClass.Class; }
        }

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
            return string.Format("{0} {1} [{2}]", Class, Name, GuildName);
        }
    }
}