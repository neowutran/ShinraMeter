using System.Collections.Generic;
using DamageMeter.AutoUpdate;

namespace DamageMeter.TeraDpsApi
{
    public class JsonData
    {
        public string areaId;
        public string bossId;
        public string encounterUnixEpoch;
        public string fightDuration; //seconds
        public string meterVersion = UpdateManager.Version;
        public string partyDps; // for main boss only, same as in moongourd uploads
        public List<JsonMob> mobs = new List<JsonMob>();
        public List<JsonMember> players = new List<JsonMember>();
    }

    public class JsonMob {
        public string entityId;
        public uint huntingZoneId;
        public uint templateId;
        public List<JsonAbnormal> abnormals = new List<JsonAbnormal>();
    }

    public class JsonMember
    {
        public string entityId;
        public int  templateId;
        public uint playerId;
        public uint playerServerId;
        public string playerName;
        public string playerServer;
        public string guild;
        public string playerClass;
        public string aggro; // all stats here are for main boss only, same as in moongourd uploads
        public string healCrit; 
        public string playerAverageCritRate;
        public string playerDeathDuration;
        public string playerDeaths;
        public string playerDps;
        public string playerTotalDamage;
        public string playerTotalDamagePercentage;
        public List<JsonSkill> dealtSkillLog = new List<JsonSkill>();
        public List<JsonSkill> receivedSkillLog = new List<JsonSkill>();
        public List<JsonAbnormal> abnormals = new List<JsonAbnormal>();
    }

    public class JsonAbnormal {
        public int id;
        public int start; //ms from start of the fight
        public int end;
        public int stack;
    }

    public class JsonSkill
    {
        public int type; // Damage = 1, Heal = 2, Mana = 3, Counter = 4
        public int time; //ms from start of the fight
        public string source; // entityId, omitted if in dealtSkillLog or source is unknown
        public string target; // entityId, omitted if in receivedSkillLog or target is unknown
        public bool crit;
        public bool dot;
        public int skillId; //dot id if dot==true, summoning skill id if damage done by pet
        public string amount;
    }
}
