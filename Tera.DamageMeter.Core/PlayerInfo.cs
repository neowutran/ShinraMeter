using System.ComponentModel;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class PlayerInfo : INotifyPropertyChanged
    {
        public User User { get; private set; }

        public string Name { get { return User.Name; } }
        public PlayerClass Class { get { return User.Class; } }

        public SkillStats Received { get; private set; }
        public SkillStats Dealt { get; private set; }

        public PlayerInfo(User user)
        {
            User = user;
            Received = new SkillStats();
            Dealt = new SkillStats();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
