using System;
using System.Collections;
using System.Collections.Generic;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.DamageMeter
{
    public class DamageTracker : IEnumerable<PlayerInfo>
    {
        readonly Dictionary<User, PlayerInfo> _statsByUser = new Dictionary<User, PlayerInfo>();
        private readonly EntityRegistry _entityRegistry;
        private readonly SkillDatabase _skillDatabase;

        public DamageTracker(EntityRegistry entityRegistry, SkillDatabase skillDatabase)
        {
            _entityRegistry = entityRegistry;
            _skillDatabase = skillDatabase;
        }

        private PlayerInfo GetOrCreate(User user)
        {
            PlayerInfo playerStats;
            if (!_statsByUser.TryGetValue(user, out playerStats))
            {
                playerStats = new PlayerInfo(user);
                _statsByUser.Add(user, playerStats);
            }

            return playerStats;
        }

        public void Update(EachSkillResultServerMessage message)
        {
            var skillResult = new SkillResult(message, _entityRegistry, _skillDatabase);
            if (skillResult.SourceUser != null)
            {
                var playerStats = GetOrCreate(skillResult.SourceUser);
                UpdateStats(playerStats.Dealt, skillResult);
            }

            if (skillResult.TargetUser != null)
            {
                var playerStats = GetOrCreate(skillResult.TargetUser);
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
