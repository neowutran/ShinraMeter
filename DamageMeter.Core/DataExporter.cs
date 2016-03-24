using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;
using Data;
using DamageMeter.TeraDpsApi;
using Newtonsoft.Json;
using Tera.Game;
using DamageMeter.Skills.Skill;
using System.Net.Http;

namespace DamageMeter
{
    public class DataExporter
    {

     
        public static void ToJsonFile()
        {

        }

        public static void ToTeraDpsApi(SDespawnNpc despawnNpc)
        {
            if(string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsToken) || string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsUser))
            {
                return;
            }

            if (!despawnNpc.Dead) return;
            
            var entity = DamageTracker.Instance.GetEntity(despawnNpc.Npc);

            if (!entity.IsBoss) return;
            var stats = DamageTracker.Instance.GetPlayerStats();
            var interval = DamageTracker.Instance.Interval(entity);
            
            var entities =
                DamageTracker.Instance.GetEntityStats();
            var totaldamage = DamageTracker.Instance.TotalDamageBossOnly(entity);

            var teradpsData = new EncounterBase();
            teradpsData.areaId = entity.NpcE.Info.HuntingZoneId;
            teradpsData.bossId = (int) entity.NpcE.Info.TemplateId;
            teradpsData.server = NetworkController.Instance.Server.Name;

            if (entities.ContainsKey(entity))
            {
                var entityStats = entities[entity];
                foreach (var debuff in entityStats.AbnormalityTime)
                {
                    teradpsData.debuffUptime.Add(new KeyValuePair<int, long>(
                        debuff.Key.Id,
                        (debuff.Value.Duration(entityStats.FirstHit / TimeSpan.TicksPerSecond, entityStats.LastHit / TimeSpan.TicksPerSecond) * 100 / interval)
                        ));
                }
            }

            foreach(var user in stats)
            {
                var teradpsUser = new Members();
                teradpsUser.playerTotalDamage = user.Dealt.GetDamageBossOnly(entity);
                if(teradpsUser.playerTotalDamage <= 0)
                {
                    continue;
                }
                teradpsUser.playerClass = user.Class.ToString();
                teradpsUser.playerName = user.Name;
                teradpsUser.playerAverageCritRate = user.Dealt.GetCritRateBossOnly(entity);
                teradpsUser.playerDps = user.Dealt.GetDpsBossOnly(entity);
                teradpsUser.playerTotalDamagePercentage = user.Dealt.DamageFractionBossOnly(entity, totaldamage);

                foreach (var buff in user.AbnormalityTime) {
                    teradpsUser.buffUptime.Add(new KeyValuePair<int, long>(
                        buff.Key.Id,
                        (buff.Value.Duration(user.Dealt.GetFirstHit(entity), user.Dealt.GetLastHit(entity)) * 100 / interval)
                    ));
                }
                var skills = user.Dealt.GetSkills(entity);
                var notimedskills = NoTimedSkills(skills);

                foreach (var skill in notimedskills)
                {
                    var skillLog = new SkillLog();
                    skillLog.skillAverageCrit = skill.Value.DmgAverageCrit;
                    skillLog.skillAverageWhite = skill.Value.DmgAverageHit;
                    skillLog.skillCritRate = skill.Value.CritRate;
                    skillLog.skillDamagePercent = skill.Value.DamagePercentageBossOnly(entity);
                    skillLog.skillHighestCrit = skill.Value.DmgBiggestCrit;
                    skillLog.skillHits = skill.Value.Hits;
                    skillLog.skillId = skill.Key.SkillId.ElementAt(0);
                    skillLog.skillLowestCrit = skill.Value.DmgLowestCrit;
                    skillLog.skillTotalDamage = skill.Value.Damage;

                    teradpsUser.skillLog.Add(skillLog);
                    
                }
                teradpsData.members.Add(teradpsUser);
            }

            string json = JsonConvert.SerializeObject(teradpsData);
            Console.WriteLine(json);

            Send(json, 3);
        }

        private static void Send(string json, int numberTry)
        {
            if(numberTry == 0)
            {
                Console.WriteLine("API ERROR");
                return;
            }
            try {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-Auth-Token", BasicTeraData.Instance.WindowData.TeraDpsToken);
                    client.DefaultRequestHeaders.Add("X-User-Id", BasicTeraData.Instance.WindowData.TeraDpsUser);


                    var response = client.PostAsync("http://teradps.io/api/que", new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json")
                    );

                    var responseString = response.Result.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString.Result);


                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Send(json, numberTry - 1);
            }
        }

        private static Dictionary<Skills.Skill.Skill, SkillStats> NoTimedSkills(
          Dictionary<long, Dictionary<Skills.Skill.Skill, SkillStats>> dictionary)
        {
            var result = new Dictionary<Skills.Skill.Skill, SkillStats>();
            foreach (var timedStats in dictionary)
            {
                foreach (var stats in timedStats.Value)
                {
                    if (result.ContainsKey(stats.Key))
                    {
                        result[stats.Key] += stats.Value;
                        continue;
                    }
                    result.Add(stats.Key, stats.Value);
                }
            }
            return result;
        }
    }
}
