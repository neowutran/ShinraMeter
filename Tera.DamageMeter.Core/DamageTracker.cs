using System;
using System.Collections;
using System.Collections.Generic;
using Tera.Game;
using Tera.Game.Messages;

namespace Tera.DamageMeter
{
    public class DamageTracker : IEnumerable<PlayerInfo>
    {
        private readonly Dictionary<Player, PlayerInfo> _statsByUser = new Dictionary<Player, PlayerInfo>();

        public SkillStats TotalDealt { get; private set; }
        public SkillStats TotalReceived { get; private set; }

        public DamageTracker()
        {
            TotalDealt = new SkillStats();
            TotalReceived = new SkillStats();
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

        private void UpdateStats(SkillStats stats, SkillResult message)
        {
            if (message.Amount == 0)
            {
                var str = String.Format("{0:x}", message.SkillId);
                Console.WriteLine("nothing"+ str);
                return;
            }
               


            //Without that, a "death from above" is show as 225 damage points
            if (message.SourcePlayer == message.TargetPlayer && message.Damage != 0)
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
                ";target:" + message.TargetPlayer+
                ";target_id: "+message.Target.Id+
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