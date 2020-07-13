using System.Collections.Generic;
using Data;
using Tera.Game;
using Tera.Game.Messages;
using System.Diagnostics;
using DamageMeter.Processing;
using Tera.RichPresence;

namespace DamageMeter
{
    public class DamageTracker
    {
        public delegate void CurrentBossChange(Entity entity);

        private static DamageTracker _instance;

        private List<Entity> _toDelete = new List<Entity>();

        public long LastIdleStartTime;

        private DamageTracker() { }

        public static DamageTracker Instance => _instance ?? (_instance = new DamageTracker());

        public void UpdateEntities(SNpcOccupierInfo npcOccupierResult)
        {
            PacketProcessor.Instance.AbnormalityTracker.Update(npcOccupierResult);
            RichPresence.Instance.HadleNpcOccupierInfo(npcOccupierResult);
            if (!npcOccupierResult.HasReset) {
                //Debug.WriteLine("S_NPC_OCCUPIER_INFO: NPC = " + npcOccupierResult.NPC + "; target = " + npcOccupierResult.Target + " ; Engager = "+npcOccupierResult.Engager);
                return;
            }

            var entity = PacketProcessor.Instance.EntityTracker.GetOrNull(npcOccupierResult.NPC);
            if (!(entity is NpcEntity)) { return; }
            var npcEntity = entity as NpcEntity;
            if (npcEntity.Info.Boss) { _toDelete.Add(entity); }
        }

        public void UpdateEntities(SpawnNpcServerMessage message)
        {
            var entity = PacketProcessor.Instance.EntityTracker.GetOrNull(message.Id);
            if (entity is NpcEntity npcEntity && PacketProcessor.Instance.Encounter == npcEntity) { _toDelete.Add(entity); }
        }

        public void DeleteEntity(Entity entity)
        {
            if (entity == null) { return; }
            if (PacketProcessor.Instance.Encounter == entity) { PacketProcessor.Instance.NewEncounter = null; }

            Database.Database.Instance.DeleteEntity(entity);
        }

        public void UpdateCurrentBoss(NpcEntity entity)
        {
            if (!entity.Info.Boss) { return; }
            if (PacketProcessor.Instance.Encounter != entity) {
                PacketProcessor.Instance.NewEncounter = entity;
                if (BasicTeraData.Instance.WindowData.EnableChat) {
                    HudManager.Instance.AddBoss(entity);
                    NotifyProcessor.Instance.AddBoss(entity.Id);
                }
            }
        }

        public void Reset()
        {
            Database.Database.Instance.DeleteAll();
            PacketProcessor.Instance.NewEncounter = null;
            LastIdleStartTime = 0;
        }

        public Entity GetActorEntity(EntityId entityId)
        {
            var entity = PacketProcessor.Instance.EntityTracker.GetOrPlaceholder(entityId);
            if (entity is NpcEntity || entity is UserEntity) { return entity; }

            return null;
        }

        public Entity GetEntity(EntityId entityId)
        {
            var entity = PacketProcessor.Instance.EntityTracker.GetOrPlaceholder(entityId);
            var entity2 = GetActorEntity(entityId);
            if (entity2 != null) { return entity2; }
            if (!(entity is ProjectileEntity)) { return null; }
            var source = (ProjectileEntity) entity;
            if (!(source.Owner is NpcEntity) && !(source.Owner is UserEntity)) { return null; }
            return source.Owner;
        }

        public void Update(SkillResult skillResult)
        {
            if (skillResult.Source == null || skillResult.Target == null) { return; }

            var entitySource = UserEntity.ForEntity(PacketProcessor.Instance.EntityTracker.GetOrPlaceholder(skillResult.Source.Id));
            var entityTarget = GetActorEntity(skillResult.Target.Id);

            if (skillResult.SourcePlayer == null && skillResult.TargetPlayer == null) { return; }
            if (BasicTeraData.Instance.WindowData.PartyOnly &&
                (skillResult.SourcePlayer == null || !(PacketProcessor.Instance.PlayerTracker.MyParty(skillResult.SourcePlayer) ||
                                                       (entityTarget as NpcEntity)?.Info.HuntingZoneId == 950)) &&
                (skillResult.TargetPlayer == null || !PacketProcessor.Instance.PlayerTracker.MyParty(skillResult.TargetPlayer))) { return; }
            if (BasicTeraData.Instance.WindowData.OnlyBoss && !(((entityTarget as NpcEntity)?.Info.Boss ?? false) ||
                                                                skillResult.SourcePlayer != null && skillResult.TargetPlayer != null ||
                                                                ((entitySource["root_source"] as NpcEntity)?.Info.Boss ?? false))) { return; }
            if (entitySource["root_source"] is PlaceHolderEntity || entityTarget == null) { return; }
            InsertSkill(entityTarget, entitySource["root_source"], entitySource["source"], skillResult);
        }

        private static bool IsValidSkill(SkillResult message)
        {
            return UserEntity.ForEntity(message.Source)["root_source"] != UserEntity.ForEntity(message.Target)["root_source"] || message.Damage == 0;
        }


        private void InsertSkill(Entity entityTarget, Entity entitySource, Entity petSource, SkillResult message)
        {

            var skillType = Database.Database.Type.Mana;
            if (message.IsHp) { skillType = message.IsHeal ? Database.Database.Type.Heal : Database.Database.Type.Damage; }
            if (message.Amount == 0) { return; skillType = Database.Database.Type.Counter; }
            if (!IsValidSkill(message)) { return; skillType = Database.Database.Type.Counter; }// count DFA etc

            if (entityTarget is NpcEntity entity)
            {
                /*
                 * Remove data from resetted boss when hitting a new boss
                 * (we don't remove the data directly when the boss reset, to let the time for a review of the encounter.)
                 */
                if (LastIdleStartTime > 0 && message.Time.AddSeconds(-BasicTeraData.Instance.WindowData.IdleResetTimeout).Ticks >= LastIdleStartTime) { Reset(); }
                else { LastIdleStartTime = 0; }
                if (skillType == Database.Database.Type.Damage && entity.Info.Boss)
                {
                    foreach (var delete in _toDelete) { DeleteEntity(delete); }
                    _toDelete = new List<Entity>();
                }

                if ((BasicTeraData.Instance.WindowData.DisplayOnlyBossHitByMeterUser && PacketProcessor.Instance.EntityTracker.MeterUser.Id == entitySource.Id) || !BasicTeraData.Instance.WindowData.DisplayOnlyBossHitByMeterUser)
                {
                    UpdateCurrentBoss(entity);
                }
            }

            Database.Database.Instance.Insert(message.Amount, skillType, entityTarget, entitySource, message.SkillId, message.Abnormality, message.IsCritical,
                message.Time.Ticks, petSource, message.HitDirection);
        }
    }
}