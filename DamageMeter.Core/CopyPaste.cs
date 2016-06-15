using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Data;
using Tera.Game;
using System.Windows.Forms;

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


        public static string Copy(Database.Data.EntityInformation entityInfo, List<Database.Data.Skill> skills, List<Database.Data.PlayerInformation> playersInfos, AbnormalityStorage abnormals, Entity z     , bool timedEncounter, string header,
            string content, string footer,
            string orderby, string order)
        {
            //stop if nothing to paste
            var firstTick = entityInfo.BeginTime;
            var lastTick =  entityInfo.EndTime;
            var firstHit = firstTick / TimeSpan.TicksPerSecond;
            var lastHit = lastTick / TimeSpan.TicksPerSecond;

            IEnumerable<Database.Data.PlayerInformation> playerInfosOrdered;
            if (order == "ascending")
            {
                switch (orderby)
                {
                    case "damage_received":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Received.Damage(currentBoss, firstHit, lastHit, timedEncounter));
                        break;
                    case "name":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerInfosOrdered =
                            playersInfos.OrderBy(playerInfo => playerInfo.Dealt.DamageFraction(currentBoss, totalDamage, timedEncounter));
                        break;
                    case "damage_dealt":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Dealt.Damage(currentBoss, timedEncounter));
                        break;
                    case "dps":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Dealt.Dps(currentBoss, timedEncounter));
                        break;
                    case "crit_rate":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Dealt.CritRate(currentBoss, timedEncounter));
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playersInfos.OrderBy(playerInfo => playerInfo.Received.Hits(currentBoss, firstHit, lastHit, timedEncounter));
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
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Received.Damage(currentBoss, firstHit, lastHit, timedEncounter));
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Received.Hits(currentBoss, firstHit, lastHit, timedEncounter));
                        break;
                    case "name":
                        playerInfosOrdered = playerInfos.OrderByDescending(playerInfo => playerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.DamageFraction(currentBoss, totalDamage, timedEncounter));
                        break;
                    case "damage_dealt":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.Damage(currentBoss, timedEncounter));
                        break;
                    case "dps":
                        playerInfosOrdered = playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.Dps(currentBoss, timedEncounter));
                        break;
                    case "crit_rate":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.CritRate(currentBoss, timedEncounter));
                        break;
                    default:
                        Console.WriteLine("wrong value for orderby");
                        throw new Exception("wrong value for orderby");
                }
            }

            var dpsString = header;
            var name = "";
            double enrageperc = 0;
            if (currentBoss != null)
            {
                name = currentBoss.Name;
                AbnormalityDuration enrage;
                abnormals.Get(currentBoss.NpcE).TryGetValue(BasicTeraData.Instance.HotDotDatabase.Get(8888888), out enrage);
                enrageperc = (lastTick - firstTick) == 0 ? 0 : (((double)(enrage?.Duration(firstTick, lastTick) ?? 0) / (lastTick - firstTick)));
            }

            dpsString = dpsString.Replace("{encounter}", name);
            var interval = TimeSpan.FromSeconds(lastHit - firstHit);
            dpsString = dpsString.Replace("{timer}", interval.ToString(@"mm\:ss"));
            dpsString = dpsString.Replace("{partyDps}", FormatHelpers.Instance.FormatValue((lastHit - firstHit)>0?totalDamage/(lastHit - firstHit):0)+"/s");
            dpsString = dpsString.Replace("{enrage}", FormatHelpers.Instance.FormatPercent(enrageperc));

            foreach (var playerStats in playerInfosOrdered)
            {
                var currentContent = content;
                if (playerStats.Dealt.Damage(currentBoss, timedEncounter) == 0) continue;

                var buffs = abnormals.Get(playerStats.Player);
                AbnormalityDuration slaying;
                buffs.Times.TryGetValue(BasicTeraData.Instance.HotDotDatabase.Get(8888889), out slaying);
                double slayingperc = (lastTick - firstTick) == 0 ? 0 : (((double)(slaying?.Duration(firstTick, lastTick) ?? 0) / (lastTick - firstTick)));
                currentContent = currentContent.Replace("{slaying}", FormatHelpers.Instance.FormatPercent(slayingperc));
                currentContent = currentContent.Replace("{dps}",
                    FormatHelpers.Instance.FormatValue(playerStats.Dealt.Dps(currentBoss, timedEncounter)) + "/s");
                currentContent = currentContent.Replace("{global_dps}",
                    FormatHelpers.Instance.FormatValue(playerStats.Dealt.GlobalDps(currentBoss, timedEncounter, lastHit - firstHit)) + "/s");
                currentContent = currentContent.Replace("{interval}", playerStats.Dealt.Interval(currentBoss) + "s");
                currentContent = currentContent.Replace("{damage_dealt}",
                    FormatHelpers.Instance.FormatValue(playerStats.Dealt.Damage(currentBoss, timedEncounter)));
                currentContent = currentContent.Replace("{class}", playerStats.Class + "");
                currentContent = currentContent.Replace("{fullname}", playerStats.Player.FullName);
                currentContent = currentContent.Replace("{name}", playerStats.Name);
                currentContent = currentContent.Replace("{deaths}", buffs.Death.Count(firstTick, lastTick) + "");
                currentContent = currentContent.Replace("{death_duration}", TimeSpan.FromTicks(buffs.Death.Duration(firstTick, lastTick)).ToString(@"mm\:ss"));
                currentContent = currentContent.Replace("{aggro}", buffs.Aggro(currentBoss?.NpcE).Count(firstTick, lastTick) + "");
                currentContent = currentContent.Replace("{aggro_duration}", TimeSpan.FromTicks(buffs.Aggro(currentBoss?.NpcE).Duration(firstTick, lastTick)).ToString(@"mm\:ss"));
                currentContent = currentContent.Replace("{damage_percentage}",
                    playerStats.Dealt.DamageFraction(currentBoss, totalDamage, timedEncounter) + "%");
                currentContent = currentContent.Replace("{crit_rate}", playerStats.Dealt.CritRate(currentBoss, timedEncounter) + "%");
                currentContent = currentContent.Replace("{crit_rate_heal}", playerStats.Dealt.CritRateHeal(currentBoss, timedEncounter) + "%");
                currentContent = currentContent.Replace("{biggest_crit}", FormatHelpers.Instance.FormatValue(playerStats.Dealt.DmgBiggestCrit(currentBoss, timedEncounter)));
                currentContent = currentContent.Replace("{damage_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.Received.Damage(currentBoss, firstHit, lastHit, timedEncounter)));
                currentContent = currentContent.Replace("{hits_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.Received.Hits(currentBoss, firstHit, lastHit, timedEncounter)));

                dpsString += currentContent;
            }
            dpsString += footer;
            return dpsString;
        }
    }
}
