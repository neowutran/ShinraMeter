using System.ComponentModel;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class PlayerInfo : INotifyPropertyChanged
    {
        public PlayerInfo(Player user)
        {
            Player = user;
            Received = new SkillsStats();
            Dealt = new Entities(this);
        }

        public Player Player { get; }

        public string Name => Player.Name;

        public PlayerClass Class => Player.Class;

        public SkillsStats Received { get; private set; }
        public Entities Dealt { get; }

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