using System;
using System.ComponentModel;
using Tera.DamageMeter.Properties;

namespace Tera.DamageMeter
{
    public class SkillStats
    {
        private readonly PlayerInfo _playerInfo;
        private long _averageCrit;
        private long _averageHit;

        private long _biggestCrit;
        private long _biggestHit;
        private long _damage;
        private long _lowestCrit;
        private long _lowestHit;


        public SkillStats(PlayerInfo playerInfo)
        {
            _playerInfo = playerInfo;
        }

        public void Add(SkillStats skillStats)
        {
            if (_playerInfo != skillStats._playerInfo)
            {
                return;
            }
            LowestCrit = skillStats.LowestCrit;
            BiggestHit = skillStats.BiggestCrit;
            _averageCrit = (_averageCrit + skillStats._averageCrit)/(skillStats.Crits+ Crits);
            _averageHit = (_averageHit + skillStats._averageHit)/(skillStats.Hits + Hits - Crits - skillStats.Crits);
            LowestHit = skillStats.LowestHit;
            BiggestHit = skillStats.BiggestHit;
            _damage += skillStats._damage;
            Heal += skillStats.Heal;
        }

        public double CritRate => Hits == 0 ? 0 : Math.Round((double) Crits*100/Hits, 1);

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
                if (Hits == 0 || Crits == Hits)
                {
                    return 0;
                }
                return _averageHit/(Hits - Crits);
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
            }
        }

        public long Heal { get; private set; }

        public int Hits { get; set; }

        public int Crits { get; private set; }

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
    }
}