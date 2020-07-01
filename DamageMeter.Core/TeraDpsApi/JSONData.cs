using System.Collections.Generic;
using DamageMeter.AutoUpdate;

namespace DamageMeter.TeraDpsApi
{
    public class JSONData
    {
        public string areaId;
        public string bossId;
        public string encounterUnixEpoch;
        public string fightDuration; //seconds
        public string meterVersion = UpdateManager.Version;
        public string partyDps;
        public List<JSONMob> mobs = new List<JSONMob>();
        public List<JSONMember> players = new List<JSONMember>();
    }

    public class JSONMob {
        public string entityId;
        public uint templateId;
        public uint modelId;
        public List<JSONAbnormal> abnormals = new List<JSONAbnormal>();
    }

    public class JSONMember
    {
        public string entityId;
        public uint templateId;
        public uint playerId;
        public uint playerServerId;
        public string playerName;
        public string playerServer;
        public string guild;
        public string playerClass;
        public string aggro;
        public string healCrit;
        public string playerAverageCritRate;
        public string playerDeathDuration;
        public string playerDeaths;
        public string playerDps;
        public string playerTotalDamage;
        public string playerTotalDamagePercentage;
        public List<JSONSkillLog> dealtSkillLog = new List<JSONSkillLog>();
        public List<JSONSkillLog> receivedSkillLog = new List<JSONSkillLog>();
        public List<JSONAbnormal> abnormals = new List<JSONAbnormal>();
    }

    public class JSONAbnormal {
        public int id;
        public int start; //ms from start of the fight
        public int end;
        public int stack;
    }

    public class JSONSkillLog
    {
        public int type; // Damage = 1, Heal = 2, Mana = 3, Counter = 4
        public int time; //ms from start of the fight
        public string source; // entityId
        public string target; // entityId
        public bool crit;
        public bool dot;
        public int skillId;
        public string amount;
    }
}
