using System;
using System.Collections.Concurrent;
using System.Linq;
using Tera.DamageMeter.Skills.Skill.SkillDetail;

namespace Tera.DamageMeter.Skills.Skill
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

        public ConcurrentDictionary<int, SkillDetailStats> SkillDetails =
            new ConcurrentDictionary<int, SkillDetailStats>();

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
            get { return SkillDetails.Select(skill => skill.Value.BiggestCrit).Concat(new long[] {0}).Max(); }
        }

        public long AverageCrit
        {
            get
            {
                long averageCrit = 0;
                var numberCrits = 0;
                foreach (var skill in SkillDetails)
                {
                    numberCrits += skill.Value.Crits;
                    averageCrit += skill.Value.AverageCrit*skill.Value.Crits;
                }
                if (numberCrits == 0)
                {
                    return 0;
                }
                return averageCrit/numberCrits;
            }
        }

        public long BiggestHit
        {
            get { return SkillDetails.Select(skill => skill.Value.BiggestHit).Concat(new long[] {0}).Max(); }
        }

        public long AverageHit
        {
            get
            {
                long averageHit = 0;
                var numberHits = 0;
                foreach (var skill in SkillDetails)
                {
                    numberHits += skill.Value.Hits - skill.Value.Crits;
                    averageHit += skill.Value.AverageHit*(skill.Value.Hits - skill.Value.Crits);
                }
                if (numberHits == 0)
                {
                    return 0;
                }
                return averageHit/numberHits;
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

        public int Hits => _playerInfo.IsHealer() ? HitsHeal : HitsDmg;

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

        public int Crits => _playerInfo.IsHealer() ? CritsHeal : CritsDmg;

        public int CritsHeal
        {
            get { return SkillDetails.Sum(skill => skill.Value.CritsHeal); }
        }

        public int CritsDmg
        {
            get { return SkillDetails.Sum(skill => skill.Value.CritsDmg); }
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
            var skillsDetail = new ConcurrentDictionary<int, SkillDetailStats>();

            foreach (var skilldetail in c1.SkillDetails)
            {
                skillsDetail.TryAdd(skilldetail.Key, skilldetail.Value);
            }


            foreach (var skilldetail in c2.SkillDetails)
            {
                skillsDetail.TryAdd(skilldetail.Key, skilldetail.Value);
            }


            skill.SkillDetails = skillsDetail;

            return skill;
        }

        public void AddData(int skillId, long damage, bool isCrit, Type type)
        {
            if (!SkillDetails.ContainsKey(skillId))
            {
                SkillDetails[skillId] = new SkillDetailStats(_playerInfo, _entityTarget, skillId);
            }
            SkillDetails[skillId].AddData(damage, isCrit, type);
        }
    }
}