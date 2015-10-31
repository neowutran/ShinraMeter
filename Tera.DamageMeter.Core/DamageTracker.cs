// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public DateTime? FirstAttack { get; private set; }
        public DateTime? LastAttack { get; private set; }
        public TimeSpan? Duration { get { return LastAttack - FirstAttack; } }

        public SkillStats TotalDealt { get; private set; }
        public SkillStats TotalReceived { get; private set; }

        public void UpdateTotal()
        {

        }

        public DamageTracker(EntityTracker entityRegistry, PlayerTracker playerTracker, SkillDatabase skillDatabase)
        {
            _entityTracker = entityRegistry;
            _skillDatabase = skillDatabase;
            _playerTracker = playerTracker;
            TotalDealt = new SkillStats();
            TotalReceived = new SkillStats();
        }

        private PlayerInfo GetOrCreate(Player player)
        {
            PlayerInfo playerStats;
            if (!_statsByUser.TryGetValue(player, out playerStats))
            {
                playerStats = new PlayerInfo(player, this);
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
                var statsChange = StatsChange(skillResult);
                playerStats.Dealt.Add(statsChange);
                TotalDealt.Add(statsChange);
            }

            if (skillResult.TargetPlayer != null)
            {
                var playerStats = GetOrCreate(skillResult.TargetPlayer);
                var statsChange = StatsChange(skillResult);
                playerStats.Received.Add(statsChange);
                TotalReceived.Add(statsChange);
            }

            if (skillResult.SourcePlayer != null && (skillResult.Damage > 0) && (skillResult.Source.Id != skillResult.Target.Id))
            {
                LastAttack = skillResult.Time;

                if (FirstAttack == null)
                    FirstAttack = skillResult.Time;
            }
        }

        private SkillStats StatsChange(SkillResult message)
        {
            var result = new SkillStats();
            if (message.Amount == 0)
                return result;

            result.Damage = message.Damage;
            result.Heal = message.Heal;
            result.Hits++;
            if (message.IsCritical)
                result.Crits++;

            return result;
        }

        public IEnumerator<PlayerInfo> GetEnumerator()
        {
            return _statsByUser.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public long? Dps(long damage)
        {
            return Dps(damage, Duration);
        }

        public static long? Dps(long damage, TimeSpan? duration)
        {
            var durationInSeconds = (duration ?? TimeSpan.Zero).TotalSeconds;
            if (durationInSeconds < 1)
                durationInSeconds = 1;
            var dps = damage / durationInSeconds;
            if (Math.Abs(dps) > long.MaxValue)
                return null;
            return (long)dps;
        }
    }
}
