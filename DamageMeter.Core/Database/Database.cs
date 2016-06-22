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
                "critic tinyint NOT NULL," +
                "time long NOT NULL"+
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
            string sql = "DELETE FROM damage WHERE source=? OR target=?";
            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter id_source = new SQLiteParameter();
            SQLiteParameter id_target = new SQLiteParameter();
            command.Parameters.Add(id_source);
            command.Parameters.Add(id_target);
            id_source.Value = entity.Id.Id;
            id_target.Value = entity.Id.Id;
            command.ExecuteNonQuery();

        }

        public void Insert(long amount, Type type, Entity target, Entity source, long skillId, bool critic, long time )
        {
            string sql = "INSERT INTO damage (amount, type, target, source, skill_id, critic, time) VALUES(?,?,?,?,?,?,?); ";
            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter parameter_amount = new SQLiteParameter();
            SQLiteParameter parameter_type = new SQLiteParameter();
            SQLiteParameter parameter_target = new SQLiteParameter();
            SQLiteParameter parameter_source = new SQLiteParameter();
            SQLiteParameter skill_id = new SQLiteParameter();
            SQLiteParameter parameter_critic = new SQLiteParameter();
            SQLiteParameter parameter_time = new SQLiteParameter();

            command.Parameters.Add(parameter_amount);
            command.Parameters.Add(parameter_type);
            command.Parameters.Add(parameter_target);
            command.Parameters.Add(parameter_source);
            command.Parameters.Add(skill_id);
            command.Parameters.Add(parameter_critic);
            command.Parameters.Add(parameter_time);
            parameter_amount.Value = amount;
            parameter_type.Value = skill_id;
        
            parameter_target.Value = target.Id.Id;
            parameter_source.Value = source.Id.Id;
            skill_id.Value = skill_id;
            parameter_critic.Value = critic ? 1 : 0;
            parameter_time.Value = time;
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
                var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin " +
               "FROM damage " +
               "WHERE type = ?";
                command = new SQLiteCommand(sql, Connexion);
                var type = new SQLiteParameter();
                command.Parameters.Add(type);
                type.Value = Type.Damage;

            }
            else {

                if (!timed)
                {
                    var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin " +
                    "FROM damage " +
                    "WHERE target = ? AND type = ?";

                    command = new SQLiteCommand(sql, Connexion);
                    var parameter_entity = new SQLiteParameter();
                    var type = new SQLiteParameter();

                    command.Parameters.Add(parameter_entity);
                    command.Parameters.Add(type);

                    parameter_entity.Value = entity.Id.Id;
                    type.Value = Type.Damage;
                }
                else {

                    var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin " +
                    "FROM damage " +
                    "WHERE time BETWEEN (SELECT MIN(time) FROM damage WHERE target = ?) AND (SELECT MAX(time) FROM damage WHERE target = ?) AND type = ? ";

                    command = new SQLiteCommand(sql, Connexion);
                    var parameter_entity = new SQLiteParameter();
                    var type = new SQLiteParameter();

                    command.Parameters.Add(parameter_entity);
                    command.Parameters.Add(parameter_entity);
                    command.Parameters.Add(type);

                    parameter_entity.Value = entity.Id.Id;
                    type.Value = Type.Damage;
               }

            }


            var rdr = command.ExecuteReader();

            rdr.Read();
            var totalDamage = rdr.IsDBNull(0) ? 0 : rdr.GetInt64(0);
            var beginTime = rdr.IsDBNull(1) ? 0 : rdr.GetInt64(1);
            var endTime = rdr.IsDBNull(2) ? 0 : rdr.GetInt64(2);

            return new Structures.EntityInformation(entity, totalDamage, beginTime, endTime);
        }

        public Structures.Skills GetSkills(long beginTime, long endTime)
        {

            var sql = "SELECT * FROM damage WHERE time BETWEEN ? AND ?;";

            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter parameter_begin = new SQLiteParameter();
            SQLiteParameter parameter_end = new SQLiteParameter();

            command.Parameters.Add(parameter_begin);
            command.Parameters.Add(parameter_end);
            parameter_begin.Value = beginTime;
            parameter_end.Value = endTime;
            var targetSourceSkills = new Dictionary<EntityId, Dictionary<EntityId, List<Structures.Skill>>>();
            var sourceTargetSkills = new Dictionary<EntityId, Dictionary<EntityId, List<Structures.Skill>>>();
            var sourceTargetIdSkill = new Dictionary<EntityId, Dictionary<EntityId, Dictionary<int, List<Structures.Skill>>>>();
            var sourceIdSkill = new Dictionary<EntityId, Dictionary<int, List<Structures.Skill>>>();
            var rdr = command.ExecuteReader();
            
                while (rdr.Read())
                {
                    var skill = new Structures.Skill(
                        rdr.GetInt64(0),
                        (Type)rdr.GetInt32(1),
                        new EntityId((ulong)rdr.GetInt64(2)),
                        new EntityId((ulong)rdr.GetInt64(3)),
                        rdr.GetInt32(4),
                        rdr.GetBoolean(5),
                        rdr.GetInt64(6)
                        );
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
            var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin, SUM(critic) as critic, COUNT(*) AS hit, source, target, type " +
                "FROM damage " +
             "WHERE time BETWEEN ? AND ? " +
             "GROUP BY source, type " +
             "ORDER BY `total_damage` DESC ";


            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter parameter_beginTime = new SQLiteParameter();
            SQLiteParameter parameter_endTime = new SQLiteParameter();

            command.Parameters.Add(parameter_beginTime);
            command.Parameters.Add(parameter_endTime);

            parameter_beginTime.Value = beginTime;
            parameter_endTime.Value = endTime;

            return PlayerInformation(command);
        }

        private List<Structures.PlayerDealt> PlayerInformation(SQLiteCommand command)
        {
            
            var result = new List<Structures.PlayerDealt>();

            using (var rdr = command.ExecuteReader())
            {
                while (rdr.Read())
                {
                    var source = new EntityId((ulong)rdr.GetInt64(5));
                    var entity = NetworkController.Instance.EntityTracker.GetOrPlaceholder(source);
                    UserEntity user = null;
                    if (entity is UserEntity)
                    {
                        user = (UserEntity)entity;
                        Player player = NetworkController.Instance.PlayerTracker.GetOrNull(user.ServerId, user.PlayerId);
                        if (player != null)
                        {


                            result.Add(new Structures.PlayerDealt(
                                                 rdr.GetInt64(0),
                                                 rdr.GetInt64(1),
                                                 rdr.GetInt64(2),
                                                 rdr.GetInt32(3),
                                                 rdr.GetInt32(4),
                                                  player,
                                                 new EntityId((ulong)rdr.GetInt64(6)),
                                                 (Type)rdr.GetInt32(7)
                                             ));



                        }
                    }

                 

                }
            }

            return result;
        }

        public List<Structures.PlayerDealt> PlayerInformation(Entity target)
        {
           if(target == null)
            {
                return new List<Structures.PlayerDealt>();
            }

            var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin, SUM(critic) as critic, COUNT(*) AS hit, source, target, type " +
            "FROM damage " +
            "WHERE target=? " +
            "GROUP BY source, type " +
            "ORDER BY `total_damage`  DESC";

            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter id = new SQLiteParameter();
            command.Parameters.Add(id);
            id.Value = target.Id;
            return PlayerInformation(command);
             
        }


        ~Database()  // destructor
        {
            Connexion.Close();
        }
    }
}
