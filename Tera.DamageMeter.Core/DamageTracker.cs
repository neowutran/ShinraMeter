using System;
using System.Collections;
using System.Collections.Generic;
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
                var str = string.Format("{0:x}", message.SkillId);
                Console.WriteLine("nothing" + str);
                return;
            }

            if ((UserEntity.ForEntity(message.Source) == UserEntity.ForEntity(message.Target)) && (message.Damage > 0))
            {
                return;
            }


            /**
            Lol, debug hard TODO remove when debugging end
            */
            Console.WriteLine("source:" + message.SourcePlayer);
            Console.WriteLine(";source_id:" + message.Source.Id);
            if (message.Skill != null)
            {
                Console.WriteLine(";skill:" + message.Skill.Name);
            }
            Console.WriteLine(
                ";skill_id:" + message.SkillId +
                ";damage:" + message.Damage +
                ";target:" + message.TargetPlayer +
                ";target_id: " + message.Target.Id +
                "; heal:" + message.Heal +
                ";amout:" + message.Amount
                );
            var skillName = "";
            if (message.Skill != null)
            {
                skillName = message.Skill.Name;
            }
            SkillStats skillStats;
            var skillKey = new KeyValuePair<int, string>(message.SkillId, skillName);
            if (stats.Skills.ContainsKey(skillKey))
            {
                stats.Skills.TryGetValue(skillKey, out skillStats);
            }
            else
            {
                skillStats = new SkillStats(stats.PlayerInfo);
            }

            if (message.IsHeal)
            {
                skillStats.AddData(message.Heal, message.IsCritical, message.IsHeal);
            }
            else
            {
                skillStats.AddData(message.Damage, message.IsCritical, message.IsHeal);
            }

            stats.Skills[skillKey] = skillStats;

            Console.WriteLine("number of skills:" + stats.Skills.Count);
        }
    }
}