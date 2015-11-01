using System;
using System.ComponentModel;
using Tera.DamageMeter.Annotations;

namespace Tera.DamageMeter
{
    public class SkillStats : INotifyPropertyChanged
    {
        private readonly PlayerInfo _playerInfo;
        private int _crits;
        private long _damage;
        private long _heal;
        private int _hits;

        public SkillStats()
        {
        }

        public SkillStats(PlayerInfo playerInfo)
        {
            _playerInfo = playerInfo;
        }

        public double CritRate
        {
            get
            {
                if (Hits == 0)
                {
                    return 0;
                }
                return Math.Round((double)Crits*100/Hits, 1);
            }
        } 

        public long Damage
        {
            get { return _damage; }
            set
            {
                if (value == _damage) return;
                if (_playerInfo != null)
                {
                    if (value != 0)
                    {
                        if (_playerInfo.FirstHit == 0)
                        {
                            _playerInfo.FirstHit = DateTime.UtcNow.Ticks/10000000;
                        }
                        _playerInfo.LastHit = DateTime.UtcNow.Ticks/10000000;
                    }
                }
                _damage = value;
                OnPropertyChanged("Damage");
            }
        }

        public long Heal
        {
            get { return _heal; }
            set
            {
                if (value == _heal) return;
                _heal = value;
                OnPropertyChanged("Heal");
            }
        }

        public int Hits
        {
            get { return _hits; }
            set
            {
                if (value == _hits) return;
                _hits = value;
                OnPropertyChanged("Hits");
            }
        }

        public int Crits
        {
            get { return _crits; }
            set
            {
                if (value == _crits) return;
                _crits = value;
                OnPropertyChanged("Crits");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}