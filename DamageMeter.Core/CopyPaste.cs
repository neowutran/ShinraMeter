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


        public static void Copy(IEnumerable<PlayerInfo> playerInfos, long totalDamage, long intervalvalue, string header,
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
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Received.Damage);
                        break;
                    case "name":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerInfosOrdered =
                            playerInfos.OrderBy(playerInfo => playerInfo.Dealt.DamageFraction(totalDamage));
                        break;
                    case "damage_dealt":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Dealt.Damage);
                        break;
                    case "dps":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Dealt.Dps);
                        break;
                    case "crit_rate":
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Dealt.CritRate);
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playerInfos.OrderBy(playerInfo => playerInfo.Received.Hits);
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
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Received.Damage);
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Received.Hits);
                        break;
                    case "name":
                        playerInfosOrdered = playerInfos.OrderByDescending(playerInfo => playerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.DamageFraction(totalDamage));
                        break;
                    case "damage_dealt":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.Damage);
                        break;
                    case "dps":
                        playerInfosOrdered = playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.Dps);
                        break;
                    case "crit_rate":
                        playerInfosOrdered =
                            playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.CritRate);
                        break;
                    default:
                        Console.WriteLine("wrong value for orderby");
                        throw new Exception("wrong value for orderby");
                }
            }

            var dpsString = header;
            var name = "";
            if (NetworkController.Instance.Encounter != null)
            {
                name = NetworkController.Instance.Encounter.Name;
            }

            dpsString = dpsString.Replace("{encounter}", name);
            var interval = TimeSpan.FromSeconds(intervalvalue);
            dpsString = dpsString.Replace("{timer}", interval.ToString(@"mm\:ss"));

            foreach (var playerStats in playerInfosOrdered)
            {
                var currentContent = content;
                if (playerStats.Dealt.Damage == 0) continue;

                currentContent = currentContent.Replace("{dps}",
                    FormatHelpers.Instance.FormatValue(playerStats.Dealt.Dps) + "/s");
                currentContent = currentContent.Replace("{interval}", playerStats.Dealt.Interval + "s");
                currentContent = currentContent.Replace("{damage_dealt}",
                    FormatHelpers.Instance.FormatValue(playerStats.Dealt.Damage));
                currentContent = currentContent.Replace("{class}", playerStats.Class + "");
                currentContent = currentContent.Replace("{fullname}", playerStats.Player.FullName);
                currentContent = currentContent.Replace("{name}", playerStats.Name);
                currentContent = currentContent.Replace("{damage_percentage}",
                    playerStats.Dealt.DamageFraction(totalDamage) + "%");
                currentContent = currentContent.Replace("{crit_rate}", playerStats.Dealt.CritRate + "%");
                currentContent = currentContent.Replace("{biggest_crit}", playerStats.Dealt.DmgBiggestCrit + "%");
                currentContent = currentContent.Replace("{damage_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.Received.Damage));
                currentContent = currentContent.Replace("{hits_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.Received.Hits));

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