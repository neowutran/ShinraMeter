using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Tera.DamageMeter.UI.Handler
{
    public static class CopyPaste
    {
        private const int KlNamelength = 9;

        [DllImport("user32.dll")]
        private static extern long GetKeyboardLayoutName(
            StringBuilder pwszKlid);


        // SOME BULLSHIT GOING ON HERE => if you want to send "%" with SendKey, SendKey will you use the keyboard shortcut "SHIFT + 5" => meaning "%" with a QUERTY keyboard
        // So that doesn't work with a french keyboard (AZERTY), so => hack here
        private static string Percentage()
        {
            var name = new StringBuilder(KlNamelength);
            GetKeyboardLayoutName(name);
            var keyBoardLayout = name.ToString();
            keyBoardLayout = keyBoardLayout.ToLower();
            Console.WriteLine("Your keyboard layout is: " + keyBoardLayout);
            //AZERTY
            if (keyBoardLayout == "0000040c" || keyBoardLayout == "0000080c")
            {
                Console.WriteLine("French detected");
                return "+ù";
            }

            //QUERTY & OTHER
            return "{%}";
        }

        public static void Paste(string text)
        {
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(300);
            const int cr = 13;
            const int lf = 10;
            

            char[] specialChars = {'{', '}', '(', ')', '+', '^', '%', '~', '[', ']'};
            foreach (var c in text.Where(c => c != lf && c != cr))
            {
                if (specialChars.Contains(c))
                {

                    if (c == '%')
                    {
                        SendKeys.SendWait(Percentage());
                    }
                    else
                    {
                        SendKeys.SendWait("{" + c + "}");
                    }
                }
                else
                {
                    if (c == '\\')
                    {
                        Thread.Sleep(300);
                        SendKeys.SendWait("{ENTER}");
                        Thread.Sleep(300);
                        SendKeys.SendWait("{ENTER}");
                        Thread.Sleep(300);
                    }
                    else
                    {
                        SendKeys.SendWait(c + "");
                    }
                }
                SendKeys.Flush();
                Thread.Sleep(5);
            }
        }

        public static void Copy(List<PlayerData> playerDatas, string header, string content, string footer,
            string orderby, string order)
        {
            //stop if nothing to paste
            if (playerDatas == null) return;
            IEnumerable<PlayerData> playerDatasOrdered;
            if (order == "ascending")
            {
                switch (orderby)
                {
                    case "damage_received":
                        playerDatasOrdered = playerDatas.OrderBy(playerData => playerData.PlayerInfo.Received.Damage);
                        break;
                    case "name":
                        playerDatasOrdered = playerDatas.OrderBy(playerData => playerData.PlayerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerDatasOrdered = playerDatas.OrderBy(playerData => playerData.DamageFraction);
                        break;
                    case "damage_dealt":
                        playerDatasOrdered = playerDatas.OrderBy(playerData => playerData.PlayerInfo.Dealt.Damage);
                        break;
                    case "dps":
                        playerDatasOrdered = playerDatas.OrderBy(playerData => playerData.PlayerInfo.Dps);
                        break;
                    case "crit_rate":
                        playerDatasOrdered = playerDatas.OrderBy(playerData => playerData.PlayerInfo.Dealt.CritRate);
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
                        playerDatasOrdered =
                            playerDatas.OrderByDescending(playerData => playerData.PlayerInfo.Received.Damage);
                        break;
                    case "name":
                        playerDatasOrdered = playerDatas.OrderByDescending(playerData => playerData.PlayerInfo.Name);
                        break;
                    case "damage_percentage":
                        playerDatasOrdered = playerDatas.OrderByDescending(playerData => playerData.DamageFraction);
                        break;
                    case "damage_dealt":
                        playerDatasOrdered =
                            playerDatas.OrderByDescending(playerData => playerData.PlayerInfo.Dealt.Damage);
                        break;
                    case "dps":
                        playerDatasOrdered = playerDatas.OrderByDescending(playerData => playerData.PlayerInfo.Dps);
                        break;
                    case "crit_rate":
                        playerDatasOrdered =
                            playerDatas.OrderByDescending(playerData => playerData.PlayerInfo.Dealt.CritRate);
                        break;
                    default:
                        Console.WriteLine("wrong value for orderby");
                        throw new Exception("wrong value for orderby");
                }
            }

            var dpsString = header;
            foreach (var playerStats in playerDatasOrdered)
            {
                var currentContent = content;
                currentContent = currentContent.Replace("{dps}",
                    FormatHelpers.Instance.FormatValue(playerStats.PlayerInfo.Dps) + "/s");
                currentContent = currentContent.Replace("{interval}", playerStats.PlayerInfo.Interval + "s");
                currentContent = currentContent.Replace("{damage_dealt}",
                    FormatHelpers.Instance.FormatValue(playerStats.PlayerInfo.Dealt.Damage));
                currentContent = currentContent.Replace("{name}", playerStats.PlayerInfo.Name);
                currentContent = currentContent.Replace("{damage_percentage}", playerStats.DamageFraction + "%");
                currentContent = currentContent.Replace("{crit_rate}", playerStats.PlayerInfo.Dealt.CritRate + "%");
                currentContent = currentContent.Replace("{damage_received}",
                    FormatHelpers.Instance.FormatValue(playerStats.PlayerInfo.Received.Damage));
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