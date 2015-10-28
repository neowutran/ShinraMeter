using System;
using System.Collections;
using System.Collections.Generic;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.DamageMeter
{
    public class DamageTracker : IEnumerable<PlayerInfo>
    {
        readonly Dictionary<Player, PlayerInfo> _statsByUser = new Dictionary<Player, PlayerInfo>();
        private readonly EntityTracker _entityTracker;
        private readonly PlayerTracker _playerTracker;
        private readonly SkillDatabase _skillDatabase;

        public DamageTracker(EntityTracker entityRegistry, PlayerTracker playerTracker, SkillDatabase skillDatabase)
        {
            _entityTracker = entityRegistry;
            _skillDatabase = skillDatabase;
            _playerTracker = playerTracker;
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

        public void Update(EachSkillResultServerMessage message)
        {
            var skillResult = new SkillResult(message, _entityTracker, _playerTracker, _skillDatabase);
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

        private void UpdateStats(SkillStats stats, SkillResult message)
        {
            if (message.Amount == 0)
                return;

            stats.Damage += message.Damage;
            stats.Heal += message.Heal;
            stats.Hits++;
            if (message.IsCritical)
                stats.Crits++;
        }

        public IEnumerator<PlayerInfo> GetEnumerator()
        {
            return _statsByUser.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
