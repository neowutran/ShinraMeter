using System.ComponentModel;
using DamageMeter.Dealt;
using DamageMeter.Taken;
using Tera.Game;

namespace DamageMeter
{
    public class PlayerInfo : INotifyPropertyChanged
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

        public EntitiesTaken Received { get; }
        public EntitiesDealt Dealt { get; }

        public event PropertyChangedEventHandler PropertyChanged;


        public bool IsHealer()
        {
            return Class == PlayerClass.Mystic || Class == PlayerClass.Priest;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}