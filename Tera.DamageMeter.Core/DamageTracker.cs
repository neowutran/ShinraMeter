using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class DamageTracker : IEnumerable<PlayerInfo>
    {
        private static DamageTracker _instance;
        private ConcurrentDictionary<Player, PlayerInfo> _statsByUser = new ConcurrentDictionary<Player, PlayerInfo>();
        public ObservableCollection<Entity> Entities = new ObservableCollection<Entity>();

        private DamageTracker()
        {
        }

        public ConcurrentDictionary<Entity, long> TotalDamageEntity { get; set; } =
            new ConcurrentDictionary<Entity, long>();


        public long TotalDamage
        {
            get
            {
                lock (TotalDamageEntity)
                {
                    if (UiModel.Instance.Encounter == null)
                    {
                        return TotalDamageEntity.Sum(totalDamageEntity => totalDamageEntity.Value);
                    }
                    return TotalDamageEntity[UiModel.Instance.Encounter];
                }
            }
        }


        private long FirstHit
        {
            get
            {
                lock (_statsByUser)
                {
                    long firsthit = 0;
                    foreach (var userstat in _statsByUser)
                    {
                        if (((firsthit == 0) || (userstat.Value.Dealt.FirstHit < firsthit)) &&
                            userstat.Value.Dealt.FirstHit != 0)
                        {
                            firsthit = userstat.Value.Dealt.FirstHit;
                        }
                    }
                    return firsthit;
                }
            }
        }

        public long Interval => LastHit - FirstHit;

        private long LastHit
        {
            get
            {
                lock (_statsByUser)
                {
                    long lasthit = 0;
                    foreach (var userstat in _statsByUser)
                    {
                        if (((lasthit == 0) || (userstat.Value.Dealt.LastHit > lasthit)) &&
                            userstat.Value.Dealt.LastHit != 0)
                        {
                            lasthit = userstat.Value.Dealt.LastHit;
                        }
                    }
                    return lasthit;
                }
            }
        }

        public Dispatcher Dispatcher { get; set; } = null;

        public static DamageTracker Instance => _instance ?? (_instance = new DamageTracker());


        public IEnumerator<PlayerInfo> GetEnumerator()
        {
            return _statsByUser.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Reset()
        {
            _statsByUser = new ConcurrentDictionary<Player, PlayerInfo>();
            Entities = new ObservableCollection<Entity>();
        }

        private PlayerInfo GetOrCreate(Player player)
        {
            PlayerInfo playerStats;
            if (_statsByUser.TryGetValue(player, out playerStats)) return playerStats;
            playerStats = new PlayerInfo(player);
            _statsByUser.TryAdd(player, playerStats);

            return playerStats;
        }

        public void Update(SkillResult skillResult)
        {
            if (skillResult.SourcePlayer != null)
            {
                var playerStats = GetOrCreate(skillResult.SourcePlayer);

                Entity entityTarget;
                if (skillResult.Target is NpcEntity)
                {
                    var target = (NpcEntity) skillResult.Target;
                    entityTarget = new Entity(target.CategoryId, target.Id, target.NpcId, target.NpcArea);
                }
                else if (skillResult.Target is UserEntity)
                {
                    var target = (UserEntity) skillResult.Target;
                    entityTarget = new Entity(target.Name);
                }
                else
                {
                    throw new Exception("Unknow target" + skillResult.Target.GetType());
                }


                UpdateStatsDealt(playerStats, skillResult, entityTarget);
            }

            if (skillResult.TargetPlayer != null)
            {
                var playerStats = GetOrCreate(skillResult.TargetPlayer);
                UpdateStatsReceived(playerStats.Received, skillResult);
            }
        }

        private static bool IsValidAttack(SkillResult message)
        {
            if (message.Amount == 0)
            {
                return false;
            }

            if ((UserEntity.ForEntity(message.Source) == UserEntity.ForEntity(message.Target)) && (message.Damage > 0))
            {
                return false;
            }

            return true;
        }

        private void UpdateEncounter(Entity entityTarget, SkillResult message)
        {
            ChangedEncounter changeEncounter = delegate(Entity entity, SkillResult msg)
            {
                if (!Entities.Contains(entity) && !msg.IsHeal)
                {
                    Entities.Add(entity);
                }
            };
            Dispatcher.Invoke(changeEncounter, entityTarget, message);
        }

        private void UpdateStatsDealt(PlayerInfo playerInfo, SkillResult message, Entity entityTarget)
        {
            if (!IsValidAttack(message))
            {
                return;
            }

            UpdateEncounter(entityTarget, message);
            var entities = playerInfo.Dealt;

            if (!entities.EntitiesStats.ContainsKey(entityTarget))
            {
                entities.EntitiesStats[entityTarget] = new SkillsStats(playerInfo);
            }


            var skillName = message.SkillId.ToString();
            if (message.Skill != null)
            {
                skillName = message.Skill.Name;
            }
            var skillKey = new Skill(skillName, new List<int> {message.SkillId});

            lock (entities.EntitiesStats[entityTarget])
            {
                SkillStats skillStats;
                entities.EntitiesStats[entityTarget].Skills.TryGetValue(skillKey, out skillStats);
                if (skillStats == null)
                {
                    skillStats = new SkillStats(playerInfo, entityTarget);
                }
                if (message.IsHeal)
                {
                    skillStats.AddData(message.Heal, message.IsCritical, SkillStats.Type.Heal);


                }
                else if (message.IsMana)
                {
                    skillStats.AddData(message.Mana, message.IsCritical, SkillStats.Type.Mana);

                }
                else
                {
                    skillStats.AddData(message.Damage, message.IsCritical, SkillStats.Type.Damage);

                }

                var skill = entities.EntitiesStats[entityTarget].Skills.Keys.ToList();
                var indexSkill = skill.IndexOf(skillKey);
                if (indexSkill != -1)
                {
                    foreach (
                        var skillid in skill[indexSkill].SkillId.Where(skillid => !skillKey.SkillId.Contains(skillid)))
                    {
                        skillKey.SkillId.Add(skillid);
                    }
                }
                SkillStats trash;
                entities.EntitiesStats[entityTarget].Skills.TryRemove(skillKey, out trash);
                entities.EntitiesStats[entityTarget].Skills.TryAdd(skillKey, skillStats);
            }
        }

        private void UpdateStatsReceived(SkillsStats stats, SkillResult message)
        {
            if (message.Amount == 0)
            {
                return;
            }

            if ((UserEntity.ForEntity(message.Source) == UserEntity.ForEntity(message.Target)) && (message.Damage > 0))
            {
                return;
            }

            var skillName = message.SkillId + "";
            if (message.Skill != null)
            {
                skillName = message.Skill.Name;
            }
            SkillStats skillStats;
            var skillKey = new Skill(skillName, new List<int> {message.SkillId});

            stats.Skills.TryGetValue(skillKey, out skillStats);
            if (skillStats == null)
            {
                skillStats = new SkillStats(stats.PlayerInfo);
            }

            if (message.IsHeal)
            {
                skillStats.AddData(message.Heal, message.IsCritical, SkillStats.Type.Heal);


            }
            else if (message.IsMana)
            {
                skillStats.AddData(message.Mana, message.IsCritical, SkillStats.Type.Mana);

            }
            else
            {
                skillStats.AddData(message.Damage, message.IsCritical, SkillStats.Type.Damage);

            }
            var skill = stats.Skills.Keys.ToList();
            var indexSkill = skill.IndexOf(skillKey);
            if (indexSkill != -1)
            {
                foreach (var skillid in skill[indexSkill].SkillId.Where(skillid => !skillKey.SkillId.Contains(skillid)))
                {
                    skillKey.SkillId.Add(skillid);
                }
            }
            SkillStats trash;
            stats.Skills.TryRemove(skillKey, out trash);
            stats.Skills.TryAdd(skillKey, skillStats);
        }

        private delegate void ChangedEncounter(Entity entity, SkillResult msg);
    }
}