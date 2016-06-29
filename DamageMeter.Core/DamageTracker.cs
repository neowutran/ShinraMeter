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

        private readonly List<Entity> _toDelete = new List<Entity>();


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
            if (entityTarget == null)
            {
                throw new Exception("Unknow target" + skillResult.Target.GetType());
            }

            InsertSkill(entityTarget, entitySource["root_source"], entitySource["source"], skillResult);
        }

        private static bool IsValidAttack(SkillResult message)
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
            if (!IsValidAttack(message))
            {
                return;
            }

            if (_toDelete.Contains(entityTarget))
            {
                DeleteEntity(entityTarget);
                _toDelete.Remove(entityTarget);
            }

            var skillType = Database.Database.Type.Mana;

            if (message.IsHp)
            {
                skillType = message.IsHeal ? Database.Database.Type.Heal : Database.Database.Type.Damage;
            }

            Database.Database.Instance.Insert(message.Amount, skillType, entityTarget, entitySource, message.SkillId,
                message.Abnormality, message.IsCritical, message.Time.Ticks, petSource);

            var entity = entityTarget as NpcEntity;
            if (entity != null)
            {
                UpdateCurrentBoss(entity);
            }
        }
    }
}