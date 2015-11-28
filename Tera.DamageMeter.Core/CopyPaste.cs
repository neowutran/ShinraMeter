using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Tera.DamageMeter
{
    public static class CopyPaste
    {

        public static void Paste(string text)
        {
            TeraWindow.SendString(text);
        }

        public static void Copy(IEnumerable<PlayerInfo> playerInfos, string header, string content, string footer,
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
                        playerInfosOrdered = playerInfos.OrderBy(playerInfo => playerInfo.Dealt.DamageFraction);
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
                    case "name":
                        playerInfosOrdered = playerInfos.OrderByDescending(playerInfo => playerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerInfosOrdered = playerInfos.OrderByDescending(playerInfo => playerInfo.Dealt.DamageFraction);
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
            if (UiModel.Instance.Encounter != null)
            {
                name = UiModel.Instance.Encounter.Name + "=";
            }

            dpsString = dpsString.Replace("{encounter}", name);
            var interval = TimeSpan.FromSeconds(DamageTracker.Instance.Interval);
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
                currentContent = currentContent.Replace("{name}", playerStats.Name);
                currentContent = currentContent.Replace("{damage_percentage}", playerStats.Dealt.DamageFraction + "%");
                currentContent = currentContent.Replace("{crit_rate}", playerStats.Dealt.CritRate + "%");
                currentContent = currentContent.Replace("{damage_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.Received.Damage));
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