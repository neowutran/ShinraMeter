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
                "id int PRIMARY KEY AUTOINCREMENT NOT NULL,"+
                "amount int NOT NULL,"+
                "type int NOT NULL," +
                "target int NOT NULL," +
                "source int NOT NULL," +
                "skill_id int NOT NULL," +
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

            command.Parameters.Add(amount);
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

        public Data.EntityInformation GlobalInformationEntity(EntityId entity)
        {
            var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin" +
          "FROM damage join entity on damage.target = entity.id" +
          "WHERE entity.entity_id = ? AND type = ?";

            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter parameter_entity = new SQLiteParameter();
            SQLiteParameter type = new SQLiteParameter();


            command.Parameters.Add(parameter_entity);
            command.Parameters.Add(type);

            parameter_entity.Value = entity.Id;
            type.Value = Type.Damage;


            var rdr = command.ExecuteReader();
            return new Data.EntityInformation(rdr.GetInt64(0), rdr.GetInt64(1), rdr.GetInt64(2));
            
        }

        public Data.Skills GetSkills(long beginTime, long endTime)
        {

            var sql = "SELECT * FROM damage WHERE time BETWEEN ? AND ?;";

            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter parameter_begin = new SQLiteParameter();
            SQLiteParameter parameter_end = new SQLiteParameter();

            command.Parameters.Add(parameter_begin);
            command.Parameters.Add(parameter_end);
            parameter_begin.Value = beginTime;
            parameter_end.Value = endTime;
            var targetSourceSkills = new Dictionary<EntityId, Dictionary<EntityId, List<Data.Skill>>>();
            var sourceTargetSkills = new Dictionary<EntityId, Dictionary<EntityId, List<Data.Skill>>>();

            var rdr = command.ExecuteReader();
            
                while (rdr.Read())
                {
                    var skill = new Data.Skill(
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
                        targetSourceSkills.Add(skill.Target, new Dictionary<EntityId, List<Data.Skill>>());
                    }

                    if (!targetSourceSkills[skill.Target].ContainsKey(skill.Source))
                    {
                        targetSourceSkills[skill.Target].Add(skill.Source, new List<Data.Skill>());
                    }

                    if (!sourceTargetSkills.ContainsKey(skill.Source))
                    {
                        sourceTargetSkills.Add(skill.Source, new Dictionary<EntityId, List<Data.Skill>>());
                    }

                    if (!sourceTargetSkills[skill.Source].ContainsKey(skill.Target))
                    {
                        sourceTargetSkills[skill.Source].Add(skill.Target, new List<Data.Skill>());
                    }

                    targetSourceSkills[skill.Target][skill.Source].Add(skill);
                    sourceTargetSkills[skill.Source][skill.Target].Add(skill);


              }
            
            return new Data.Skills(sourceTargetSkills, targetSourceSkills);

        }


        public List<Data.PlayerInformation> PlayerInformation(long beginTime, long endTime)
        {
            var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin, SUM(critic) as critic, COUNT(*) AS hit, source, target, type" +
                "FROM damage " +
             "WHERE time BETWEEN ? AND ?" +
             "GROUP BY source, type" +
             "ORDER BY `total_damage`  DESC";

            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            SQLiteParameter parameter_beginTime = new SQLiteParameter();
            SQLiteParameter parameter_endTime = new SQLiteParameter();

            command.Parameters.Add(parameter_beginTime);
            command.Parameters.Add(parameter_endTime);

            parameter_beginTime.Value = beginTime;
            parameter_endTime.Value = endTime;

            return PlayerInformation(command);
        }

        private List<Data.PlayerInformation> PlayerInformation(SQLiteCommand command)
        {
            
            var result = new List<Data.PlayerInformation>();

            using (var rdr = command.ExecuteReader())
            {
                while (rdr.Read())
                {

                    result.Add(new Data.PlayerInformation(
                        rdr.GetInt64(0),
                        rdr.GetInt64(1),
                        rdr.GetInt64(2),
                        rdr.GetInt32(3),
                        rdr.GetInt32(4),
                        new EntityId((ulong)rdr.GetInt64(5)),
                        new EntityId((ulong)rdr.GetInt64(6)), 
                        (Type) rdr.GetInt32(7)
                    ));

                }
            }

            return result;
        }

        public List<Data.PlayerInformation> PlayerInformation(EntityId target)
        {

            var sql = "SELECT SUM(amount) as total_damage, MIN(time) as debut, MAX(time) as fin, SUM(critic) as critic, COUNT(*) AS hit, source, target, type" +
            "FROM damage " +
            "WHERE target=?" +
            "GROUP BY source, type" +
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
