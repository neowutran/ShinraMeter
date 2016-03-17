using System;
using System.Collections.Generic;
using System.Linq;
using DamageMeter.Skills.Skill.SkillDetail;

namespace DamageMeter.Skills.Skill
{
    public class SkillStats : ICloneable
    {
        public enum Type
        {
            Damage,
            Heal,
            Mana
        }

        private readonly Entity _entityTarget;
        private PlayerInfo _playerInfo;

        public Dictionary<int, SkillDetailStats> SkillDetails =
            new Dictionary<int, SkillDetailStats>();

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

        public long DmgBiggestCrit
        {
            get { return SkillDetails.Select(skill => skill.Value.DmgBiggestCrit).Concat(new long[] {0}).Max(); }
        }

        public long HealBiggestCrit
        {
            get { return SkillDetails.Select(skill => skill.Value.HealBiggestCrit).Concat(new long[] {0}).Max(); }
        }

        public long DmgAverageCrit
        {
            get
            {
                long averageCrit = 0;
                var numberCrits = 0;
                foreach (var skill in SkillDetails)
                {
                    numberCrits += skill.Value.CritsDmg;
                    averageCrit += skill.Value.DmgAverageCrit*skill.Value.CritsDmg;
                }
                if (numberCrits == 0)
                {
                    return 0;
                }
                return averageCrit/numberCrits;
            }
        }

        public long HealAverageCrit
        {
            get
            {
                long averageCrit = 0;
                var numberCrits = 0;
                foreach (var skill in SkillDetails)
                {
                    numberCrits += skill.Value.CritsHeal;
                    averageCrit += skill.Value.HealAverageCrit*skill.Value.CritsHeal;
                }
                if (numberCrits == 0)
                {
                    return 0;
                }
                return averageCrit/numberCrits;
            }
        }

        public long DmgBiggestHit
        {
            get { return SkillDetails.Select(skill => skill.Value.DmgBiggestHit).Concat(new long[] {0}).Max(); }
        }

        public long HealBiggestHit
        {
            get { return SkillDetails.Select(skill => skill.Value.HealBiggestHit).Concat(new long[] {0}).Max(); }
        }

        public long DmgAverageHit
        {
            get
            {
                long averageHit = 0;
                var numberHits = 0;
                foreach (var skill in SkillDetails)
                {
                    numberHits += skill.Value.HitsDmg - skill.Value.CritsDmg;
                    averageHit += skill.Value.DmgAverageHit*(skill.Value.HitsDmg - skill.Value.CritsDmg);
                }
                if (numberHits == 0)
                {
                    return 0;
                }
                return averageHit/numberHits;
            }
        }

        public long HealAverageHit
        {
            get
            {
                long averageHit = 0;
                var numberHits = 0;
                foreach (var skill in SkillDetails)
                {
                    numberHits += skill.Value.HitsHeal - skill.Value.CritsHeal;
                    averageHit += skill.Value.HealAverageHit*(skill.Value.HitsHeal - skill.Value.CritsHeal);
                }
                if (numberHits == 0)
                {
                    return 0;
                }
                return averageHit/numberHits;
            }
        }

        public long DmgAverageTotal
        {
            get
            {
                long average = 0;
                var totalHits = 0;
                foreach (var skill in SkillDetails)
                {
                    totalHits += skill.Value.HitsDmg;
                    average += skill.Value.DmgAverageTotal*skill.Value.HitsDmg;
                }
                if (totalHits == 0)
                {
                    return 0;
                }
                return average/totalHits;
            }
        }


        public long HealAverageTotal
        {
            get
            {
                long average = 0;
                var totalHits = 0;
                foreach (var skill in SkillDetails)
                {
                    totalHits += skill.Value.HitsHeal;
                    average += skill.Value.HealAverageTotal*skill.Value.HitsHeal;
                }
                if (totalHits == 0)
                {
                    return 0;
                }
                return average/totalHits;
            }
        }

        public long FirstHit
        {
            get
            {
                long firsthit = 0;
                foreach (var skill in SkillDetails)
                {
                    if ((skill.Value.FirstHit != 0 && skill.Value.FirstHit < firsthit) || firsthit == 0)
                    {
                        firsthit = skill.Value.FirstHit;
                    }
                }
                return firsthit;
            }
        }

        public long LastHit
        {
            get { return SkillDetails.Select(skill => skill.Value.LastHit).Concat(new long[] {0}).Max(); }
        }

        public long Damage
        {
            get { return SkillDetails.Sum(skill => skill.Value.Damage); }
        }

        public long Heal
        {
            get { return SkillDetails.Sum(skill => skill.Value.Heal); }
        }

        public long Mana
        {
            get { return SkillDetails.Sum(skill => skill.Value.Mana); }
        }

        public int HitsAll => HitsDmg + HitsHeal;

        public int Hits => _playerInfo.IsHealer ? HitsHeal : HitsDmg;

        public int HitsDmg
        {
            get { return SkillDetails.Sum(skill => skill.Value.HitsDmg); }
        }

        public int HitsMana
        {
            get { return SkillDetails.Sum(skill => skill.Value.HitsMana); }
        }

        public int HitsHeal
        {
            get { return SkillDetails.Sum(skill => skill.Value.HitsHeal); }
        }

        public int CritsAll => CritsDmg + CritsHeal;

        public int Crits => _playerInfo.IsHealer ? CritsHeal : CritsDmg;

        public int CritsHeal
        {
            get { return SkillDetails.Sum(skill => skill.Value.CritsHeal); }
        }

        public int CritsDmg
        {
            get { return SkillDetails.Sum(skill => skill.Value.CritsDmg); }
        }

        public object Clone()
        {
            var clone = new SkillStats(_playerInfo, _entityTarget)
            {
                SkillDetails = SkillDetails.ToDictionary(i => i.Key, i => (SkillDetailStats) i.Value.Clone())
            };
            return clone;
        }

        public void SetPlayerInfo(PlayerInfo playerInfo)
        {
            _playerInfo = playerInfo;
            foreach (var skilldetail in SkillDetails)
            {
                skilldetail.Value.PlayerInfo = playerInfo;
            }
        }

        public static SkillStats operator +(SkillStats c1, SkillStats c2)
        {
            if (c1._playerInfo != c2._playerInfo)
            {
                throw new Exception();
            }
            var skill = c1._entityTarget != c2._entityTarget
                ? new SkillStats(c1._playerInfo, null)
                : new SkillStats(c1._playerInfo, c1._entityTarget);
            var skillsDetail = c1.SkillDetails.ToDictionary(skilldetail => skilldetail.Key,
                skilldetail => skilldetail.Value);


            foreach (var skilldetail in c2.SkillDetails)
            {
                if (skillsDetail.ContainsKey(skilldetail.Key))
                {
                    skillsDetail[skilldetail.Key] += skilldetail.Value;
                }
                else
                {
                    skillsDetail.Add(skilldetail.Key, skilldetail.Value);
                }
            }


            skill.SkillDetails = skillsDetail;

            return skill;
        }

        public void AddData(int skillId, long damage, bool isCrit, Type type, long time)
        {
            if (!SkillDetails.ContainsKey(skillId))
            {
                SkillDetails[skillId] = new SkillDetailStats(_playerInfo, _entityTarget, skillId);
            }
            SkillDetails[skillId].AddData(damage, isCrit, type, time);
        }
    }
}