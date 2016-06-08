using System;
using System.Collections.Generic;
using System.Linq;
using Tera.Game;
using Data;
using System.Data.SQLite;

namespace DamageMeter
{
    public class DamageTracker
    {
        public delegate void CurrentBossChange(Entity entity);

        private static DamageTracker _instance;


        private DamageTracker()
        {
        }

        public long TotalDamage(Entity entity, bool timedEncounter) 
        {
            string sql = "SELECT SUM(amount) from damage join entity ON damage.target = entity.id WHERE entity.name=? AND entity.id=?";
            SQLiteCommand command = new SQLiteCommand(sql, Database.Instance.Connexion);
            SQLiteParameter id = new SQLiteParameter();
            SQLiteParameter name = new SQLiteParameter();
            command.Parameters.Add(name);
            command.Parameters.Add(id);
            name.Value = entity.Name;
            id.Value = entity.Id.Id;
            long result = (long)command.ExecuteScalar();
            return result;
        }

        public long PartyDps(Entity entity, bool timedEncounter)
        {
            var interval = Interval(entity);
            var damage = TotalDamage(entity, timedEncounter);
            if (interval == 0)
            {
                return damage;
            }
            return damage / interval;
        }

        public long FirstHit(Entity currentBoss)
        {
            string sql = "SELECT MIN(time) from damage join entity ON damage.target = entity.id WHERE entity.name=? AND entity.id=?";
            SQLiteCommand command = new SQLiteCommand(sql, Database.Instance.Connexion);
            SQLiteParameter id = new SQLiteParameter();
            SQLiteParameter name = new SQLiteParameter();
            command.Parameters.Add(name);
            command.Parameters.Add(id);
            name.Value = currentBoss.Name;
            id.Value = currentBoss.Id.Id;
            long result = (long)command.ExecuteScalar();
            return result;

        }

        public long Interval(Entity currentboss) {
            return LastHit(currentboss) - FirstHit(currentboss);
        }

        public long LastHit(Entity currentBoss)
        {
            string sql = "SELECT MAX(time) from damage join entity ON damage.target = entity.id WHERE entity.name=? AND entity.id=?";
            SQLiteCommand command = new SQLiteCommand(sql, Database.Instance.Connexion);
            SQLiteParameter id = new SQLiteParameter();
            SQLiteParameter name = new SQLiteParameter();
            command.Parameters.Add(name);
            command.Parameters.Add(id);
            name.Value = currentBoss.Name;
            id.Value = currentBoss.Id.Id;
            long result = (long)command.ExecuteScalar();
            return result;            
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
                    _toDelete.Add(new Entity(npcEntity));
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

            string sql = "DELETE FROM entity WHERE entity.name=? AND entity.id=?";
            SQLiteCommand command = new SQLiteCommand(sql, Database.Instance.Connexion);
            SQLiteParameter id = new SQLiteParameter();
            SQLiteParameter name = new SQLiteParameter();
            command.Parameters.Add(name);
            command.Parameters.Add(id);
            name.Value = entity.Name;
            id.Value = entity.Id.Id;
            command.ExecuteNonQuery();
          
        }

        public void UpdateCurrentBoss(Entity entity)
        {
            if (entity.IsBoss)
            {
                NetworkController.Instance.NewEncounter = entity;
            }
        }

        public void Reset()
        {
            string sql = "DELETE FROM entity";
            SQLiteCommand command = new SQLiteCommand(sql, Database.Instance.Connexion);
            command.ExecuteNonQuery();

            NetworkController.Instance.NewEncounter = null;
           
        }

        public Entity GetActorEntity(EntityId entityId)
        {
            var entity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(entityId);
            if (entity is NpcEntity)
            {
                var target = (NpcEntity) entity;
                return new Entity(target);
            }
            if (entity is UserEntity)
            {
                var target = (UserEntity) entity;
                return new Entity(target.Name);
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
                if (source.Owner is NpcEntity)
                {
                    var source2 = (NpcEntity) source.Owner;
                    return new Entity(source2);
                }
                if (source.Owner is UserEntity)
                {
                    var source2 = (UserEntity) source.Owner;
                    return new Entity(source2.Name);
                }
                return null;
            }
            return null;
        }

        public void Update(SkillResult skillResult)
        {
            var entitySource = GetActorEntity(skillResult.Source.Id);
            var entityTarget = GetActorEntity(skillResult.Target.Id);

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

            Database.Type type = Database.Type.Mana;

            if (message.IsHp)
            {
                if (message.IsHeal)
                {
                    type = Database.Type.Heal;
                }
                else
                {
                    type = Database.Type.Damage; 
                }

            }
           

            string sql = "INSERT INTO damage (amount, type, target, source, skill_id, critic, time) VALUES(?,?,?,?,?,?,?); ";
            SQLiteCommand command = new SQLiteCommand(sql, Database.Instance.Connexion);
            SQLiteParameter id = new SQLiteParameter();
            SQLiteParameter name = new SQLiteParameter();
            command.Parameters.Add(name);
            command.Parameters.Add(id);
            name.Value = entity.Name;
            id.Value = entity.Id.Id;
            command.ExecuteNonQuery();

        }
    }
}