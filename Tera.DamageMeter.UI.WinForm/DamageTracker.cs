using System;
using System.Collections;
using System.Collections.Generic;
using Tera.Protocol.Game;

namespace Tera.DamageMeter
{
    public class DamageTracker : IEnumerable<PlayerStats>
    {
        readonly Dictionary<User, PlayerStats> _statsByUser = new Dictionary<User, PlayerStats>();
        private readonly EntityRegistry _entityRegistry;

        public DamageTracker(EntityRegistry entityRegistry)
        {
            _entityRegistry = entityRegistry;
        }

        private PlayerStats GetOrCreate(User user)
        {
            PlayerStats playerStats;
            if (!_statsByUser.TryGetValue(user, out playerStats))
            {
                playerStats = new PlayerStats(user);
                _statsByUser.Add(user, playerStats);
            }

            return playerStats;
        }

        public void Update(EachSkillResultServerMessage message)
        {
            var source = _entityRegistry.Get(message.Source) as User;
            if (source != null)
            {
                var playerStats = GetOrCreate(source);
                UpdateStats(playerStats.Dealt, message);
            }

            var target = _entityRegistry.Get(message.Target) as User;
            if (target != null)
            {
                var playerStats = GetOrCreate(target);
                UpdateStats(playerStats.Received, message);
            }
        }

        private void UpdateStats(Stats stats, EachSkillResultServerMessage message)
        {
            stats.Damage += message.Damage;
            stats.Heal += message.Heal;
            stats.Hits++;
            if (message.IsCritical)
                stats.Crits++;
        }

        public IEnumerator<PlayerStats> GetEnumerator()
        {
            return _statsByUser.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
