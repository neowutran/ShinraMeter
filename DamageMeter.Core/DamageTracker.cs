using System;
using System.Collections.Generic;
using Data;
using Tera.Game;

namespace DamageMeter
{
    public class DamageTracker
    {
        public delegate void CurrentBossChange(Entity entity);

        private static DamageTracker _instance;

        private List<Entity> _toDelete = new List<Entity>();
        private List<EntityId> _unknownEntityIds = new List<EntityId>();

        private DamageTracker()
        {
        }

        public static DamageTracker Instance => _instance ?? (_instance = new DamageTracker());

        public void UpdateEntities(NpcOccupierResult npcOccupierResult, long time)
        {
            if (!npcOccupierResult.HasReset)
            {
                return;
            }

            var entity = NetworkController.Instance.EntityTracker.GetOrNull(npcOccupierResult.Npc);
            if (!(entity is NpcEntity)) return;
            var npcEntity = entity as NpcEntity;
            if (npcEntity.Info.Boss)
            {
                _toDelete.Add(entity);
            }

        }


        public void DeleteEntity(Entity entity)
        {
            if (entity == null)
            {
                return;
            }
            if (NetworkController.Instance.Encounter == entity)
            {
                NetworkController.Instance.NewEncounter = null;
            }

            Database.Database.Instance.DeleteEntity(entity);
        }

        public void UpdateCurrentBoss(NpcEntity entity)
        {
            if (!entity.Info.Boss) return;
            if (NetworkController.Instance.Encounter != entity)
            {
                NetworkController.Instance.NewEncounter = entity;
            }
        }

        public void Reset()
        {
            Database.Database.Instance.DeleteAll();
            NetworkController.Instance.NewEncounter = null;
        }

        public Entity GetActorEntity(EntityId entityId)
        {
            var entity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(entityId);
            if (entity is NpcEntity || entity is UserEntity)
            {
                return entity;
            }

            return null;
        }

        public Entity GetEntity(EntityId entityId)
        {
            
            var entity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(entityId);
            var entity2 = GetActorEntity(entityId);
            if (entity2 != null)
            {
                return entity2;
            }
            if (!(entity is ProjectileEntity)) return null;
            var source = (ProjectileEntity) entity;
            if (!(source.Owner is NpcEntity) && !(source.Owner is UserEntity)) return null;
            return source.Owner;
        }

        public void Update(SkillResult skillResult)
        {
            if (skillResult.Source == null || skillResult.Target == null) return;

            var entitySource = UserEntity.ForEntity(NetworkController.Instance.EntityTracker.GetOrPlaceholder(skillResult.Source.Id));
            var entityTarget = GetActorEntity(skillResult.Target.Id);

            if (skillResult.SourcePlayer == null && skillResult.TargetPlayer == null) return;
            if (BasicTeraData.Instance.WindowData.PartyOnly &&
                (skillResult.SourcePlayer == null ||
                 !NetworkController.Instance.PlayerTracker.MyParty(skillResult.SourcePlayer)) &&
                (skillResult.TargetPlayer == null ||
                 !NetworkController.Instance.PlayerTracker.MyParty(skillResult.TargetPlayer))) return;
            if (BasicTeraData.Instance.WindowData.OnlyBoss && !(((entityTarget as NpcEntity)?.Info.Boss ?? false) ||
                                                                (skillResult.SourcePlayer != null && skillResult.TargetPlayer != null) ||
                                                                ((entitySource["root_source"] as NpcEntity)?.Info.Boss ?? false))) return;
            if (entitySource["root_source"] is PlaceHolderEntity)
            {
                if (_unknownEntityIds.Contains(entitySource["root_source"].Id))
                    return;
                _unknownEntityIds.Add(entitySource["root_source"].Id);
                BasicTeraData.LogError("Unknow source " + _unknownEntityIds.Count + ", skill = " + skillResult.SkillId + ";" + skillResult.SkillName + ";" + skillResult.Skill.Id, false, true);
            }
            if(entityTarget == null)
            {
                if (_unknownEntityIds.Contains(skillResult.Target.Id))
                    return;
                _unknownEntityIds.Add(skillResult.Target.Id);
                BasicTeraData.LogError("Unknow target " + _unknownEntityIds.Count + ", skill = " + skillResult.SkillId + ";" + skillResult.SkillName + ";" + skillResult.Skill.Id, false, true);
                return;
            }

            InsertSkill(entityTarget, entitySource["root_source"], entitySource["source"], skillResult);
        }

        private static bool IsValidSkill(SkillResult message)
        {
            if (message.Amount == 0)
                // to count buff skills/consumable usage - need additional hitstat for it (damage/heal/mana/uses)
            {
                return false;
            }

            return (UserEntity.ForEntity(message.Source)["root_source"] != UserEntity.ForEntity(message.Target)["root_source"]) || (message.Damage == 0);
        }


        private void InsertSkill(Entity entityTarget, Entity entitySource, Entity petSource, SkillResult message)
        {
            if (!IsValidSkill(message))
            {
                return;
            }

            var skillType = Database.Database.Type.Mana;
            if (message.IsHp)
            {
                skillType = message.IsHeal ? Database.Database.Type.Heal : Database.Database.Type.Damage;
            }
   
            var entity = entityTarget as NpcEntity;
            if (entity != null)
            {

                /*
                 * Remove data from resetted boss when hitting a new boss
                 * (we don't remove the data directly when the boss reset, to let the time for a review of the encounter.)
                 */
                if (skillType == Database.Database.Type.Damage && entity.Info.Boss)
                {
                    foreach (var delete in _toDelete)
                    {
                        DeleteEntity(delete);
                    }
                    _toDelete = new List<Entity>();
                }

                UpdateCurrentBoss(entity);

            }
            
            Database.Database.Instance.Insert(message.Amount, skillType, entityTarget, entitySource, message.SkillId,
               message.Abnormality, message.IsCritical, message.Time.Ticks, petSource, message.HitDirection);

        }
    }
}