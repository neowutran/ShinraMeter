using System;
using Tera.Game;

namespace DamageMeter.Skills.Skill.SkillDetail
{
    public class SkillDetailStats : ICloneable
    {
        private readonly Entity _entityTarget;
        private long _damage;
        private long _dmgAverageCrit;
        private long _dmgAverageHit;

        private long _dmgBiggestCrit;
        private long _dmgBiggestHit;
        private long _dmgLowestCrit;
        private long _dmgLowestHit;
        private long _healAverageCrit;

        private long _healAverageHit;

        private long _healBiggestCrit;

        private long _healBiggestHit;


        public SkillDetailStats(PlayerInfo playerInfo, Entity entityTarget, int skillId)
        {
            PlayerInfo = playerInfo;
            _entityTarget = entityTarget;
            Id = skillId;
        }

        public PlayerInfo PlayerInfo { get; set; }

        public int Id { get; }

        public double DamagePercentage
            => PlayerInfo.Dealt.Damage == 0 ? 0 : Math.Round((double) Damage*100/PlayerInfo.Dealt.Damage, 1);

        public double CritRate => Hits == 0 ? 0 : Math.Round((double) Crits*100/Hits, 1);

        public double CritRateHeal => HitsHeal == 0 ? 0 : Math.Round((double) CritsHeal*100/HitsHeal, 1);
        public double CritRateDmg => HitsDmg == 0 ? 0 : Math.Round((double) CritsDmg*100/HitsDmg, 1);

        public long DmgBiggestCrit
        {
            get { return _dmgBiggestCrit; }
            private set
            {
                if (_dmgBiggestCrit < value)
                {
                    _dmgBiggestCrit = value;
                }
            }
        }

        public long DmgAverageCrit
        {
            get
            {
                if (CritsDmg == 0)
                {
                    return 0;
                }
                return _dmgAverageCrit/CritsDmg;
            }
            private set { _dmgAverageCrit += value; }
        }

        public long DmgLowestCrit
        {
            get { return _dmgLowestCrit; }
            private set
            {
                if ((_dmgLowestCrit > 0 && value < _dmgLowestCrit) || _dmgLowestCrit == 0)
                {
                    _dmgLowestCrit = value;
                }
            }
        }

        public long DmgBiggestHit
        {
            get { return _dmgBiggestHit; }
            private set
            {
                if (_dmgBiggestHit < value)
                {
                    _dmgBiggestHit = value;
                }
            }
        }

        public long HealBiggestHit
        {
            get { return _healBiggestHit; }
            private set
            {
                if (_healBiggestHit < value)
                {
                    _healBiggestHit = value;
                }
            }
        }

        public long HealBiggestCrit
        {
            get { return _healBiggestCrit; }
            private set
            {
                if (_healBiggestCrit < value)
                {
                    _healBiggestCrit = value;
                }
            }
        }

        public long HealAverageCrit
        {
            get
            {
                if (CritsHeal == 0)
                {
                    return 0;
                }
                return _healAverageCrit/CritsHeal;
            }
            private set { _healAverageCrit += value; }
        }

        public long DmgAverageHit
        {
            get
            {
                if (HitsDmg == 0 || CritsDmg == HitsDmg)
                {
                    return 0;
                }
                return _dmgAverageHit/(HitsDmg - CritsDmg);
            }
            private set { _dmgAverageHit += value; }
        }


        public long HealAverageHit
        {
            get
            {
                if (HitsHeal == 0 || CritsHeal == HitsHeal)
                {
                    return 0;
                }
                return _healAverageHit/(HitsHeal - CritsHeal);
            }
            private set { _healAverageHit += value; }
        }

        public long DmgAverageTotal
        {
            get
            {
                if (HitsDmg == 0)
                {
                    return 0;
                }
                return _damage/HitsDmg;
            }
        }

        public long HealAverageTotal
        {
            get
            {
                if (HitsHeal == 0)
                {
                    return 0;
                }
                return Heal/HitsHeal;
            }
        }

