using System;
using System.Collections;
using System.Collections.Generic;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.DamageMeter
{
    public class DamageTracker : IEnumerable<PlayerInfo>
    {
        private readonly EntityTracker _entityTracker;
        private readonly PlayerTracker _playerTracker;
        private readonly SkillDatabase _skillDatabase;
        private readonly Dictionary<Player, PlayerInfo> _statsByUser = new Dictionary<Player, PlayerInfo>();

        public DamageTracker(EntityTracker entityRegistry, PlayerTracker playerTracker, SkillDatabase skillDatabase)
        {
            _entityTracker = entityRegistry;
            _skillDatabase = skillDatabase;
            _playerTracker = playerTracker;
        }

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


            //Without that, a "death from above" is show as 225 damage points
            if (message.SourcePlayer == message.TargetPlayer && message.Skill.Name.Contains("Death From Above"))
            {
                return;
            }

            /**
            Lol, debug hard TODO remove when debugging end
            */
            Console.WriteLine("source:" + message.SourcePlayer);
            Console.WriteLine(";source_id:" + message.Source);
            if (message.Skill != null)
            {
                Console.WriteLine(";skill:" + message.Skill.Name);
            }
            Console.WriteLine(
                ";skill_id:" + message.SkillId +
                ";damage:" + message.Damage +
                ";target:" + message.TargetPlayer+
                ";target_id: "+message.Target+
                "; heal:" + message.Heal +
                ";amout:" + message.Amount
                );


            stats.Damage += message.Damage;
            stats.Heal += message.Heal;
            stats.Hits++;
            if (message.IsCritical)
            {
                stats.Crits++;
            }
        }
    }
}