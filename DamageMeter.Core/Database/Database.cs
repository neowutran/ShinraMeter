using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Database
{

    public class Database
    {
        private static Database _instance;
        public static Database Instance => _instance ?? (_instance = new Database());

        public SQLiteConnection Connexion;

        private Database()
        {
            Connexion = new SQLiteConnection("Data Source=:memory:;Version=3;");
            Connexion.Open();
            Init();
        }

        public enum Type { Damage, Heal, Mana };

        private void Init()
        {

            var sql = "create table damage ("+
                "id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,"+
                "amount INTEGER NOT NULL," +
                "type INTEGER NOT NULL," +
                "target INTEGER NOT NULL," +
                "source INTEGER NOT NULL," +
                "skill_id INTEGER NOT NULL," +
                "critic INTEGER NOT NULL," +
                "time INTEGER NOT NULL" +
                "); ";
            var command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_target` ON `damage` (`target` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_time` ON `damage` (`time` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            }
        
        public void DeleteAll()
        {
            string sql = "DELETE FROM damage";
            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();
        }

        public void DeleteEntity(Entity entity)
        {
            string sql = "DELETE FROM damage WHERE source = $entity OR target = $entity";
            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            command.Parameters.AddWithValue("$entity", entity.Id.Id);
            command.ExecuteNonQuery();
        }

        public void Insert(long amount, Type type, Entity target, Entity source, long skillId, bool critic, long time )
        {
            string sql = "INSERT INTO damage (amount, type, target, source, skill_id, critic, time) VALUES( $amount , $type , $target , $source , $skill_id , $critic , $time ) ;";
            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            command.Parameters.AddWithValue("$amount", amount);
            command.Parameters.AddWithValue("$type", (int)type);
            command.Parameters.AddWithValue("$target", target.Id.Id);
            command.Parameters.AddWithValue("$source", source.Id.Id);
            command.Parameters.AddWithValue("$skill_id", skillId);
            command.Parameters.AddWithValue("$critic", critic ? 1 : 0);
            command.Parameters.AddWithValue("$time", time);
            command.ExecuteNonQuery();
        }

        public List<EntityId> AllEntity()
        {
            var entities = new List<EntityId>();
            var sql = "SELECT DISTINCT target, MAX(time) as max_time FROM damage ORDER BY `max_time` DESC;";
            var command = new SQLiteCommand(sql, Connexion);
            var rdr = command.ExecuteReader();
            while (rdr.Read())
            {
                if (rdr.IsDBNull(0)) return entities;
                entities.Add(new EntityId((ulong)rdr.GetInt64(0)));
            }
            return entities;
         }

        public Structures.EntityInformation GlobalInformationEntity(NpcEntity entity, bool timed)
        {
            SQLiteCommand command;

            if (entity == null)
            {
                var sql = "SELECT SUM(amount) as total_amount, MIN(time) as start_time, MAX(time) as end_time " +
               "FROM damage " +
               "WHERE type = $type";
                command = new SQLiteCommand(sql, Connexion);
                command.Parameters.AddWithValue("$type", (int)Type.Damage);
            }
            else {

                if (!timed)
                {
                    var sql = "SELECT SUM(amount) as total_amount, MIN(time) as start_time, MAX(time) as end_time " +
                    "FROM damage " +
                    "WHERE target = $target AND type = $type";
                    command = new SQLiteCommand(sql, Connexion);
                    command.Parameters.AddWithValue("$type", (int)Type.Damage);
                    command.Parameters.AddWithValue("$target", entity.Id.Id);
                }
                else {

                    var sql = "SELECT SUM(amount) as total_amount, MIN(time) as start_time, MAX(time) as end_time " +
                    "FROM damage " +
                    "WHERE time BETWEEN (SELECT MIN(time) FROM damage WHERE target = $target) AND (SELECT MAX(time) FROM damage WHERE target = $target) AND type = $type ";
                    command = new SQLiteCommand(sql, Connexion);
                    command.Parameters.AddWithValue("$type", (int)Type.Damage);
                    command.Parameters.AddWithValue("$target", entity.Id.Id);
               }

            }


            var rdr = command.ExecuteReader();
          
            rdr.Read();
            var totalDamage = rdr.IsDBNull(rdr.GetOrdinal("total_amount")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("total_amount")); 
            var beginTime = rdr.IsDBNull(rdr.GetOrdinal("start_time")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("start_time"));
            var endTime = rdr.IsDBNull(rdr.GetOrdinal("end_time")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("end_time"));
            Console.WriteLine("total:"+totalDamage);
            return new Structures.EntityInformation(entity, totalDamage, beginTime, endTime);
        }

        public Structures.Skills GetSkills(long beginTime, long endTime)
        {
        
          var sql = "SELECT amount, type, target, source, skill_id, critic, time FROM damage WHERE time BETWEEN $begin AND $end ;";

            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            command.Parameters.AddWithValue("$begin", beginTime);
            command.Parameters.AddWithValue("$end", endTime);

            var targetSourceSkills = new Dictionary<EntityId, Dictionary<EntityId, List<Structures.Skill>>>();
            var sourceTargetSkills = new Dictionary<EntityId, Dictionary<EntityId, List<Structures.Skill>>>();
            var sourceTargetIdSkill = new Dictionary<EntityId, Dictionary<EntityId, Dictionary<int, List<Structures.Skill>>>>();
            var sourceIdSkill = new Dictionary<EntityId, Dictionary<int, List<Structures.Skill>>>();
            var rdr = command.ExecuteReader();
            
                while (rdr.Read())
                {

                var amount = rdr.GetFieldValue<long>(rdr.GetOrdinal("amount"));
                var type = (Type)rdr.GetFieldValue<long>(rdr.GetOrdinal("type"));
                var target = new EntityId((ulong)rdr.GetFieldValue<long>(rdr.GetOrdinal("target")));
                var source = new EntityId((ulong)rdr.GetFieldValue<long>(rdr.GetOrdinal("source")));
                var skillid = rdr.GetFieldValue<long>(rdr.GetOrdinal("skill_id"));
                var critic = rdr.GetFieldValue<long>(rdr.GetOrdinal("critic")) == 1 ? true : false;
                var time = rdr.GetFieldValue<long>(rdr.GetOrdinal("time"));
                var skill = new Structures.Skill(amount,type,target,source, (int)skillid,critic,time);

                    if (!targetSourceSkills.ContainsKey(skill.Target))
                    {
                        targetSourceSkills.Add(skill.Target, new Dictionary<EntityId, List<Structures.Skill>>());
                    }

                    if (!targetSourceSkills[skill.Target].ContainsKey(skill.Source))
                    {
                        targetSourceSkills[skill.Target].Add(skill.Source, new List<Structures.Skill>());
                    }

                    if (!sourceTargetSkills.ContainsKey(skill.Source))
                    {
                    sourceTargetIdSkill.Add(skill.Source, new Dictionary<EntityId, Dictionary<int, List<Structures.Skill>>>());
                    sourceIdSkill.Add(skill.Source, new Dictionary<int, List<Structures.Skill>>());
                    sourceTargetSkills.Add(skill.Source, new Dictionary<EntityId, List<Structures.Skill>>());
                    }

                    if (!sourceTargetSkills[skill.Source].ContainsKey(skill.Target))
                    {
                        sourceTargetSkills[skill.Source].Add(skill.Target, new List<Structures.Skill>());
                        sourceTargetIdSkill[skill.Source].Add(skill.Target, new Dictionary<int, List<Structures.Skill>>());
                   }

                if (!sourceTargetIdSkill[skill.Source][skill.Target].ContainsKey(skill.SkillId))
                {
                    sourceTargetIdSkill[skill.Source][skill.Target].Add(skill.SkillId, new List<Structures.Skill>());
                }

                if (!sourceIdSkill[skill.Source].ContainsKey(skill.SkillId))
                {
                    sourceIdSkill[skill.Source].Add(skill.SkillId, new List<Structures.Skill>());
                }

                targetSourceSkills[skill.Target][skill.Source].Add(skill);
                sourceTargetSkills[skill.Source][skill.Target].Add(skill);
                sourceTargetIdSkill[skill.Source][skill.Target][skill.SkillId].Add(skill);
                sourceIdSkill[skill.Source][skill.SkillId].Add(skill);

              }
      
            var skills = new Structures.Skills(sourceTargetSkills, targetSourceSkills, sourceTargetIdSkill, sourceIdSkill);
            return skills;

        }


        public List<Structures.PlayerDealt> PlayerInformation(long beginTime, long endTime)
        {
            Console.WriteLine("start:" + beginTime + ";" + endTime);

            var sql = "SELECT SUM(amount) as total_amount, MIN(time) as start_time, MAX(time) as end_time, SUM(critic) as number_critics, COUNT(*) AS number_hits, source, target, type " +
                "FROM damage " +
             "WHERE time BETWEEN $begin AND $end " +
             "GROUP BY type " +
             "ORDER BY `total_amount` DESC;";
            
            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            command.Parameters.AddWithValue("$begin", beginTime);
            command.Parameters.AddWithValue("$end", endTime);
            return PlayerInformation(command);
        }

        private List<Structures.PlayerDealt> PlayerInformation(SQLiteCommand command)
        {
            
            var result = new List<Structures.PlayerDealt>();

            var rdr = command.ExecuteReader();
            
                while (rdr.Read())
                {
                    var source = new EntityId((ulong)rdr.GetInt64(rdr.GetOrdinal("source")));
                    var entity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(source);
                    UserEntity user = null;
                    if (entity is UserEntity)
                    {
                        user = (UserEntity)entity;
                        Player player = NetworkController.Instance.PlayerTracker.GetOrNull(user.ServerId, user.PlayerId);
                        if (player != null)
                        {

                            var amount = rdr.IsDBNull(rdr.GetOrdinal("total_amount")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("total_amount"));
                            var beginTime = rdr.IsDBNull(rdr.GetOrdinal("start_time")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("start_time")); ;
                            var endTime = rdr.IsDBNull(rdr.GetOrdinal("end_time")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("end_time")); ;
                            var critic = rdr.IsDBNull(rdr.GetOrdinal("number_critics")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("number_critics")); ;
                            var hit = rdr.IsDBNull(rdr.GetOrdinal("number_hits")) ? 0 : rdr.GetFieldValue<long>(rdr.GetOrdinal("number_hits")); ;
                            var entityId = rdr.GetFieldValue<long>(rdr.GetOrdinal("target"));
                            var type = rdr.GetFieldValue<long>(rdr.GetOrdinal("type"));

                            result.Add(new Structures.PlayerDealt(
                                                 amount,
                                                 beginTime,
                                                 endTime,
                                                 critic,
                                                 hit,
                                                 player,
                                                 new EntityId((ulong)entityId),
                                                 (Type)type
                                             ));
                        }
                    }
                
            }
           return result;
        }

        public List<Structures.PlayerDealt> PlayerInformation(NpcEntity target)
        {
            SQLiteCommand command;
            string sql;
           if (target == null)
            {
                sql = "SELECT SUM(amount) as total_amount, MIN(time) as start_time, MAX(time) as end_time, SUM(critic) as number_critics, COUNT(*) AS number_hits, source, target, type "+
                    "FROM damage GROUP BY type ORDER BY `total_amount` DESC;";
                command = new SQLiteCommand(sql, Connexion);
                return PlayerInformation(command);

            }

            sql = "SELECT SUM(amount) as total_amount, MIN(time) as start_time, MAX(time) as end_time, SUM(critic) as number_critics, COUNT(*) AS number_hits, source, target, type " +
            "FROM damage " +
            "WHERE target = $target " +
            "GROUP BY source, type " +
            "ORDER BY `total_amount`  DESC;";
            command = new SQLiteCommand(sql, Connexion);
            command.Parameters.AddWithValue("$target", target.Id.Id);
            return PlayerInformation(command);
             
        }


        ~Database()  // destructor
        {
            Connexion.Close();
        }
    }
}
