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
using Newtonsoft.Json.Linq;

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
            if(interval == 0)
            {
                return;
            }
            var firstHit = DamageTracker.Instance.FirstHit(entity);
            var lastHit = DamageTracker.Instance.LastHit(entity);


            long partyDps;
            
            var entities =
                DamageTracker.Instance.GetEntityStats();


            long totaldamage = 0;
           
            totaldamage = DamageTracker.Instance.TotalDamage(entity, timedEncounter);
            partyDps = DamageTracker.Instance.PartyDps(entity, timedEncounter);
         
            var teradpsData = new EncounterBase();
            teradpsData.areaId = entity.NpcE.Info.HuntingZoneId+"";
            teradpsData.bossId = entity.NpcE.Info.TemplateId+"";
            teradpsData.fightDuration = interval+"";
            teradpsData.partyDps = partyDps+"";
    
            if (entities.ContainsKey(entity))
            {
                var entityStats = entities[entity];
                foreach (var debuff in entityStats.AbnormalityTime)
                {
                    long percentage = (debuff.Value.Duration(entityStats.FirstHit / TimeSpan.TicksPerSecond, entityStats.LastHit / TimeSpan.TicksPerSecond) * 100 / interval);
                    if(percentage == 0)
                    {
                        continue;
                    }
                    teradpsData.debuffUptime.Add(new KeyValuePair<string, string>(
                        debuff.Key.Id+"", percentage+""
                        ));
                }
            }

            foreach(var user in stats)
            {
                var teradpsUser = new Members();

                var damage = user.Dealt.Damage(entity, timedEncounter);
                teradpsUser.playerTotalDamage = damage+"";
                
               if(damage <= 0)
                {
                    continue;
                }
                teradpsUser.playerClass = user.Class.ToString();
                teradpsUser.playerName = user.Name;
                teradpsUser.playerServer = BasicTeraData.Instance.Servers.GetServerName(user.Player.ServerId);

                teradpsUser.playerAverageCritRate = user.Dealt.CritRate(entity, timedEncounter)+"";
                teradpsUser.playerDps = user.Dealt.GlobalDps(entity, timedEncounter, interval)+"";
                teradpsUser.playerTotalDamagePercentage = user.Dealt.DamageFraction(entity, totaldamage, timedEncounter)+"";

                var death = user.Death;
                teradpsUser.playerDeaths = user.Death.Count(firstHit, lastHit)+"";
                teradpsUser.playerDeathDuration = death.Duration(firstHit, lastHit)+"";
                
                foreach (var buff in user.AbnormalityTime) {
                    long percentage = (buff.Value.Duration(user.Dealt.GetFirstHit(entity), user.Dealt.GetLastHit(entity)) * 100 / interval);
                    if(percentage == 0)
                    {
                        continue;
                    }   
                    teradpsUser.buffUptime.Add(new KeyValuePair<string, string>(
                        buff.Key.Id+"", percentage+""
                        
                    ));
                }
                Dictionary<Skills.Skill.Skill, SkillStats> notimedskills;
                if (timedEncounter) notimedskills = NoTimedSkills(user.Dealt.GetSkillsByTime(entity));
                else                notimedskills = NoTimedSkills(user.Dealt.GetSkills(entity));

                foreach (var skill in notimedskills)
                {
                    var skillLog = new SkillLog();
                    var skilldamage = skill.Value.Damage;

                    skillLog.skillAverageCrit = skill.Value.DmgAverageCrit+"";
                    skillLog.skillAverageWhite = skill.Value.DmgAverageHit+"";
                    skillLog.skillCritRate = skill.Value.CritRateDmg+"";                
                    skillLog.skillDamagePercent = skill.Value.DamagePercentage(entity, timedEncounter)+"";
                    skillLog.skillHighestCrit = skill.Value.DmgBiggestCrit+"";
                    skillLog.skillHits = skill.Value.HitsDmg+"";
                    skillLog.skillId = skill.Key.SkillId.ElementAt(0)+"";
                    skillLog.skillLowestCrit = skill.Value.DmgLowestCrit+"";
                    skillLog.skillTotalDamage = skilldamage+"";

                    if (skilldamage == 0)
                    {
                        continue;
                    }

                    teradpsUser.skillLog.Add(skillLog);
                    
                }
                teradpsData.members.Add(teradpsUser);
            }

            string json = JsonConvert.SerializeObject(teradpsData);

            var sendThread = new Thread(() => Send(entity, json, 3));
            sendThread.Start();
        }

        private static void Send(Entity boss, string json, int numberTry)
        {
            if(numberTry == 0)
            {
                Console.WriteLine("API ERROR");
                NetworkController.Instance.BossLink.Add("!Api error or timeout." + " " + boss.Name + " " + DateTime.Now.Ticks, boss);
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
                    Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string,object>>(responseString.Result);
                    if (responseObject.ContainsKey("id"))
                    {
                        NetworkController.Instance.BossLink.Add((string)responseObject["id"], boss);
                    }
                    else {
                        NetworkController.Instance.BossLink.Add("!" + (string)responseObject["message"] +" "+ boss.Name + " "+DateTime.Now.Ticks, boss);
                   }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Send(boss, json, numberTry - 1);
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
