using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        [Flags]
        public enum Dest
        {
            None = 1,
            Excel = 2,
            Site = 4,
            Manual = 8
        }

        private static void SendAnonymousStatistics(string json, int numberTry)
        {
            if (numberTry == 0)
                return;

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(40);
                    var response = client.PostAsync("https://neowutran.ovh/storage/store.php", new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                    );
                    var responseString = response.Result.Content.ReadAsStringAsync();


                    Debug.WriteLine(responseString.Result);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
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
            var interval = interTick / TimeSpan.TicksPerSecond;
            if (interval == 0)
                return null;
            var totaldamage = entityInfo.TotalDamage;
            var partyDps = TimeSpan.TicksPerSecond * totaldamage / interTick;

            var teradpsData = new EncounterBase();
            var extendedStats = new ExtendedStats();
            var _abnormals = abnormals.Clone(entity, firstTick, lastTick);
            teradpsData.encounterUnixEpoch = new DateTimeOffset(new DateTime(lastTick, DateTimeKind.Utc))
                .ToUnixTimeSeconds();
            extendedStats.Entity = entity;
            extendedStats.BaseStats = teradpsData;
            extendedStats.FirstTick = firstTick;
            extendedStats.LastTick = lastTick;
            teradpsData.areaId = entity.Info.HuntingZoneId + "";
            teradpsData.bossId = entity.Info.TemplateId + "";
            teradpsData.fightDuration = interval + "";
            teradpsData.partyDps = partyDps + "";
            extendedStats.Debuffs = _abnormals.Get(entity);

            foreach (var debuff in extendedStats.Debuffs.OrderByDescending(x => x.Value.Duration(firstTick, lastTick)))
            {
                var percentage = debuff.Value.Duration(firstTick, lastTick) * 100 / interTick;
                if (percentage == 0) continue;
                teradpsData.debuffUptime.Add(new KeyValuePair<string, string>(
                    debuff.Key.Id + "", percentage + ""
                ));
                var stacks = new List<List<int>> {new List<int> {0, (int) percentage}};
                var stackList = debuff.Value.Stacks(firstTick, lastTick).OrderBy(x => x);
                teradpsData.debuffDetail.Add(new List<object> {debuff.Key.Id, stacks});
                if (stackList.Any() && stackList.Max() == 1) continue;
                foreach (var stack in stackList)
                {
                    percentage = debuff.Value.Duration(firstTick, lastTick, stack) * 100 / interTick;
                    if (percentage == 0) continue;
                    stacks.Add(new List<int> {stack, (int) percentage});
                }
            }

            foreach (var user in playersInfo.OrderByDescending(x => x.Amount))
            {
                var teradpsUser = new Members();
                var damage = user.Amount;
                teradpsUser.playerTotalDamage = damage + "";

                if (damage <= 0)
                    continue;

                var buffs = _abnormals.Get(user.Source);
                teradpsUser.guild = string.IsNullOrWhiteSpace(user.Source.GuildName) ? null : user.Source.GuildName;
                teradpsUser.playerClass = user.Source.Class.ToString();
                teradpsUser.playerName = user.Source.Name;
                teradpsUser.playerId = user.Source.PlayerId;
                teradpsUser.playerServer = BasicTeraData.Instance.Servers.GetServerName(user.Source.ServerId);
                teradpsUser.playerAverageCritRate = Math.Round(user.CritRate, 1) + "";
                teradpsUser.healCrit = user.Source.IsHealer
                    ? heals.FirstOrDefault(x => x.Source == user.Source)?.CritRate + ""
                    : null;
                teradpsUser.playerDps = TimeSpan.TicksPerSecond * damage / interTick + "";
                teradpsUser.playerTotalDamagePercentage = user.Amount * 100 / entityInfo.TotalDamage + "";

                extendedStats.PlayerReceived.Add(user.Source.Name,
                    Tuple.Create(skills.HitsReceived(user.Source.User, entity, timedEncounter),
                        skills.DamageReceived(user.Source.User, entity, timedEncounter)));
                extendedStats.PlayerCritDamageRate.Add(user.Source.Name, user.CritDamageRate);

                var death = buffs.Death;
                teradpsUser.playerDeaths = death.Count(firstTick, lastTick) + "";
                teradpsUser.playerDeathDuration = death.Duration(firstTick, lastTick) / TimeSpan.TicksPerSecond + "";

                var aggro = buffs.Aggro(entity);
                teradpsUser.aggro = 100 * aggro.Duration(firstTick, lastTick) / interTick + "";

                foreach (var buff in buffs.Times.OrderByDescending(x => x.Value.Duration(firstTick, lastTick)))
                {
                    var percentage = buff.Value.Duration(firstTick, lastTick) * 100 / interTick;
                    if (percentage == 0)
                        continue;
                    teradpsUser.buffUptime.Add(new KeyValuePair<string, string>(
                        buff.Key.Id + "", percentage + ""
                    ));
                    var stacks = new List<List<int>> {new List<int> {0, (int) percentage}};
                    var stackList = buff.Value.Stacks(firstTick, lastTick).OrderBy(x => x);
                    teradpsUser.buffDetail.Add(new List<object> {buff.Key.Id, stacks});
                    if (stackList.Any() && stackList.Max() == 1) continue;
                    foreach (var stack in buff.Value.Stacks(firstTick, lastTick).OrderBy(x => x))
                    {
                        percentage = buff.Value.Duration(firstTick, lastTick, stack) * 100 / interTick;
                        if (percentage == 0) continue;
                        stacks.Add(new List<int> {stack, (int) percentage});
                    }
                }
                var serverPlayerName = $"{teradpsUser.playerServer}_{teradpsUser.playerName}";
                extendedStats.PlayerSkills.Add(serverPlayerName,
                    skills.GetSkillsDealt(user.Source.User, entity, timedEncounter));
                extendedStats.PlayerBuffs.Add(serverPlayerName, buffs);

                var skillsId = SkillAggregate.GetAggregate(user, entityInfo.Entity, skills, timedEncounter,
                    Database.Database.Type.Damage);
                extendedStats.PlayerSkillsAggregated[teradpsUser.playerServer + "/" + teradpsUser.playerName] =
                    skillsId;

                foreach (var skill in skillsId.OrderByDescending(x => x.Amount()))
                {
                    var skillLog = new SkillLog();
                    var skilldamage = skill.Amount();

                    skillLog.skillAverageCrit = Math.Round(skill.AvgCrit()) + "";
                    skillLog.skillAverageWhite = Math.Round(skill.AvgWhite()) + "";
                    skillLog.skillCritRate = skill.CritRate() + "";
                    skillLog.skillDamagePercent = skill.DamagePercent() + "";
                    skillLog.skillHighestCrit = skill.BiggestCrit() + "";
                    skillLog.skillHits = skill.Hits() + "";
                    var skillKey = skill.Skills.First().Key;
                    skillLog.skillId = BasicTeraData.Instance.SkillDatabase
                                           .GetSkillByPetName(skillKey.NpcInfo?.Name, user.Source.RaceGenderClass)?.Id
                                           .ToString() ?? skillKey.Id.ToString();
                    skillLog.skillLowestCrit = skill.LowestCrit() + "";
                    skillLog.skillTotalDamage = skilldamage + "";


                    if (skilldamage == 0)
                        continue;
                    teradpsUser.skillLog.Add(skillLog);
                }
                if (NetworkController.Instance.MeterPlayers.Contains(user.Source))
                    teradpsData.uploader = teradpsData.members.Count.ToString();
                teradpsData.members.Add(teradpsUser);
            }
            return extendedStats;
        }

        public static void AutomatedExport(SDespawnNpc despawnNpc, AbnormalityStorage abnormality)
        {
            if (!despawnNpc.Dead) return;

            var entity = (NpcEntity) DamageTracker.Instance.GetEntity(despawnNpc.Npc);
            AutomatedExport(entity, abnormality);
        }

        public static void ManualExport(NpcEntity entity, AbnormalityStorage abnormality, Dest type)
        {
            if (entity == null) return;
            var stats = GenerateStats(entity, abnormality);
            if (stats == null) return;
            var sendThread = new Thread(() =>
            {
                if (type.HasFlag(Dest.Site) &&
                    NetworkController.Instance.BossLink.Any(x => x.Value == entity && x.Key.StartsWith("!0")))
                    ToTeraDpsApi(stats.BaseStats, entity);
                if (type.HasFlag(Dest.Site) &&
                    NetworkController.Instance.BossLink.Any(
                        x => x.Value == entity && x.Key.StartsWith("!") && !x.Key.StartsWith("!0")))
                    ToPrivateServer(stats.BaseStats, entity,
                        NetworkController.Instance.BossLink
                            .Where(x => x.Value == entity && x.Key.StartsWith("!") && !x.Key.StartsWith("!0"))
                            .Select(x => int.Parse(x.Key.Substring(1, x.Key.IndexOf(" ", StringComparison.Ordinal) - 1))).ToList());
                if (type.HasFlag(Dest.Excel))
                    ExcelExport.ExcelSave(stats,
                        stats.BaseStats.members.Select(x => x.playerName).FirstOrDefault(x => NetworkController.Instance
                            .MeterPlayers.Select(z => z.Name).Contains(x)), type.HasFlag(Dest.Manual));
            });
            sendThread.Start();
        }

        public static void AutomatedExport(NpcEntity entity, AbnormalityStorage abnormality)
        {
            if (entity == null) return;
            var stats = GenerateStats(entity, abnormality);
            if (stats == null)
                return;

            var sendThread = new Thread(() =>
            {
                ToTeraDpsApi(stats.BaseStats, entity);
                ToPrivateServer(stats.BaseStats, entity);
                ExcelExport.ExcelSave(stats, NetworkController.Instance.EntityTracker.MeterUser.Name);
                ToAnonymousStatistics(stats.BaseStats);
            });
            sendThread.Start();
        }

        private static void ToAnonymousStatistics(EncounterBase teradpsData)
        {
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
                areaId != 710 &&
                areaId != 780 &&
                areaId != 980 &&
                areaId != 781 &&
                areaId != 981 &&
                areaId != 950
            )
                return;

            foreach (var members in teradpsData.members)
            {
                members.playerName = "Anonymous";
                members.playerId = 0;
                members.guild = null;
            }

            SendAnonymousStatistics(
                JsonConvert.SerializeObject(teradpsData,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.None
                    }), 3);
        }

        private static void ToPrivateServer(EncounterBase teradpsData, NpcEntity entity, List<int> serverlist = null)
        {
            if (!BasicTeraData.Instance.WindowData.PrivateServerExport) return;

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
                areaId != 710 &&
                areaId != 780 &&
                areaId != 980 &&
                areaId != 781 &&
                areaId != 981 &&
                areaId != 950
            )
                return;

            var j = BasicTeraData.Instance.WindowData.PrivateDpsServers.Count;
            for (var i = 0; i < j; i++)
            {
                if (serverlist != null && !serverlist.Contains(i + 1)) continue;
                long timediff;
                try
                {
                    var url =
                        new Uri(BasicTeraData.Instance.WindowData.PrivateDpsServers[i])
                            .GetLeftPart(UriPartial.Authority);
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(40);
                        var response = client.GetAsync(url + "/shared/servertime");
                        timediff = (response.Result.Headers.Date.Value.UtcDateTime.Ticks - DateTime.UtcNow.Ticks) /
                                   TimeSpan.TicksPerSecond;
                        teradpsData.encounterUnixEpoch += timediff;
                    }
                }
                catch
                {
                    Debug.WriteLine("Get server time error");
                    NetworkController.Instance.BossLink.TryAdd(
                        "!" + (i + 1) + " " + LP.TeraDpsIoApiError + " " + entity.Info.Name + " " + entity.Id + " " +
                        DateTime.UtcNow.Ticks,
                        entity);
                    continue;
                }

                var json = JsonConvert.SerializeObject(teradpsData,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.None
                    });
                teradpsData.encounterUnixEpoch -= timediff;
                SendTeraDpsIo(entity, json, 3, i + 1);
            }
        }

        private static void ToTeraDpsApi(EncounterBase teradpsData, NpcEntity entity)
        {
            if (!BasicTeraData.Instance.WindowData.SiteExport) return;

            var areaId = int.Parse(teradpsData.areaId);
            if (
                //areaId != 886 &&
                //areaId != 467 &&
                //areaId != 767 &&
                //areaId != 768 &&
                //areaId != 468 &&
                areaId != 770 &&
                areaId != 769 &&
                areaId != 916 &&
                areaId != 969 &&
                areaId != 970 &&
                areaId != 710 &&
                areaId != 780 &&
                areaId != 980 &&
                areaId != 781 &&
                areaId != 981 &&
                !(areaId == 950 && int.Parse(teradpsData.bossId) / 100 != 11)
            )
                return;

            long timediff;
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(40);
                    var response = client.GetAsync("https://moongourd.com/shared/servertime");
                    timediff = (response.Result.Headers.Date.Value.UtcDateTime.Ticks - DateTime.UtcNow.Ticks) /
                               TimeSpan.TicksPerSecond;
                    teradpsData.encounterUnixEpoch += timediff;
                }
            }
            catch
            {
                Debug.WriteLine("Get server time error");
                NetworkController.Instance.BossLink.TryAdd(
                    "!0 " + LP.TeraDpsIoApiError + " " + entity.Info.Name + " " + entity.Id + " " +
                    DateTime.UtcNow.Ticks, entity);
                return;
            }

            var json = JsonConvert.SerializeObject(teradpsData,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.None
                });
            teradpsData.encounterUnixEpoch -= timediff;
            SendTeraDpsIo(entity, json, 3);
            DELETEME(json);
        }

        private static void DELETEME(string json)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Add("X-Auth-Token", BasicTeraData.Instance.WindowData.TeraDpsToken);
                    //client.DefaultRequestHeaders.Add("X-User-Id", BasicTeraData.Instance.WindowData.TeraDpsUser);
                    client.DefaultRequestHeaders.Add("X-Local-Time",
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                    client.Timeout = TimeSpan.FromSeconds(40);
                    var response = client.PostAsync("http://teralogs.com/api/logs", new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                    );
                    var responseString = response.Result.Content.ReadAsStringAsync();
                    Debug.WriteLine(responseString.Result);
                }
            }
            catch
            {
            }
        }

        private static void SendTeraDpsIo(NpcEntity boss, string json, int numberTry, int server = 0)
        {
            var url = server == 0
                ? "https://moongourd.com/dpsmeter_data.php"
                : BasicTeraData.Instance.WindowData.PrivateDpsServers[server - 1];
            Debug.WriteLine(json);

            if (numberTry == 0)
            {
                Console.WriteLine("API ERROR");
                NetworkController.Instance.BossLink.TryAdd(
                    "!" + server + " " + LP.TeraDpsIoApiError + " " + boss.Info.Name + " " + boss.Id + " " +
                    DateTime.UtcNow.Ticks, boss);
                return;
            }
            try
            {
                using (var client = new HttpClient())
                {
                    if (server == 0)
                        client.DefaultRequestHeaders.Add("X-Auth-Token",
                            BasicTeraData.Instance.WindowData.TeraDpsToken);
                    //client.DefaultRequestHeaders.Add("X-User-Id", BasicTeraData.Instance.WindowData.TeraDpsUser);
                    client.DefaultRequestHeaders.Add("X-Local-Time",
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                    client.Timeout = TimeSpan.FromSeconds(40);
                    var response = client.PostAsync(url, new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                    );


                    var responseString = response.Result.Content.ReadAsStringAsync();
                    Debug.WriteLine(responseString.Result);
                    var responseObject =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString.Result);
                    if (responseObject.ContainsKey("id") && ((string) responseObject["id"]).StartsWith("http"))
                        NetworkController.Instance.BossLink.TryAdd((string) responseObject["id"], boss);
                    else
                        NetworkController.Instance.BossLink.TryAdd(
                            "!" + server + " " + (string) responseObject["message"] + " " + boss.Info.Name + " " +
                            boss.Id + " " +
                            DateTime.UtcNow.Ticks, boss);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                Thread.Sleep(10000);
                SendTeraDpsIo(boss, json, numberTry - 1, server);
            }
        }
    }
}