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
        private Dictionary<Player, PlayerInfo> _usersStats = new Dictionary<Player, PlayerInfo>();
        public Dictionary<Entity, EntityInfo> EntitiesStats = new Dictionary<Entity, EntityInfo>();


        private DamageTracker()
        {
        }

        public long TotalDamage
        {
            get
            {
                if (NetworkController.Instance.Encounter == null)
                {
                    return
                        EntitiesStats.Select(entityStats => entityStats.Value).Select(stats => stats.TotalDamage).Sum();
                }
                if (!EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                {
                    return 0;
                }
                return EntitiesStats[NetworkController.Instance.Encounter].TotalDamage;
            }
        }

        private long FirstHit
        {
            get
            {
                long firsthit = 0;
                foreach (var userstat in _usersStats)
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
                foreach (var userstat in _usersStats)
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


        public Dictionary<Entity, EntityInfo> GetEntityStats()
        {
            return EntitiesStats.ToDictionary(stats => stats.Key, stats => (EntityInfo) stats.Value.Clone());
        }

        public List<PlayerInfo> GetPlayerStats()
        {
            return _usersStats.Select(stat => (PlayerInfo) stat.Value.Clone()).ToList();
        }


        public void UpdateEntities(NpcOccupierResult npcOccupierResult, long time)
        {
            /*
            var npcEntity =
                (NpcEntity) NetworkController.Instance.EntityTracker.GetOrPlaceholder(npcOccupierResult.Npc);
            var entityNpc = new Entity(npcEntity.CategoryId, npcEntity.Id, npcEntity.NpcId, npcEntity.NpcArea);
            if (!EntitiesStats.ContainsKey(entityNpc) && !npcOccupierResult.HasReset)
            {
                EntitiesStats.Add(entityNpc, new EntityInfo());
                EntitiesStats[entityNpc].FirstHit = time/ 10000000;
                return;
            }*/


            foreach (var entityStats in EntitiesStats)
            {
                var entity = entityStats.Key;
                if (entity.Id != npcOccupierResult.Npc || !entity.IsBoss() || !npcOccupierResult.HasReset) continue;
                DeleteEntity(entity);
                return;
            }
        }


        public void DeleteEntity(Entity entity)
        {
            if (NetworkController.Instance.Encounter == entity)
            {
                NetworkController.Instance.Encounter = null;
            }
            EntitiesStats.Remove(entity);
            foreach (var stats in _usersStats)
            {
                stats.Value.Dealt.RemoveEntity(entity);
                stats.Value.Received.RemoveEntity(entity);
            }
        }

        public event CurrentBossChange CurrentBossUpdated;

        public void UpdateCurrentBoss(Entity entity)
        {
            var handler = CurrentBossUpdated;
            handler?.Invoke(entity);
        }

        public void SetFirstHit(Entity entity, long time)
        {
            if (EntitiesStats[entity].FirstHit == 0)
            {
                EntitiesStats[entity].FirstHit = time;
            }
        }

        public void SetLastHit(Entity entity, long time)
        {
            EntitiesStats[entity].LastHit = time;
        }

        public long GetInterval(Entity entity)
        {
            return EntitiesStats[entity].LastHit - EntitiesStats[entity].FirstHit;
        }

        public void Reset()
        {
            _usersStats = new Dictionary<Player, PlayerInfo>();
            EntitiesStats = new Dictionary<Entity, EntityInfo>();
        }


        private PlayerInfo GetOrCreate(Player player)
        {
            PlayerInfo playerStats;

            if (_usersStats.TryGetValue(player, out playerStats))
            {
                return playerStats;
            }

            playerStats = new PlayerInfo(player);
            _usersStats.Add(player, playerStats);


            return playerStats;
        }

        public void Update(SkillResult skillResult, long time)
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

                UpdateStatsDealt(playerStats, skillResult, entityTarget, time);
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
                    Console.WriteLine("UNKNOW DAMAGE:" + source.Owner.GetType());
                    entitySource = new Entity("UNKNOW");
                }
            }
            else
            {
                entitySource = new Entity("UNKNOW");
            }

            UpdateStatsReceived(playerStats, skillResult, entitySource, time);
        }

        private static bool IsValidAttack(SkillResult message)
        {
            if (message.Amount == 0)
            {
                return false;
            }

            if ((UserEntity.ForEntity(message.Source)["user"] == UserEntity.ForEntity(message.Target)["user"]) &&
                (message.Damage != 0))
            {
                return false;
            }

            return true;
        }

        private void UpdateEncounter(Entity entity, SkillResult msg)
        {
            if (!EntitiesStats.ContainsKey(entity) && msg.Damage != 0)
            {
                EntitiesStats.Add(entity, new EntityInfo());
            }
        }

        private void UpdateStatsDealt(PlayerInfo playerInfo, SkillResult message, Entity entityTarget, long time)
        {
            if (!IsValidAttack(message))
            {
                return;
            }

            UpdateEncounter(entityTarget, message);
            UpdateSkillStats(message, entityTarget, playerInfo, time);
        }


        private static void UpdateSkillStats(SkillResult message, Entity entityTarget, PlayerInfo playerInfo, long time)
        {
            var entities = playerInfo.Dealt;

            entities.AddStats(time, entityTarget, new SkillsStats(playerInfo));

            var skillName = message.SkillId.ToString();
            if (message.Skill != null)
            {
                skillName = message.Skill.Name;
            }
            var skillKey = new Skill(skillName, new List<int> {message.SkillId});


            SkillStats skillStats;
            var dictionarySkillStats = entities.GetSkills(time, entityTarget);
            dictionarySkillStats.TryGetValue(skillKey, out skillStats);
            if (skillStats == null)
            {
                skillStats = new SkillStats(playerInfo, entityTarget);
            }
            if (message.IsHp)
            {
                if (message.Amount > 0)
                {
                    skillStats.AddData(message.SkillId, message.Heal, message.IsCritical, SkillStats.Type.Heal, time);
                }
                else
                {
                    skillStats.AddData(message.SkillId, message.Damage, message.IsCritical, SkillStats.Type.Damage, time);
                }
            }
            else
            {
                skillStats.AddData(message.SkillId, message.Mana, message.IsCritical, SkillStats.Type.Mana, time);
            }

            var skill = dictionarySkillStats.Keys.ToList();
            var indexSkill = skill.IndexOf(skillKey);
            if (indexSkill != -1)
            {
                foreach (
                    var skillid in skill[indexSkill].SkillId.Where(skillid => !skillKey.SkillId.Contains(skillid)))
                {
                    skillKey.SkillId.Add(skillid);
                }
            }
            dictionarySkillStats.Remove(skillKey);
            dictionarySkillStats.Add(skillKey, skillStats);
        }

        private void UpdateStatsReceived(PlayerInfo playerInfo, SkillResult message, Entity entitySource, long time)
        {
            if (!IsValidAttack(message))
            {
                return;
            }

            //Not damage & if you are a healer, don't show heal / mana regen affecting you, as that will modify your crit rate and other stats. 
            if ((message.IsHp && message.Amount > 0 || !message.IsHp) && !PlayerClassHelper.IsHeal(playerInfo.Class) &&
                (UserEntity.ForEntity(message.Source)["user"] != UserEntity.ForEntity(message.Target)["user"]))
            {
                UpdateSkillStats(message, new Entity(playerInfo.Player.User.Name), playerInfo, time);
                return;
            }

            if (message.Damage <= 0)
            {
                return;
            }

            var entity = entitySource;

            if (entitySource.IsBoss())
            {
                foreach (var t in EntitiesStats.Where(t => t.Key.Name == entitySource.Name && t.Key.Id == entitySource.Id))
                {
                    entity = t.Key;
                    break;
                }

                //Don't count damage taken if boss haven't taken any damage
                if (entity == null)
                {
                    return;
                }
            }

            playerInfo.Received.AddStats(time, entity, message.Damage);
            if (EntitiesStats.ContainsKey(entity)) return;
            EntitiesStats.Add(entity, new EntityInfo());
        }
    }
}