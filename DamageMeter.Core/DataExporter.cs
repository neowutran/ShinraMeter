using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using DamageMeter.TeraDpsApi;
using Data;
using Lang;
using Newtonsoft.Json;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class DataExporter
    {
        private static void SendAnonymousStatistics(string json, int numberTry)
        {
            if (numberTry == 0)
            {
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(40);
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
                Thread.Sleep(2000);
                SendAnonymousStatistics(json, numberTry - 1);
            }
        }


        private static ExtendedStats GenerateStats(NpcEntity entity, AbnormalityStorage abnormals)
        {
            if (!entity.Info.Boss) return null;

            var timedEncounter = false;

            /*
              modify timedEncounter depending on teradps.io need

            */

            var entityInfo = Database.Database.Instance.GlobalInformationEntity(entity, timedEncounter);
            var skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
            var playersInfo = timedEncounter
                ? Database.Database.Instance.PlayerDamageInformation(entityInfo.BeginTime, entityInfo.EndTime)
                : Database.Database.Instance.PlayerDamageInformation(entity);
            var heals = Database.Database.Instance.PlayerHealInformation(entityInfo.BeginTime, entityInfo.EndTime);
            playersInfo.RemoveAll(x => x.Amount == 0);
            
            var firstTick = entityInfo.BeginTime;
            var lastTick = entityInfo.EndTime;
            var interTick = lastTick - firstTick;
            var interval = interTick/TimeSpan.TicksPerSecond;
            if (interval == 0)
            {
                return null;
            }
            var totaldamage = entityInfo.TotalDamage;
            var partyDps = TimeSpan.TicksPerSecond*totaldamage/interTick;

            var teradpsData = new EncounterBase();
            var extendedStats = new ExtendedStats();
            var _abnormals = abnormals.Clone(entity, firstTick, lastTick);
            extendedStats.Entity = entity;
            extendedStats.BaseStats = teradpsData;
            extendedStats.FirstTick = firstTick;
            extendedStats.LastTick = lastTick;
            teradpsData.areaId = entity.Info.HuntingZoneId + "";
            teradpsData.bossId = entity.Info.TemplateId + "";
            teradpsData.fightDuration = interval + "";
            teradpsData.partyDps = partyDps + "";
            extendedStats.Debuffs = _abnormals.Get(entity);

            foreach (var debuff in extendedStats.Debuffs)
            {
                var percentage = debuff.Value.Duration(firstTick, lastTick)*100/interTick;
                if (percentage == 0)
                {
                    continue;
                }
                teradpsData.debuffUptime.Add(new KeyValuePair<string, string>(
                    debuff.Key.Id + "", percentage + ""
                    ));
            }

            foreach (var user in playersInfo)
            {
                var teradpsUser = new Members();
                var damage = user.Amount;
                teradpsUser.playerTotalDamage = damage + "";

                if (damage <= 0)
                {
                    continue;
                }

                var buffs = _abnormals.Get(user.Source);
                teradpsUser.playerClass = user.Source.Class.ToString();
                teradpsUser.playerName = user.Source.Name;
                teradpsUser.playerServer = BasicTeraData.Instance.Servers.GetServerName(user.Source.ServerId);
                teradpsUser.playerAverageCritRate = Math.Round(user.CritRate, 1) + "";
                teradpsUser.healCrit = user.Source.IsHealer
                    ? heals.FirstOrDefault(x => x.Source == user.Source)?.CritRate + ""
                    : null;
                teradpsUser.playerDps = TimeSpan.TicksPerSecond*damage/interTick + "";
                teradpsUser.playerTotalDamagePercentage = user.Amount*100/entityInfo.TotalDamage + "";

                extendedStats.PlayerReceived.Add(user.Source.Name, Tuple.Create(skills.HitsReceived(user.Source.User.Id, entity, timedEncounter), skills.DamageReceived(user.Source.User.Id, entity, timedEncounter)));

                var death = buffs.Death;
                teradpsUser.playerDeaths = death.Count(firstTick, lastTick) + "";
                teradpsUser.playerDeathDuration = death.Duration(firstTick, lastTick)/TimeSpan.TicksPerSecond + "";

                var aggro = buffs.Aggro(entity);
                teradpsUser.aggro = 100*aggro.Duration(firstTick, lastTick)/interTick + "";

                foreach (var buff in buffs.Times)
                {
                    var percentage = buff.Value.Duration(firstTick, lastTick)*100/interTick;
                    if (percentage == 0)
                    {
                        continue;
                    }
                    teradpsUser.buffUptime.Add(new KeyValuePair<string, string>(
                        buff.Key.Id + "", percentage + ""
                        ));
                }
                var serverPlayerName = $"{teradpsUser.playerServer}_{teradpsUser.playerName}";
                extendedStats.PlayerSkills.Add(serverPlayerName,
                    skills.GetSkillsDealt(user.Source.User.Id, entity, timedEncounter));
                extendedStats.PlayerBuffs.Add(serverPlayerName, buffs);

                var skillsId = SkillAggregate.GetAggregate(user, entityInfo.Entity, skills, timedEncounter, Database.Database.Type.Damage);
                extendedStats.PlayerSkillsAggregated[teradpsUser.playerServer + "/" + teradpsUser.playerName] = skillsId;

                foreach (var skill in skillsId)
                {
                    var skillLog = new SkillLog();
                    var skilldamage = skill.Amount();

                    skillLog.skillAverageCrit = Math.Round(skill.AvgCrit()) + "";
                    skillLog.skillAverageWhite = Math.Round(skill.AvgWhite()) + "";
                    skillLog.skillCritRate = skill.CritRate() + "";
                    skillLog.skillDamagePercent = skill.DamagePercent() + "";
                    skillLog.skillHighestCrit = skill.BiggestCrit() + "";
                    skillLog.skillHits = skill.Hits() + "";
                    skillLog.skillId= skill.Skills.First().Key.Id + "";
                    skillLog.skillLowestCrit = skill.LowestCrit() + "";
                    skillLog.skillTotalDamage = skilldamage + "";


                    if (skilldamage == 0)
                    {
                        continue;
                    }
                    teradpsUser.skillLog.Add(skillLog);
                }
                teradpsData.members.Add(teradpsUser);
            }
            return extendedStats;
        }

        public static void Export(SDespawnNpc despawnNpc, AbnormalityStorage abnormality)
        {
            if (!despawnNpc.Dead) return;

            var entity = (NpcEntity)DamageTracker.Instance.GetEntity(despawnNpc.Npc);

            var stats = GenerateStats(entity, abnormality);
            if (stats == null)
            {
                return;
            }
            var sendThread = new Thread(() =>
            {
                ToTeraDpsApi(stats.BaseStats, despawnNpc);
                ExcelExport.ExcelSave(stats, NetworkController.Instance.EntityTracker.MeterUser.Name);
                ToAnonymousStatistics(stats.BaseStats);
            });
            sendThread.Start();
        }

        public static void Export(NpcEntity entity, AbnormalityStorage abnormality)
        {
            if (entity==null) return;
            var stats = GenerateStats(entity, abnormality);
            if (stats == null)
            {
                return;
            }
            var sendThread = new Thread(() =>
            {
                ExcelExport.ExcelSave(stats);
            });
            sendThread.Start();
        }

        /**
            The datastorage cost almost nothing, so I will use it to provide statistics about most played class, average dps for each class etc. 
            Mostly to expose overpowered class 
            Playername are wiped out client side & server side. No IP address are stored, so it s anonymous. 
        */

        private static void ToAnonymousStatistics(EncounterBase teradpsData)
        {
            //Leveling area only, don't care about that
            var areaId = int.Parse(teradpsData.areaId);
            if (
                areaId != 886 &&
                areaId != 467 &&
                areaId != 767 &&
                areaId != 768 &&
                areaId != 470 &&
                areaId != 468 && 
                areaId != 770 && 
                areaId != 769 &&
                areaId != 916 &&
                areaId != 969 && 
                areaId != 970 &&
                areaId != 950 
                )
            {
                return;
            }

            SendAnonymousStatistics(
                JsonConvert.SerializeObject(teradpsData,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}), 3);
        }


        private static void ToTeraDpsApi(EncounterBase teradpsData, SDespawnNpc despawnNpc)
        {
            if (
                //string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsToken)
                //|| string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsUser)
                //|| 
                !BasicTeraData.Instance.WindowData.SiteExport)
            {
                return;
            }

            var entity = DamageTracker.Instance.GetEntity(despawnNpc.Npc);

            /*
              Validation, without that, the server cpu will be burning \o 
            */
            var areaId = int.Parse(teradpsData.areaId);
            if (
                 areaId != 886 &&
                //areaId != 467 &&
                //areaId != 767 &&
                //areaId != 768 &&
                //areaId != 470 &&
                areaId != 468
                //areaId != 770 &&
                //areaId != 769 &&
                //areaId != 916 &&
                //areaId != 969 &&
                //areaId != 970 &&
                //areaId != 950
                )
            {
                return;
            }

            //if (int.Parse(teradpsData.partyDps) < 2000000 && areaId != 468)
            //{
            //    return;
            //}

            var json = JsonConvert.SerializeObject(teradpsData,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            SendTeraDpsIo((NpcEntity) entity, json, 3);
        }

        private static void SendTeraDpsIo(NpcEntity boss, string json, int numberTry)
        {

            Console.WriteLine(json);

            if (numberTry == 0)
            {
                Console.WriteLine("API ERROR");
                NetworkController.Instance.BossLink.TryAdd(
                    "!"+LP.TeraDpsIoApiError + " " + boss.Info.Name + " " + boss.Id + " " + DateTime.Now.Ticks, boss);
                return;
            }
            try
            {
                using (var client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Add("X-Auth-Token", BasicTeraData.Instance.WindowData.TeraDpsToken);
                    //client.DefaultRequestHeaders.Add("X-User-Id", BasicTeraData.Instance.WindowData.TeraDpsUser);

                    client.Timeout = TimeSpan.FromSeconds(40);
                    var response = client.PostAsync("http://moongourd.net/dpsmeter_data.php", new StringContent(
                                          json,
                                          Encoding.UTF8,
                                          "application/json")
                                          );

                    var responseString = response.Result.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString.Result);
                    var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString.Result);
                    if (responseObject.ContainsKey("id"))
                    {
                        NetworkController.Instance.BossLink.TryAdd((string) responseObject["id"], boss);
                    }
                    else
                    {
                        NetworkController.Instance.BossLink.TryAdd(
                            "!" + (string) responseObject["message"] + " " + boss.Info.Name + " " + boss.Id + " " +
                            DateTime.Now.Ticks, boss);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(10000);
                SendTeraDpsIo(boss, json, numberTry - 1);
            }
        }
    }
}