        public long DmgLowestHit
        {
            get { return _dmgLowestHit; }
            private set
            {
                if ((_dmgLowestHit > 0 && value < _dmgLowestHit) || _dmgLowestHit == 0)
                {
                    _dmgLowestHit = value;
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
                if (PlayerInfo != null)
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

        public int Hits => PlayerInfo.IsHealer() ? HitsHeal : HitsDmg;

        public int HitsDmg { get; private set; }

        public int HitsMana { get; private set; }

        public int HitsHeal { get; private set; }

        public int CritsAll => CritsDmg + CritsHeal;

        public int Crits => PlayerInfo.IsHealer() ? CritsHeal : CritsDmg;

        public int CritsHeal { get; private set; }
        public int CritsDmg { get; private set; }

        public object Clone()
        {
            var newskill = new SkillDetailStats(PlayerInfo, _entityTarget, Id);
            newskill.DmgLowestCrit = DmgLowestCrit;

            newskill.DmgBiggestCrit = DmgBiggestCrit;
            newskill.HealBiggestCrit = HealBiggestCrit;
            newskill.HitsHeal = HitsHeal;
            newskill.HitsDmg = HitsDmg;
            newskill.Mana = Mana;
            newskill.HitsMana = HitsMana;

            newskill.CritsDmg = CritsDmg;
            newskill.CritsHeal = CritsHeal;
            newskill._dmgAverageCrit = _dmgAverageCrit;
            newskill._dmgAverageHit = _dmgAverageHit;
            newskill._healAverageCrit = _healAverageCrit;
            newskill._healAverageHit = _healAverageHit;
            newskill.DmgLowestHit = DmgLowestHit;

            newskill.DmgBiggestHit = DmgBiggestHit;
            newskill.HealBiggestHit = HealBiggestHit;


            newskill.FirstHit = FirstHit;
            newskill.LastHit = LastHit;


            newskill._damage = _damage;
            newskill.Heal = Heal;


            return newskill;
        }

        public static SkillDetailStats operator +(SkillDetailStats c1, SkillDetailStats c2)
        {
            if (c1.PlayerInfo != c2.PlayerInfo)
            {
                throw new Exception();
            }
            var skill = c1._entityTarget != c2._entityTarget
                ? new SkillDetailStats(c1.PlayerInfo, null, c1.Id)
                : new SkillDetailStats(c1.PlayerInfo, c1._entityTarget, c1.Id);
            skill.DmgLowestCrit = c1.DmgLowestCrit;
            skill.DmgLowestCrit = c2.DmgLowestCrit;

            skill.DmgBiggestCrit = c1.DmgBiggestCrit;
            skill.DmgBiggestCrit = c2.DmgBiggestCrit;
            skill.HealBiggestCrit = c1.HealBiggestCrit;
            skill.HealBiggestCrit = c2.HealBiggestCrit;
            skill.HitsHeal = c1.HitsHeal + c2.HitsHeal;
            skill.HitsDmg = c1.HitsDmg + c2.HitsDmg;
            skill.Mana = c1.Mana + c2.Mana;
            skill.HitsMana = c1.HitsMana + c2.HitsMana;

            skill.CritsDmg = c1.CritsDmg + c2.CritsDmg;
            skill.CritsHeal = c1.CritsHeal + c2.CritsHeal;


            skill._dmgAverageCrit = c1._dmgAverageCrit + c2._dmgAverageCrit;

            skill._dmgAverageHit = c1._dmgAverageHit + c2._dmgAverageHit;
            skill._healAverageCrit = c1._healAverageCrit + c2._healAverageCrit;
            skill._healAverageHit = c1._healAverageHit + c2._healAverageHit;
            skill.DmgLowestHit = c1.DmgLowestHit;
            skill.DmgLowestHit = c2.DmgLowestHit;

            skill.DmgBiggestHit = c1.DmgBiggestHit;
            skill.DmgBiggestHit = c2.DmgBiggestHit;
            skill.HealBiggestHit = c1.HealBiggestHit;
            skill.HealBiggestHit = c2.HealBiggestHit;


            skill.FirstHit = c1.FirstHit > c2.FirstHit ? c2.FirstHit : c1.FirstHit;
            skill.LastHit = c1.LastHit > c2.LastHit ? c1.LastHit : c2.LastHit;


            skill._damage = c1._damage + c2._damage;
            skill.Heal = c1.Heal + c2.Heal;
            return skill;
        }

        private void SetTotalDamage(long damage)
        {
            if (_entityTarget == null) return;
            lock (DamageTracker.Instance.TotalDamageEntity)
            {
                if (DamageTracker.Instance.TotalDamageEntity.ContainsKey(_entityTarget))
                {
                    DamageTracker.Instance.TotalDamageEntity[_entityTarget] += damage;
                }
                else
                {
                    DamageTracker.Instance.TotalDamageEntity[_entityTarget] = damage;
                }
            }
            DamageTracker.Instance.SetFirstHit(_entityTarget);
            DamageTracker.Instance.SetLastHit(_entityTarget);
            DamageTracker.Instance.UpdateCurrentBoss(_entityTarget);
        }


        public void AddData(long damage, bool isCrit, SkillStats.Type type)
        {
            if (isCrit)
            {
                switch (type)
                {
                    case SkillStats.Type.Heal:
                        CritsHeal++;
                        HitsHeal++;
                        Heal += damage;
                        HealBiggestCrit = damage;
                        HealAverageCrit = damage;
                        break;
                    case SkillStats.Type.Damage:
                        HitsDmg++;
                        CritsDmg++;
                        Damage += damage;
                        DmgBiggestCrit = damage;
                        DmgAverageCrit = damage;
                        DmgLowestCrit = damage;
                        SetTotalDamage(damage);
                        break;
                    default:
                        throw new Exception("NO CRIT ON MANA");
                }
            }
            else
            {
                switch (type)
                {
                    case SkillStats.Type.Heal:
                        HitsHeal++;
                        Heal += damage;
                        HealBiggestHit = damage;
                        HealAverageHit = damage;
                        break;
                    case SkillStats.Type.Damage:
                        HitsDmg++;
                        Damage += damage;
                        DmgBiggestHit = damage;
                        DmgAverageHit = damage;
                        DmgLowestHit = damage;
                        SetTotalDamage(damage);
                        break;
                    default:
                        HitsMana++;
                        Mana += damage;
                        break;
                }
            }
        }
    }
}