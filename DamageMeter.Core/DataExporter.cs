using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using DamageMeter.TeraDpsApi;
using Data;
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


        private static ExtendedStats GenerateStats(SDespawnNpc despawnNpc, AbnormalityStorage abnormals)
        {
            if (!despawnNpc.Dead) return null;

            var entity = (NpcEntity) DamageTracker.Instance.GetEntity(despawnNpc.Npc);
            if (!entity.Info.Boss) return null;

            var timedEncounter = false;

            /*
              modify timedEncounter depending on teradps.io need

            */

            var entityInfo = Database.Database.Instance.GlobalInformationEntity(entity, timedEncounter);
            var skills = Database.Database.Instance.GetSkills(entityInfo.BeginTime, entityInfo.EndTime);
            var playersInfo = timedEncounter
                ? Database.Database.Instance.PlayerInformation(entityInfo.BeginTime, entityInfo.EndTime)
                : Database.Database.Instance.PlayerInformation(entity);

            var heals = playersInfo.Where(x => x.Type == Database.Database.Type.Heal).ToList();
            playersInfo.RemoveAll(x => x.Type != Database.Database.Type.Damage);
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
            teradpsData.AreaId = entity.Info.HuntingZoneId + "";
            teradpsData.BossId = entity.Info.TemplateId + "";
            teradpsData.FightDuration = interval + "";
            teradpsData.PartyDps = partyDps + "";
            extendedStats.Debuffs = _abnormals.Get(entity);

            foreach (var debuff in extendedStats.Debuffs)
            {
                var percentage = debuff.Value.Duration(firstTick, lastTick)*100/interTick;
                if (percentage == 0)
                {
                    continue;
                }
                teradpsData.DebuffUptime.Add(new KeyValuePair<string, string>(
                    debuff.Key.Id + "", percentage + ""
                    ));
            }

            foreach (var user in playersInfo)
            {
                var teradpsUser = new Members();
                var damage = user.Amount;
                teradpsUser.PlayerTotalDamage = damage + "";

                if (damage <= 0)
                {
                    continue;
                }

                var buffs = _abnormals.Get(user.Source);
                teradpsUser.PlayerClass = user.Source.Class.ToString();
                teradpsUser.PlayerName = user.Source.Name;
                teradpsUser.PlayerServer = BasicTeraData.Instance.Servers.GetServerName(user.Source.ServerId);
                teradpsUser.PlayerAverageCritRate = Math.Round(user.CritRate, 1) + "";
                teradpsUser.HealCrit = user.Source.IsHealer
                    ? heals.FirstOrDefault(x => x.Source == user.Source)?.CritRate + ""
                    : null;
                teradpsUser.PlayerDps = TimeSpan.TicksPerSecond*damage/interTick + "";
                teradpsUser.PlayerTotalDamagePercentage = user.Amount/entityInfo.TotalDamage + "";

                var death = buffs.Death;
                teradpsUser.PlayerDeaths = death.Count(firstTick, lastTick) + "";
                teradpsUser.PlayerDeathDuration = death.Duration(firstTick, lastTick)/TimeSpan.TicksPerSecond + "";

                var aggro = buffs.Aggro(entity);
                teradpsUser.Aggro = 100*aggro.Duration(firstTick, lastTick)/interTick + "";

                foreach (var buff in buffs.Times)
                {
                    var percentage = buff.Value.Duration(firstTick, lastTick)*100/interTick;
                    if (percentage == 0)
                    {
                        continue;
                    }
                    teradpsUser.BuffUptime.Add(new KeyValuePair<string, string>(
                        buff.Key.Id + "", percentage + ""
                        ));
                }
                var serverPlayerName = $"{teradpsUser.PlayerServer}_{teradpsUser.PlayerName}";
                extendedStats.PlayerSkills.Add(serverPlayerName,
                    skills.GetSkills(user.Source.User.Id, entity, timedEncounter, entityInfo.BeginTime,
                        entityInfo.EndTime));
                extendedStats.PlayerBuffs.Add(serverPlayerName, buffs);

                var skillsId = skills.SkillsId(user.Source.User, entity, timedEncounter);


                foreach (var skill in skillsId)
                {
                    var skillLog = new SkillLog();
                    var skilldamage = skills.Amount(user.Source.User.Id, entity, skill.Id, timedEncounter);

                    skillLog.SkillAverageCrit =
                        Math.Round(skills.AverageCrit(user.Source.User.Id, entity, skill.Id, timedEncounter)) + "";
                    skillLog.SkillAverageWhite =
                       Math.Round(skills.AverageWhite(user.Source.User.Id, entity, skill.Id, timedEncounter)) + "";
                    skillLog.SkillCritRate = skills.CritRate(user.Source.User.Id, entity, skill.Id, timedEncounter) + "";
                    skillLog.SkillDamagePercent = skills.Amount(user.Source.User.Id, entity, skill.Id, timedEncounter)*100/
                                                  user.Amount + "";
                    skillLog.SkillHighestCrit =
                        skills.BiggestCrit(user.Source.User.Id, entity, skill.Id, timedEncounter) + "";
                    skillLog.SkillHits = skills.Hits(user.Source.User.Id, entity, skill.Id, timedEncounter) + "";
                    skillLog.SkillId= skill.Id + "";
                    skillLog.SkillLowestCrit =
                        skills.LowestCrit(user.Source.User.Id, entity, skill.Id, timedEncounter) + "";
                    skillLog.SkillTotalDamage = skilldamage + "";

                    if (skilldamage == 0)
                    {
                        continue;
                    }
                    teradpsUser.SkillLog.Add(skillLog);
                }
                teradpsData.Members.Add(teradpsUser);
            }
            return extendedStats;
        }

        public static void Export(SDespawnNpc despawnNpc, AbnormalityStorage abnormality)
        {
            var stats = GenerateStats(despawnNpc, abnormality);
            if (stats == null)
            {
                return;
            }
            var sendThread = new Thread(() =>
            {
                ToTeraDpsApi(stats.BaseStats, despawnNpc);
                ExcelExport.ExcelSave(stats);
                ToAnonymousStatistics(stats.BaseStats);
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
            var areaId = int.Parse(teradpsData.AreaId);
            if (
                areaId != 467 &&
                areaId != 767 &&
                areaId != 768 &&
                areaId != 468
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
            if (string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsToken)
                || string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsUser)
                || !BasicTeraData.Instance.WindowData.SiteExport)
            {
                return;
            }


            var entity = DamageTracker.Instance.GetEntity(despawnNpc.Npc);

            if (string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsToken) ||
                string.IsNullOrEmpty(BasicTeraData.Instance.WindowData.TeraDpsUser) ||
                !BasicTeraData.Instance.WindowData.SiteExport) return;

            /*
              Validation, without that, the server cpu will be burning \o 
            */
            var areaId = int.Parse(teradpsData.AreaId);
            if (
                areaId != 467 &&
                areaId != 767 &&
                areaId != 768 &&
                areaId != 468
                )
            {
                return;
            }

            if (int.Parse(teradpsData.PartyDps) < 2000000 && areaId != 468)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(teradpsData,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            SendTeraDpsIo((NpcEntity) entity, json, 3);
        }

        private static void SendTeraDpsIo(NpcEntity boss, string json, int numberTry)
        {
            if (numberTry == 0)
            {
                Console.WriteLine("API ERROR");
                NetworkController.Instance.BossLink.TryAdd(
                    "!Api error or timeout." + " " + boss.Info.Name + " " + boss.Id + " " + DateTime.Now.Ticks, boss);
                return;
            }
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-Auth-Token", BasicTeraData.Instance.WindowData.TeraDpsToken);
                    client.DefaultRequestHeaders.Add("X-User-Id", BasicTeraData.Instance.WindowData.TeraDpsUser);
                    client.Timeout = TimeSpan.FromSeconds(40);

                    var response = client.PostAsync("http://teradps.io/api/que", new StringContent(
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