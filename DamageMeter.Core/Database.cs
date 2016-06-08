using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter
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

            string sql = "create table entity (" +
              "id int PRIMARY KEY AUTOINCREMENT NOT NULL," +
              "entity_id int NOT NULL," +
              "name varchar(100) NOT NULL" +
              "); ";

            SQLiteCommand command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_name` ON `entity` (`name` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_entity_id` ON `entity` (`entity_id` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "create table damage ("+
                "id int PRIMARY KEY AUTOINCREMENT NOT NULL,"+
                "amount int NOT NULL,"+
                "type int NOT NULL," +
                "target int NOT NULL," +
                "source int NOT NULL," +
                "skill_id int NOT NULL," +
                "critic tinyint NOT NULL," +
                "time datetime NOT NULL," +
                "CONSTRAINT `fk_target`"+
                "FOREIGN KEY (`target` )"+
                "REFERENCES `entity` (`id` )"+
                "ON DELETE CASCADE"+
                "ON UPDATE CASCADE,"+
                "CONSTRAINT `fk_source`" +
                "FOREIGN KEY (`source` )" +
                "REFERENCES `entity` (`id` )" +
                "ON DELETE CASCADE" +
                "ON UPDATE CASCADE," +
                "); ";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_type` ON `damage` (`type` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_target` ON `damage` (`target` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_source` ON `damage` (`source` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_skill_id` ON `damage` (`skill_id` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_time` ON `damage` (`time` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

            sql = "CREATE INDEX `index_type` ON `damage` (`type` ASC);";
            command = new SQLiteCommand(sql, Connexion);
            command.ExecuteNonQuery();

        }

       

        ~Database()  // destructor
        {
            Connexion.Close();
        }
    }
}
