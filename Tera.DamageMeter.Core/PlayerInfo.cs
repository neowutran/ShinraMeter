using System.ComponentModel;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class PlayerInfo : INotifyPropertyChanged
    {
        public Player Player { get; private set; }

        public string Name { get { return Player.Name; } }
        public PlayerClass Class { get { return Player.Class; } }

        public SkillStats Received { get; private set; }
        public SkillStats Dealt { get; private set; }

        public PlayerInfo(Player user)
        {
            Player = user;
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
