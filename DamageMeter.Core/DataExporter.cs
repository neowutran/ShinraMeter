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
using System.IO;
using System.Globalization;

namespace DamageMeter
{
    public class DataExporter
    {

     
        private static void SendAnonymousStatistics(string json)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.PostAsync("http://cloud.neowutran.ovh:8083/store.php", new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json")
                    );
                    var responseString = response.Result.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString.Result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }


        private static EncounterBase GenerateStats(SDespawnNpc despawnNpc, AbnormalityStorage abnormals)
        {
            if (!despawnNpc.Dead) return null;

            var entity = DamageTracker.Instance.GetEntity(despawnNpc.Npc);

            if (!entity.IsBoss || !DamageTracker.Instance.EntitiesStats.ContainsKey(entity)) return null;

            bool timedEncounter = false;

            //Nightmare desolarus
            if (entity.NpcE.Info.HuntingZoneId == 759 && entity.NpcE.Info.TemplateId == 1003)
            {
                timedEncounter = true;
            }

            var firstTick = DamageTracker.Instance.EntitiesStats[entity].FirstHit;
            var lastTick = DamageTracker.Instance.EntitiesStats[entity].LastHit;
            var interTick = lastTick - firstTick;
            var interval = interTick / TimeSpan.TicksPerSecond;
            if (interval == 0)
            {
                return null;
            }
            var stats = DamageTracker.Instance.GetPlayerStats();
            var totaldamage = DamageTracker.Instance.TotalDamage(entity, timedEncounter);
            var partyDps = TimeSpan.TicksPerSecond * totaldamage / interTick;

            var teradpsData = new EncounterBase();
            teradpsData.areaId = entity.NpcE.Info.HuntingZoneId + "";
            teradpsData.bossId = entity.NpcE.Info.TemplateId + "";
            teradpsData.fightDuration = interval + "";
            teradpsData.partyDps = partyDps + "";

            foreach (var debuff in abnormals.Get(entity.NpcE))
            {
                long percentage = (debuff.Value.Duration(firstTick, lastTick) * 100 / interTick);
                if (percentage == 0)
                {
                    continue;
                }
                teradpsData.debuffUptime.Add(new KeyValuePair<string, string>(
                    debuff.Key.Id + "", percentage + ""
                    ));
            }

            foreach (var user in stats)
            {
                var teradpsUser = new Members();
                var damage = user.Dealt.Damage(entity, timedEncounter);
                teradpsUser.playerTotalDamage = damage + "";

                if (damage <= 0)
                {
                    continue;
                }

                var buffs = abnormals.Get(user.Player);
                teradpsUser.playerClass = user.Class.ToString();
                teradpsUser.playerName = user.Name;
                teradpsUser.playerServer = BasicTeraData.Instance.Servers.GetServerName(user.Player.ServerId);
                teradpsUser.playerAverageCritRate = user.Dealt.CritRate(entity, timedEncounter) + "";
                teradpsUser.healCrit = user.IsHealer ? user.Dealt.CritRateHeal(entity, timedEncounter) + "" : null;
                teradpsUser.playerDps = TimeSpan.TicksPerSecond * damage / interTick + "";
                teradpsUser.playerTotalDamagePercentage = user.Dealt.DamageFraction(entity, totaldamage, timedEncounter) + "";

                var death = buffs.Death;
                teradpsUser.playerDeaths = death.Count(firstTick, lastTick) + "";
                teradpsUser.playerDeathDuration = death.Duration(firstTick, lastTick) / TimeSpan.TicksPerSecond + "";

                var aggro = buffs.Aggro(entity.NpcE);
                teradpsUser.aggro = 100 * aggro.Duration(firstTick, lastTick) / interTick + "";

                foreach (var buff in buffs.Times)
                {
                    long percentage = (buff.Value.Duration(firstTick, lastTick) * 100 / interTick);
                    if (percentage == 0)
                    {
                        continue;
                    }
                    teradpsUser.buffUptime.Add(new KeyValuePair<string, string>(
                        buff.Key.Id + "", percentage + ""
                    ));
                }
                var notimedskills = NoTimedSkills(timedEncounter ? user.Dealt.GetSkillsByTime(entity) : user.Dealt.GetSkills(entity));

                foreach (var skill in notimedskills)
                {
                    var skillLog = new SkillLog();
                    var skilldamage = skill.Value.Damage;

                    skillLog.skillAverageCrit = skill.Value.DmgAverageCrit + "";
                    skillLog.skillAverageWhite = skill.Value.DmgAverageHit + "";
                    skillLog.skillCritRate = skill.Value.CritRateDmg + "";
                    skillLog.skillDamagePercent = skill.Value.DamagePercentage(entity, timedEncounter) + "";
                    skillLog.skillHighestCrit = skill.Value.DmgBiggestCrit + "";
                    skillLog.skillHits = skill.Value.HitsDmg + "";
                    skillLog.skillId = BasicTeraData.Instance.SkillDatabase.GetSkillByPetName(skill.Key.NpcInfo?.Name, user.Player.RaceGenderClass)?.Id.ToString() ?? skill.Key.SkillId.ElementAt(0).ToString();
                    skillLog.skillLowestCrit = skill.Value.DmgLowestCrit + "";
                    skillLog.skillTotalDamage = skilldamage + "";

                    if (skilldamage == 0)
                    {
                        continue;
                    }
                    teradpsUser.skillLog.Add(skillLog);
                }
                teradpsData.members.Add(teradpsUser);
            }
            return teradpsData;

        }

        /**
            The datastorage cost almost nothing, so I will use it to provide statistics about most played class, average dps for each class etc. 
            Mostly to expose overpowered class 
            Playername are wiped out client side & server side. No IP address are stored, so it s anonymous. 
        */
        public static void ToAnonymousStatistics(SDespawnNpc despawnNpc, AbnormalityStorage abnormals)
        {
            EncounterBase teradpsData = GenerateStats(despawnNpc, abnormals);
            if (teradpsData == null)
            {
                return;
            }
            foreach(var member in teradpsData.members)
            {
                member.playerName = "";
            }

            string json = JsonConvert.SerializeObject(teradpsData, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            SendAnonymousStatistics(json);
        }


        public static void ToTeraDpsApi(SDespawnNpc despawnNpc, AbnormalityStorage abnormals)
        {
            if(!BasicTeraData.Instance.WindowData.Excel && 
                (string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsToken) 
                    || string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsUser)
                    || !BasicTeraData.Instance.WindowData.SiteExport) )
            {
                return;
            }

            EncounterBase teradpsData = GenerateStats(despawnNpc, abnormals);
            if(teradpsData == null)
            {
                return;
            }
            var entity = DamageTracker.Instance.GetEntity(despawnNpc.Npc);

            if (BasicTeraData.Instance.WindowData.Excel)
            {
                var excelThread = new Thread(() => ExcelExport.ExcelSave(teradpsData));
                excelThread.Start();
            }
            if (string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsToken) || string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsUser) || !BasicTeraData.Instance.WindowData.SiteExport) return;
            string json = JsonConvert.SerializeObject(teradpsData, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var sendThread = new Thread(() => SendTeraDpsIo(entity, json, 3));
            sendThread.Start();
        }

        private static void SendTeraDpsIo(Entity boss, string json, int numberTry)
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
                SendTeraDpsIo(boss, json, numberTry - 1);
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
