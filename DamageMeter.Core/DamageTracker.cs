using System;
using System.Collections.Generic;
using System.Linq;
using DamageMeter.Skills;
using DamageMeter.Skills.Skill;
using DamageMeter.Taken;
using Tera.Game;
using Skill = DamageMeter.Skills.Skill.Skill;

namespace DamageMeter
{
    public class DamageTracker
    {
        public delegate void CurrentBossChange(Entity entity);

        private static DamageTracker _instance;
        private Dictionary<Player, PlayerInfo> _statsByUser = new Dictionary<Player, PlayerInfo>();
        public LinkedList<Entity> Entities = new LinkedList<Entity>();


        private DamageTracker()
        {
        }

        public Dictionary<Entity, long> TotalDamageEntity { get; set; } =
            new Dictionary<Entity, long>();

        private Dictionary<Entity, long> EntitiesFirstHit { get; } = new Dictionary<Entity, long>()
            ;

        private Dictionary<Entity, long> EntitiesLastHit { get; } = new Dictionary<Entity, long>();

        public long TotalDamage
        {
            get
            {
                if (NetworkController.Instance.Encounter == null)
                {
                    return TotalDamageEntity.Sum(totalDamageEntity => totalDamageEntity.Value);
                }
                if (!TotalDamageEntity.ContainsKey(NetworkController.Instance.Encounter))
                {
                    return 0;
                }
                return TotalDamageEntity[NetworkController.Instance.Encounter];
            }
        }

