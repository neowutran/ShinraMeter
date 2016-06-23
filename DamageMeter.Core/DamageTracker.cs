using System;
using System.Collections.Generic;
using Tera.Game;
using Data;

namespace DamageMeter
{
    public class DamageTracker
    {
        public delegate void CurrentBossChange(Entity entity);

        private static DamageTracker _instance;


        private DamageTracker()
        {
        }

        public static DamageTracker Instance => _instance ?? (_instance = new DamageTracker());

        private List<Entity> _toDelete = new List<Entity>();

        public void UpdateEntities(NpcOccupierResult npcOccupierResult, long time)
        {
            if (!npcOccupierResult.HasReset)
            {
                return;
            }
      
            var entity = NetworkController.Instance.EntityTracker.GetOrNull(npcOccupierResult.Npc);
            if(entity is NpcEntity)
            {
                var npcEntity = entity as NpcEntity;
                if (npcEntity.Info.Boss)
                {
                    _toDelete.Add(entity);
                }
            }
        }


        public void DeleteEntity(Entity entity)
        {
            if(entity == null)
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
            if (entity.Info.Boss)
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
            if (entity is ProjectileEntity)
            {
                var source = (ProjectileEntity) entity;
                if (source.Owner is NpcEntity || source.Owner is UserEntity)
                {
                    return source.Owner;
                }
            }
            return null;
        }

        public void Update(SkillResult skillResult)
        {

            if (skillResult.Source == null || skillResult.Target == null) return;

            var entitySource = GetEntity(skillResult.Source.Id);
            var entityTarget = GetEntity(skillResult.Target.Id);

            if (skillResult.SourcePlayer == null && skillResult.TargetPlayer == null) return;
            if (!BasicTeraData.Instance.WindowData.PartyOnly || (
                ((skillResult.SourcePlayer == null) ? false : NetworkController.Instance.PlayerTracker.MyParty(skillResult.SourcePlayer)) ||
                ((skillResult.TargetPlayer == null) ? false : NetworkController.Instance.PlayerTracker.MyParty(skillResult.TargetPlayer))))
            {

                if (entityTarget == null)
                {
                    throw new Exception("Unknow target" + skillResult.Target.GetType());
                }

                InsertSkill(entityTarget, entitySource, skillResult, skillResult.Time.Ticks);
            }
        }

        private static bool IsValidAttack(SkillResult message)
        {
            if (message.Amount == 0) // to count buff skills/consumable usage - need additional hitstat for it (damage/heal/mana/uses)
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

        
        private void InsertSkill(Entity entityTarget, Entity entitySource, SkillResult message, long time)
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

            Database.Database.Type skill_type = Database.Database.Type.Mana;

            if (message.IsHp)
            {
                if (message.IsHeal)
                {
                    skill_type = Database.Database.Type.Heal;
                }
                else
                {
                    skill_type = Database.Database.Type.Damage; 
                }

            }

            Database.Database.Instance.Insert(message.Amount, skill_type, entityTarget, entitySource, message.SkillId, message.IsCritical, time);

            if (entityTarget is NpcEntity) {
                UpdateCurrentBoss((NpcEntity)entityTarget);
                return;
            }
        }
    }
}