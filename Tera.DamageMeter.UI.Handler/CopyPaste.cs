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
        private const int KL_NAMELENGTH = 9;

        [DllImport("user32.dll")]
        private static extern long GetKeyboardLayoutName(
            StringBuilder pwszKLID);


        // SOME BULLSHIT GOING ON HERE => if you want to send "%" with SendKey, SendKey will you use the keyboard shortcut "SHIFT + 5" => meaning "%" with a QUERTY keyboard
        // So that doesn't work with a french keyboard (AZERTY), so => hack here
        private static string Percentage()
        {
            var name = new StringBuilder(KL_NAMELENGTH);
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

        public static void Paste()
        {
            var text = Clipboard.GetText();
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
                    SendKeys.SendWait(c + "");
                }
                SendKeys.Flush();
                Thread.Sleep(1);
            }
        }

        public static void Copy(List<PlayerData> playerDatas, string header, string content, string footer)
        {
            //stop if nothing to paste
            if (playerDatas == null) return;
            IEnumerable<PlayerData> playerDatasOrdered =
                playerDatas.OrderByDescending(
                    playerData => playerData.PlayerInfo.Dealt.Damage + playerData.PlayerInfo.Dealt.Heal);
            var dpsString = header;
            foreach (var playerStats in playerDatasOrdered)
            {
                var currentContent = content;
                currentContent = currentContent.Replace("{dps}", playerStats.PlayerInfo.Dps + "/s");
                currentContent = currentContent.Replace("{interval}", playerStats.PlayerInfo.Interval + "s");
                currentContent = currentContent.Replace("{damage_dealt}",
                    Helpers.FormatValue(playerStats.PlayerInfo.Dealt.Damage));
                currentContent = currentContent.Replace("{name}", playerStats.PlayerInfo.Name);
                currentContent = currentContent.Replace("{percentage}", Math.Round(playerStats.DamageFraction*100.0, 1) + "%");
                currentContent = currentContent.Replace("{damage_received}",
                    Helpers.FormatValue(playerStats.PlayerInfo.Received.Damage));
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