        private long FirstHit
        {
            get
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

        public long Interval => LastHit - FirstHit;

        private long LastHit
        {
            get
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

        public static DamageTracker Instance => _instance ?? (_instance = new DamageTracker());


        public List<PlayerInfo> GetStats()
        {
            return _statsByUser.Select(stat => (PlayerInfo) stat.Value.Clone()).ToList();
        }


        public void UpdateEntities(NpcOccupierResult npcOccupierResult)
        {
            foreach (var entity in Entities)
            {
                if (entity.Id != npcOccupierResult.Npc || !entity.IsBoss() || !npcOccupierResult.HasReset) continue;
                DeleteEntity(entity);
                return;
            }
        }


        private void DeleteEntity(Entity entity)
        {
            if (NetworkController.Instance.Encounter == entity)
            {
                NetworkController.Instance.Encounter = null;
            }
            Instance.Entities.Remove(entity);
            Instance.TotalDamageEntity.Remove(entity);
            Instance.EntitiesFirstHit.Remove(entity);
            Instance.EntitiesLastHit.Remove(entity);
            foreach (var stats in _statsByUser)
            {
                stats.Value.Dealt.EntitiesStats.Remove(entity);
                stats.Value.Received.EntitiesStats.Remove(entity);
            }
        }

        public event CurrentBossChange CurrentBossUpdated;

        public void UpdateCurrentBoss(Entity entity)
        {
            var handler = CurrentBossUpdated;
            handler?.Invoke(entity);
        }

        public void SetFirstHit(Entity entity)
        {
            if (!EntitiesFirstHit.ContainsKey(entity))
            {
                EntitiesFirstHit[entity] = DateTime.UtcNow.Ticks/10000000;
            }
        }

        public void SetLastHit(Entity entity)
        {
            EntitiesLastHit[entity] = DateTime.UtcNow.Ticks/10000000;
        }

        public long GetInterval(Entity entity)
        {
            return EntitiesLastHit[entity] - EntitiesFirstHit[entity];
        }

        public void Reset()
        {
            _statsByUser = new Dictionary<Player, PlayerInfo>();

            Entities = new LinkedList<Entity>();

            TotalDamageEntity = new Dictionary<Entity, long>();
        }


        private PlayerInfo GetOrCreate(Player player)
        {
            PlayerInfo playerStats;

            if (_statsByUser.TryGetValue(player, out playerStats))
            {
                return playerStats;
            }

            playerStats = new PlayerInfo(player);
            _statsByUser.Add(player, playerStats);


            return playerStats;
        }

        public void Update(SkillResult skillResult)
        {
            PlayerInfo playerStats;
            if (skillResult.SourcePlayer != null)
            {
                playerStats = GetOrCreate(skillResult.SourcePlayer);

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

            if (skillResult.TargetPlayer == null) return;
            playerStats = GetOrCreate(skillResult.TargetPlayer);
            Entity entitySource;
            if (skillResult.Source is NpcEntity)
            {
                var source = (NpcEntity) skillResult.Source;
                entitySource = new Entity(source.CategoryId, source.Id, source.NpcId, source.NpcArea);
            }
            else if (skillResult.Source is UserEntity)
            {
                var source = (UserEntity) skillResult.Source;
                entitySource = new Entity(source.Name);
            }
            else if (skillResult.Source is ProjectileEntity)
            {
                var source = (ProjectileEntity) skillResult.Source;
                if (source.Owner is NpcEntity)
                {
                    var source2 = (NpcEntity) source.Owner;
                    entitySource = new Entity(source2.CategoryId, source.Id, source2.NpcId, source2.NpcArea);
                }
                else if (source.Owner is UserEntity)
                {
                    var source2 = (UserEntity) source.Owner;
                    entitySource = new Entity(source2.Name);
                }
                else
                {
                    Console.WriteLine("UNKNOW DAMAGE");
                    entitySource = new Entity("UNKNOW");
                }
            }
            else
            {
                Console.WriteLine("UNKNOW DAMAGE");
                entitySource = new Entity("UNKNOW");
            }

            UpdateStatsReceived(playerStats, skillResult, entitySource);
        }

        private static bool IsValidAttack(SkillResult message)
        {
            if (message.Amount == 0)
            {
                return false;
            }

            if ((UserEntity.ForEntity(message.Source)["user"] == UserEntity.ForEntity(message.Target)["user"]) && (message.Damage > 0))
            {
                return false;
            }

            return true;
        }

        private void UpdateEncounter(Entity entity, SkillResult msg)
        {
            if (!Entities.Contains(entity) && !msg.IsHeal && !msg.IsMana)
            {
                Entities.AddFirst(entity);
            }
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


            SkillStats skillStats;
            entities.EntitiesStats[entityTarget].Skills.TryGetValue(skillKey, out skillStats);
            if (skillStats == null)
            {
                skillStats = new SkillStats(playerInfo, entityTarget);
            }
            if (message.IsHeal)
            {
                skillStats.AddData(message.SkillId, message.Heal, message.IsCritical, SkillStats.Type.Heal);
            }
            else if (message.IsMana)
            {
                skillStats.AddData(message.SkillId, message.Mana, message.IsCritical, SkillStats.Type.Mana);
            }
            else
            {
                skillStats.AddData(message.SkillId, message.Damage, message.IsCritical, SkillStats.Type.Damage);
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
            entities.EntitiesStats[entityTarget].Skills.Remove(skillKey);
            entities.EntitiesStats[entityTarget].Skills.Add(skillKey, skillStats);
        }

        private void UpdateStatsReceived(PlayerInfo playerInfo, SkillResult message, Entity entitySource)
        {
         

            if (message.Damage == 0)
            {
                return;
            }

            if ((UserEntity.ForEntity(message.Target)["user"] == UserEntity.ForEntity(message.Source)["user"]))
            {
                return;
            }

            var entity = entitySource;

            if (entitySource.IsBoss())
            {
                foreach (var t in Entities.Where(t => t.Name == entitySource.Name))
                {
                    entity = t;
                    break;
                }

                //Don't count damage taken if boss haven't taken any damage
                if (entity == null)
                {
                    return;
                }
            }


            if (!playerInfo.Received.EntitiesStats.ContainsKey(entity))
            {
                playerInfo.Received.EntitiesStats[entity] = new DamageTaken();
            }

            playerInfo.Received.EntitiesStats[entity].AddDamage(message.Damage);
            if (Instance.Entities.Contains(entity)) return;
            Instance.Entities.AddFirst(entity);
            TotalDamageEntity.Add(entity, 0);
        }
    }
}