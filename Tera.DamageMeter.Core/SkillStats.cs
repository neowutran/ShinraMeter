using System;

namespace Tera.DamageMeter
{
    public class SkillStats
    {
        public enum Type
        {
            Damage,
            Heal,
            Mana
        };

        private readonly Entity _entityTarget;
        private readonly PlayerInfo _playerInfo;
        private long _averageCrit;
        private long _averageHit;

        private long _biggestCrit;
        private long _biggestHit;
        private long _damage;
        private long _lowestCrit;
        private long _lowestHit;

        public SkillStats(PlayerInfo playerInfo, Entity entityTarget)
        {
            _playerInfo = playerInfo;
            _entityTarget = entityTarget;
        }

        public SkillStats(PlayerInfo playerInfo)
        {
            _playerInfo = playerInfo;
        }

        public double DamagePercentage
            => _playerInfo.Dealt.Damage == 0 ? 0 : Math.Round((double) Damage*100/_playerInfo.Dealt.Damage, 1);

        public double CritRate => Hits == 0 ? 0 : Math.Round((double) Crits*100/Hits, 1);

        public double CritRateHeal => HitsHeal == 0 ? 0 : Math.Round((double) CritsHeal*100/HitsHeal, 1);
        public double CritRateDmg => HitsDmg == 0 ? 0 : Math.Round((double) CritsDmg*100/HitsDmg, 1);

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

        public long FirstHit { get; set; }
        public long LastHit { get; set; }

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
                        if (FirstHit == 0)
                        {
                            FirstHit = DateTime.UtcNow.Ticks/10000000;
                        }
                        LastHit = DateTime.UtcNow.Ticks/10000000;
                    }
                }
                _damage = value;
            }
        }

        public long Heal { get; private set; }

        public long Mana { get; private set; }

        public int HitsAll => HitsDmg + HitsHeal;

        public int Hits => _playerInfo.IsHealer() ? HitsHeal : HitsDmg;

        public int HitsDmg { get; private set; }

        public int HitsMana { get; private set; }

        public int HitsHeal { get; private set; }

        public int CritsAll => CritsDmg + CritsHeal;

        public int Crits => _playerInfo.IsHealer() ? CritsHeal : CritsDmg;

        public int CritsHeal { get; private set; }
        public int CritsDmg { get; private set; }

        public static SkillStats operator +(SkillStats c1, SkillStats c2)
        {
            if (c1._playerInfo != c2._playerInfo)
            {
                throw new Exception();
            }
            var skill = c1._entityTarget != c2._entityTarget
                ? new SkillStats(c1._playerInfo, null)
                : new SkillStats(c1._playerInfo, c1._entityTarget);
            skill.LowestCrit = c1.LowestCrit;
            skill.LowestCrit = c2.LowestCrit;

            skill.BiggestCrit = c1.BiggestCrit;
            skill.BiggestCrit = c2.BiggestCrit;
            skill.HitsHeal = c1.HitsHeal + c2.HitsHeal;
            skill.HitsDmg = c1.HitsDmg + c2.HitsDmg;
            skill.Mana = c1.Mana + c2.Mana;
            skill.HitsMana = c1.HitsMana + c2.HitsMana;

            skill.CritsDmg = c1.CritsDmg + c2.CritsDmg;
            skill.CritsHeal = c1.CritsHeal + c2.CritsHeal;

            if (skill.Crits == 0)
            {
                skill._averageCrit = 0;
            }
            else
            {
                skill._averageCrit = (c1._averageCrit*c1.Crits + c2._averageCrit*c2.Crits)/(skill.Crits);
            }
            if (skill.Hits - skill.Crits == 0)
            {
                skill._averageHit = 0;
            }
            else
            {
                skill._averageHit = (c1._averageHit*(c1.Hits - c1.Crits) + c2._averageHit*(c2.Hits - c2.Crits))/
                                    (skill.Hits - skill.Crits);
            }

            skill.LowestHit = c1.LowestHit;
            skill.LowestHit = c2.LowestHit;

            skill.BiggestHit = c1.BiggestHit;
            skill.BiggestHit = c2.BiggestHit;

            skill.FirstHit = c1.FirstHit > c2.FirstHit ? c2.FirstHit : c1.FirstHit;
            skill.LastHit = c1.LastHit > c2.LastHit ? c1.LastHit : c2.LastHit;


            skill._damage = c1._damage + c2._damage;
            skill.Heal = c1.Heal + c2.Heal;
            return skill;
        }

        private void SetTotalDamage(long damage)
        {
            if (_entityTarget == null) return;
            if (DamageTracker.Instance.TotalDamageEntity.ContainsKey(_entityTarget))
            {
                DamageTracker.Instance.TotalDamageEntity[_entityTarget] += damage;
            }
            else
            {
                DamageTracker.Instance.TotalDamageEntity[_entityTarget] = damage;
            }
            DamageTracker.Instance.SetFirstHit(_entityTarget);
            DamageTracker.Instance.SetLastHit(_entityTarget);
            DamageTracker.Instance.UpdateCurrentBoss(_entityTarget);
        }


        public void AddData(long damage, bool isCrit, Type type)
        {
            if (isCrit)
            {
                switch (type)
                {
                    case Type.Heal:
                        CritsHeal++;
                        HitsHeal++;
                        Heal += damage;
                        break;
                    case Type.Damage:
                        HitsDmg++;
                        CritsDmg++;
                        Damage += damage;
                        BiggestCrit = damage;
                        AverageCrit = damage;
                        LowestCrit = damage;
                        SetTotalDamage(damage);
                        break;
                    case Type.Mana:
                    default:
                        throw new Exception("NO CRIT ON MANA");
                }
            }
            else
            {
                switch (type)
                {
                    case Type.Heal:
                        HitsHeal++;
                        Heal += damage;
                        break;
                    case Type.Damage:
                        HitsDmg++;
                        Damage += damage;
                        BiggestHit = damage;
                        AverageHit = damage;
                        LowestHit = damage;
                        SetTotalDamage(damage);
                        break;
                    case Type.Mana:
                    default:
                        HitsMana++;
                        Mana += damage;
                        break;
                }
            }
        }
    }
}