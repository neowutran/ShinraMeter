using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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

            bool timedEncounter = false;

            //Nightmare desolarus
            if (entity.NpcE.Info.HuntingZoneId == 759 && entity.NpcE.Info.TemplateId == 1003)
            {
                timedEncounter = true;
            }
            
            var stats = DamageTracker.Instance.GetPlayerStats();
            var interval = DamageTracker.Instance.Interval(entity);
            var firstHit = DamageTracker.Instance.FirstHit(entity);
            var lastHit = DamageTracker.Instance.LastHit(entity);


            long partyDps;
            
            var entities =
                DamageTracker.Instance.GetEntityStats();


            long totaldamage = 0;
           
            totaldamage = DamageTracker.Instance.TotalDamage(entity, timedEncounter);
            partyDps = DamageTracker.Instance.PartyDps(entity, timedEncounter);
         
            var teradpsData = new EncounterBase();
            teradpsData.areaId = entity.NpcE.Info.HuntingZoneId;
            teradpsData.bossId = (int) entity.NpcE.Info.TemplateId;
            teradpsData.fightDuration = interval;
            teradpsData.partyDps = partyDps;
    
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
                               
                teradpsUser.playerTotalDamage = user.Dealt.Damage(entity, timedEncounter);
                
               if(teradpsUser.playerTotalDamage <= 0)
                {
                    continue;
                }
                teradpsUser.playerClass = user.Class.ToString();
                teradpsUser.playerName = user.Name;
                teradpsUser.playerServer = BasicTeraData.Instance.Servers.GetServerName(user.Player.ServerId);
                Dictionary < long, Dictionary<Skills.Skill.Skill, SkillStats>> skills;

                teradpsUser.playerAverageCritRate = user.Dealt.CritRate(entity, timedEncounter);
                teradpsUser.playerDps = user.Dealt.Dps(entity, timedEncounter);
                teradpsUser.playerTotalDamagePercentage = user.Dealt.DamageFraction(entity, totaldamage, timedEncounter);

                var death = user.DeathCounter;
                if (death == null)
                {
                    teradpsUser.playerDeaths = 0;
                    teradpsUser.playerDeathDuration = 0;
                }
                else {
                    teradpsUser.playerDeaths = user.DeathCounter.Count(firstHit, lastHit);
                    teradpsUser.playerDeathDuration = death.Duration(firstHit, lastHit);
                }

                skills = user.Dealt.GetSkillsByTime(entity);


                foreach (var buff in user.AbnormalityTime) {
                    teradpsUser.buffUptime.Add(new KeyValuePair<int, long>(
                        buff.Key.Id,
                        (buff.Value.Duration(user.Dealt.GetFirstHit(entity), user.Dealt.GetLastHit(entity)) * 100 / interval)
                    ));
                }
                var notimedskills = NoTimedSkills(skills);

                foreach (var skill in notimedskills)
                {
                    var skillLog = new SkillLog();
                    skillLog.skillAverageCrit = skill.Value.DmgAverageCrit;
                    skillLog.skillAverageWhite = skill.Value.DmgAverageHit;
                    skillLog.skillCritRate = skill.Value.CritRateDmg;

                   
                    skillLog.skillDamagePercent = skill.Value.DamagePercentage(entity, timedEncounter);
                 

                    skillLog.skillHighestCrit = skill.Value.DmgBiggestCrit;
                    skillLog.skillHits = skill.Value.HitsDmg;
                    skillLog.skillId = skill.Key.SkillId.ElementAt(0);
                    skillLog.skillLowestCrit = skill.Value.DmgLowestCrit;
                    skillLog.skillTotalDamage = skill.Value.Damage;

                    teradpsUser.skillLog.Add(skillLog);
                    
                }
                teradpsData.members.Add(teradpsUser);
            }

            string json = JsonConvert.SerializeObject(teradpsData);
            Console.WriteLine(json);

            
            var sendThread = new Thread(() => Send(json, 3));
            sendThread.Start();
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
