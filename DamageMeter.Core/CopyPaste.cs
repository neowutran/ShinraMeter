using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DamageMeter.Database.Structures;
using Data;
using Lang;
using Tera.Game.Abnormality;
using System.Windows;

namespace DamageMeter
{
    public static class CopyPaste
    {
        private static readonly object Lock = new object();

        public static void Paste(string text)
        {
            if (!Monitor.TryEnter(Lock)) return;
            TeraWindow.SendString(text);
            Monitor.Exit(Lock);
        }

        public static void CopyInspect(string name)
        {

            var clip = string.Empty;
            if (NetworkController.Instance.Server.Region == "TW")   // todo JP & KR command if needed.
                clip = "/查看 ";
            else if (NetworkController.Instance.Server.Region == "JP")
                clip = "/詳細確認 ";
            else if (NetworkController.Instance.Server.Region == "KR")
                clip = "/살펴보기 ";
            else
                clip = "/inspect ";

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    Clipboard.SetText(clip + name);
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                    //Ignore
                }
            }
        }


        public static string Copy(StatsSummary statsSummary, Skills skills, AbnormalityStorage abnormals,
            bool timedEncounter, string header, string content,
            string footer, string orderby, string order)
        {
            //stop if nothing to paste
            var entityInfo = statsSummary.EntityInformation;
            var playersInfos = statsSummary.PlayerDamageDealt;
            var firstTick = entityInfo.BeginTime;
            var lastTick = entityInfo.EndTime;
            var firstHit = firstTick/TimeSpan.TicksPerSecond;
            var lastHit = lastTick/TimeSpan.TicksPerSecond;
            var heals = statsSummary.PlayerHealDealt;
            playersInfos.RemoveAll(x => x.Amount == 0);
            
            IEnumerable<PlayerDamageDealt> playerInfosOrdered;
            if (order == "ascending")
            {
                switch (orderby)
                {
                    case "damage_received":
                        playerInfosOrdered =
                            playersInfos.OrderBy(
                                playerInfo =>
                                    skills.DamageReceived(playerInfo.Source.User, entityInfo.Entity,
                                        timedEncounter));
                        break;
                    case "name":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Source.Name);
                        break;
                    case "damage_percentage":
                    case "damage_dealt":
                    case "dps":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Amount);
                        break;
                    case "crit_rate":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.CritRate);
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playersInfos.OrderBy(
                                playerInfo =>
                                    skills.HitsReceived(playerInfo.Source.User, entityInfo.Entity, timedEncounter));
                        break;
                    default:
                        Console.WriteLine("wrong value for orderby");
                        throw new Exception("wrong value for orderby");
                }
            }
            else
            {
                switch (orderby)
                {
                    case "damage_received":
                        playerInfosOrdered =
                            playersInfos.OrderByDescending(
                                playerInfo =>
                                    skills.DamageReceived(playerInfo.Source.User, entityInfo.Entity,
                                        timedEncounter));
                        break;
                    case "name":
                        playerInfosOrdered = playersInfos.OrderByDescending(playerInfo => playerInfo.Source.Name);
                        break;
                    case "damage_percentage":
                    case "damage_dealt":
                    case "dps":
                        playerInfosOrdered = playersInfos.OrderByDescending(playerInfo => playerInfo.Amount);
                        break;
                    case "crit_rate":
                        playerInfosOrdered = playersInfos.OrderByDescending(playerInfo => playerInfo.CritRate);
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playersInfos.OrderByDescending(
                                playerInfo =>
                                    skills.HitsReceived(playerInfo.Source.User, entityInfo.Entity, timedEncounter));
                        break;
                    default:
                        Console.WriteLine("wrong value for orderby");
                        throw new Exception("wrong value for orderby");
                }
            }

            var dpsString = header;

            var name = entityInfo.Entity?.Info.Name ?? "";
            AbnormalityDuration enrage;
            abnormals.Get(entityInfo.Entity).TryGetValue(BasicTeraData.Instance.HotDotDatabase.Get(8888888), out enrage);
            var enrageperc = lastTick - firstTick == 0
                ? 0
                : (double) (enrage?.Duration(firstTick, lastTick) ?? 0)/(lastTick - firstTick);

            dpsString = dpsString.Replace("{encounter}", name);
            var interval = TimeSpan.FromSeconds(lastHit - firstHit);
            dpsString = dpsString.Replace("{timer}", interval.ToString(@"mm\:ss"));
            dpsString = dpsString.Replace("{partyDps}",
                FormatHelpers.Instance.FormatValue(lastHit - firstHit > 0
                    ? entityInfo.TotalDamage/(lastHit - firstHit)
                    : 0) + LP.PerSecond);
            dpsString = dpsString.Replace("{enrage}", FormatHelpers.Instance.FormatPercent(enrageperc));

            foreach (var playerStats in playerInfosOrdered)
            {
                var currentContent = content;

                var buffs = abnormals.Get(playerStats.Source);
                AbnormalityDuration slaying;
                var firstOrDefault = heals.FirstOrDefault(x => x.Source == playerStats.Source);
                double healCritrate = 0;
                if (firstOrDefault != null)
                {
                    healCritrate = firstOrDefault.CritRate;
                }
                buffs.Times.TryGetValue(BasicTeraData.Instance.HotDotDatabase.Get(8888889), out slaying);
                var slayingperc = lastTick - firstTick == 0
                    ? 0
                    : (double) (slaying?.Duration(firstTick, lastTick) ?? 0)/(lastTick - firstTick);
                currentContent = currentContent.Replace("{slaying}", FormatHelpers.Instance.FormatPercent(slayingperc));
                currentContent = currentContent.Replace("{dps}",
                    FormatHelpers.Instance.FormatValue( playerStats.Interval == 0 ? playerStats.Amount : playerStats.Amount*TimeSpan.TicksPerSecond/playerStats.Interval) +
                    "/s");
                currentContent = currentContent.Replace("{global_dps}",
                    FormatHelpers.Instance.FormatValue(entityInfo.Interval == 0 ? playerStats.Amount : playerStats.Amount*TimeSpan.TicksPerSecond/entityInfo.Interval) +
                    "/s");
                currentContent = currentContent.Replace("{interval}", playerStats.Interval + LP.Seconds);
                currentContent = currentContent.Replace("{damage_dealt}",
                    FormatHelpers.Instance.FormatValue(playerStats.Amount));
                currentContent = currentContent.Replace("{class}", LP.ResourceManager.GetString(playerStats.Source.Class.ToString(), LP.Culture)  + "");
                currentContent = currentContent.Replace("{fullname}", playerStats.Source.FullName);
                currentContent = currentContent.Replace("{name}", playerStats.Source.Name);
                currentContent = currentContent.Replace("{deaths}", buffs.Death.Count(firstTick, lastTick) + "");
                currentContent = currentContent.Replace("{death_duration}",
                    TimeSpan.FromTicks(buffs.Death.Duration(firstTick, lastTick)).ToString(@"mm\:ss"));
                currentContent = currentContent.Replace("{aggro}",
                    buffs.Aggro(entityInfo.Entity).Count(firstTick, lastTick) + "");
                currentContent = currentContent.Replace("{aggro_duration}",
                    TimeSpan.FromTicks(buffs.Aggro(entityInfo.Entity).Duration(firstTick, lastTick))
                        .ToString(@"mm\:ss"));
                currentContent = currentContent.Replace("{damage_percentage}",
                    playerStats.Amount*100/entityInfo.TotalDamage + "%");
                currentContent = currentContent.Replace("{crit_rate}", playerStats.CritRate + "%");
                currentContent = currentContent.Replace("{crit_rate_heal}",
                    healCritrate + "%");
                currentContent = currentContent.Replace("{biggest_crit}",
                    FormatHelpers.Instance.FormatValue(skills.BiggestCrit(playerStats.Source.User, entityInfo.Entity,
                        timedEncounter)));
                currentContent = currentContent.Replace("{damage_received}",
                    FormatHelpers.Instance.FormatValue(skills.DamageReceived(playerStats.Source.User,
                        entityInfo.Entity, timedEncounter)));
                currentContent = currentContent.Replace("{hits_received}",
                    FormatHelpers.Instance.FormatValue(skills.HitsReceived(playerStats.Source.User,
                        entityInfo.Entity, timedEncounter)));

                dpsString += currentContent;
            }
            dpsString += footer;
            return dpsString;
        }
    }
}