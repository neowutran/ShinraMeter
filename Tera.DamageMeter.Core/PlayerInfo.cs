using System.ComponentModel;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class PlayerInfo : INotifyPropertyChanged
    {
        public PlayerInfo(Player user)
        {
            FirstHit = 0;
            LastHit = 0;
            Player = user;
            Received = new SkillStats();
            Dealt = new SkillStats(this);
        }

        public Player Player { get; }

        public string Name
        {
            get { return Player.Name; }
        }

        public PlayerClass Class => Player.Class;

        public long FirstHit { get; set; }
        public long LastHit { get; set; }

        public long Interval => LastHit - FirstHit;

        public long Dps
        {
            get
            {
                if (Interval == 0)
                {
                    return 0;
                }
                return Dealt.Damage/Interval;
            }
        }

        public SkillStats Received { get; private set; }
        public SkillStats Dealt { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}