using System;
using System.ComponentModel;
using Tera.DamageMeter.Annotations;

namespace Tera.DamageMeter
{
    public class SkillStats : INotifyPropertyChanged
    {
        private readonly PlayerInfo _playerInfo;
        private long _averageCrit;
        private long _averageHit;

        private long _biggestCrit;
        private long _biggestHit;
        private int _crits;
        private long _damage;
        private long _heal;
        private int _hits;
        private long _lowestCrit;
        private long _lowestHit;


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
                return Math.Round((double) Crits*100/Hits, 1);
            }
        }

        public long BiggestCrit
        {
            get { return _biggestCrit; }
            private set
            {
                if (_biggestCrit < value)
                {
                    _biggestCrit = value;
                }
            }
        }

        public long AverageCrit
        {
            get
            {
                if (Crits == 0)
                {
                    return 0;
                }
                return _averageCrit/Crits;
            }
            private set { _averageCrit += value; }
        }

        public long LowestCrit
        {
            get { return _lowestCrit; }
            private set
            {
                if ((_lowestCrit > 0 && value < _lowestCrit) || _lowestCrit == 0)
                {
                    _lowestCrit = value;
                }
            }
        }

        public long BiggestHit
        {
            get { return _biggestHit; }
            private set
            {
                if (_biggestHit < value)
                {
                    _biggestHit = value;
                }
            }
        }

        public long AverageHit
        {
            get
            {
                if (Hits == 0)
                {
                    return 0;
                }
                return _averageHit/Hits;
            }
            private set { _averageHit += value; }
        }

        public long LowestHit
        {
            get { return _lowestHit; }
            private set
            {
                if ((_lowestHit > 0 && value < _lowestHit) || _lowestHit == 0)
                {
                    _lowestHit = value;
                }
            }
        }

        public long Damage
        {
            get { return _damage; }
            private set
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
            private set
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
            private set
            {
                if (value == _crits) return;
                _crits = value;
                OnPropertyChanged("Crits");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddData(long damage, bool isCrit, bool isHeal)
        {
            Hits++;
            if (isCrit)
            {
                Crits++;
                if (isHeal)
                {
                    Heal += damage;
                }
                else
                {
                    Damage += damage;
                    BiggestCrit = damage;
                    AverageCrit = damage;
                    LowestCrit = damage;
                }
            }
            else
            {
                if (isHeal)
                {
                    Heal += damage;
                }
                else
                {
                    Damage += damage;
                    BiggestHit = damage;
                    AverageHit = damage;
                    LowestHit = damage;
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}