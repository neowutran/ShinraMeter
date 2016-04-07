using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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


        public static void Copy(IEnumerable<PlayerInfo> playerInfos, long totalDamage, long partyDps, long firstHit, long lastHit, Entity currentBoss, bool timedEncounter, string header,
            string content, string footer,
            string orderby, string order)
        {
            //stop if nothing to paste
            if (playerInfos == null) return;
            IEnumerable<PlayerInfo> playerInfosOrdered;
            if (order == "ascending")
            {
                switch (orderby)
                {
                    case "damage_received":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Received.Damage(currentBoss, firstHit, lastHit, timedEncounter));
                        break;
                    case "name":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerInfosOrdered =
                            playerInfos.OrderBy(playerInfo => playerInfo.Dealt.DamageFraction(currentBoss, totalDamage, timedEncounter));
                        break;
                    case "damage_dealt":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Dealt.Damage(currentBoss, timedEncounter));
                        break;
                    case "dps":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Dealt.Dps(currentBoss, timedEncounter));
                        break;
                    case "crit_rate":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Dealt.CritRate(currentBoss, timedEncounter));
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playerInfos.OrderBy(playerInfo => playerInfo.Received.Hits(currentBoss, firstHit, lastHit, timedEncounter));
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
            if (currentBoss != null)
            {
                name = currentBoss.Name;
            }

            dpsString = dpsString.Replace("{encounter}", name);
            var interval = TimeSpan.FromSeconds(lastHit - firstHit);
            dpsString = dpsString.Replace("{timer}", interval.ToString(@"mm\:ss"));
            dpsString = dpsString.Replace("{partyDps}", FormatHelpers.Instance.FormatValue(partyDps)+"/s");

            foreach (var playerStats in playerInfosOrdered)
            {
                var currentContent = content;
                if (playerStats.Dealt.Damage(currentBoss, timedEncounter) == 0) continue;

                currentContent = currentContent.Replace("{dps}",
                    FormatHelpers.Instance.FormatValue(playerStats.Dealt.Dps(currentBoss, timedEncounter)) + "/s");
                currentContent = currentContent.Replace("{interval}", playerStats.Dealt.Interval(currentBoss) + "s");
                currentContent = currentContent.Replace("{damage_dealt}",
                    FormatHelpers.Instance.FormatValue(playerStats.Dealt.Damage(currentBoss, timedEncounter)));
                currentContent = currentContent.Replace("{class}", playerStats.Class + "");
                currentContent = currentContent.Replace("{fullname}", playerStats.Player.FullName);
                currentContent = currentContent.Replace("{name}", playerStats.Name);
                currentContent = currentContent.Replace("{deaths}", (playerStats.DeathCounter?.Count(firstHit, lastHit)??0) + "");
                currentContent = currentContent.Replace("{death_duration}", TimeSpan.FromSeconds(playerStats.DeathCounter?.Duration(firstHit, lastHit)??0).ToString(@"mm\:ss"));
                currentContent = currentContent.Replace("{damage_percentage}",
                    playerStats.Dealt.DamageFraction(currentBoss, totalDamage, timedEncounter) + "%");
                currentContent = currentContent.Replace("{crit_rate}", playerStats.Dealt.CritRate(currentBoss, timedEncounter) + "%");
                currentContent = currentContent.Replace("{biggest_crit}", FormatHelpers.Instance.FormatValue(playerStats.Dealt.DmgBiggestCrit(currentBoss, timedEncounter)));
                currentContent = currentContent.Replace("{damage_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.Received.Damage(currentBoss, firstHit, lastHit, timedEncounter)));
                currentContent = currentContent.Replace("{hits_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.Received.Hits(currentBoss, firstHit, lastHit, timedEncounter)));

                dpsString += currentContent;
            }
            dpsString += footer;
            if (dpsString != "")
            {
                Clipboard.SetText(dpsString);
            }
        }
    }
}