using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class DamageTracker : IEnumerable<PlayerInfo>
    {
        private readonly Dictionary<Player, PlayerInfo> _statsByUser = new Dictionary<Player, PlayerInfo>();

        public DamageTracker()
        {
            TotalDealt = new SkillsStats();
            TotalReceived = new SkillsStats();
        }

        public SkillsStats TotalDealt { get; private set; }
        public SkillsStats TotalReceived { get; private set; }

        public IEnumerator<PlayerInfo> GetEnumerator()
        {
            return _statsByUser.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private PlayerInfo GetOrCreate(Player player)
        {
            PlayerInfo playerStats;
            if (!_statsByUser.TryGetValue(player, out playerStats))
            {
                playerStats = new PlayerInfo(player);
                _statsByUser.Add(player, playerStats);
            }

            return playerStats;
        }

        public void Update(SkillResult skillResult)
        {
            if (skillResult.SourcePlayer != null)
            {
                var playerStats = GetOrCreate(skillResult.SourcePlayer);
                UpdateStats(playerStats.Dealt, skillResult);
            }

            if (skillResult.TargetPlayer != null)
            {
                var playerStats = GetOrCreate(skillResult.TargetPlayer);
                UpdateStats(playerStats.Received, skillResult);
            }
        }

        private void UpdateStats(SkillsStats stats, SkillResult message)
        {
            if (message.Amount == 0)
            {
                return;
            }

            if ((UserEntity.ForEntity(message.Source) == UserEntity.ForEntity(message.Target)) && (message.Damage > 0))
            {
                return;
            }

            var skillName = message.SkillId+"";
            if (message.Skill != null)
            {
                skillName = message.Skill.Name;
            }
            SkillStats skillStats;
            var skillKey = new Skill(skillName, new List<int>{ message.SkillId });

            stats.Skills.TryGetValue(skillKey, out skillStats);
            if (skillStats == null)
            {
                skillStats = new SkillStats(stats.PlayerInfo);
            }

            skillStats.AddData(message.IsHeal ? message.Heal : message.Damage, message.IsCritical, message.IsHeal);
            List<Skill> skill = stats.Skills.Keys.ToList();
            int indexSkill = skill.IndexOf(skillKey);
            if (indexSkill != -1)
            {
                foreach (var skillid in skill[indexSkill].SkillId.Where(skillid => !skillKey.SkillId.Contains(skillid)))
                {
                    skillKey.SkillId.Add(skillid);
                }
            }
            stats.Skills.Remove(skillKey);
            stats.Skills.Add(skillKey,skillStats);
        }
    }
}