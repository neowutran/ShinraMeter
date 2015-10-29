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
        const int KL_NAMELENGTH = 9;

        [DllImport("user32.dll")]
        private static extern long GetKeyboardLayoutName(
              System.Text.StringBuilder pwszKLID);


        // SOME BULLSHIT GOING ON HERE => if you want to send "%" with SendKey, SendKey will you use the keyboard shortcut "SHIFT + 5" => meaning "%" with a QUERTY keyboard
        // So that doesn't work with a french keyboard (AZERTY), so => hack here
        private static string Percentage()
        {
            StringBuilder name = new StringBuilder(KL_NAMELENGTH);
            GetKeyboardLayoutName(name);
            String keyBoardLayout = name.ToString();
            keyBoardLayout = keyBoardLayout.ToLower();
            Console.WriteLine("Your keyboard layout is: "+keyBoardLayout);
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
            foreach (var c in text.Where(c => (int) c != lf && (int) c != cr))
            {
                if (specialChars.Contains(c))
                {
                    if (c == '%')
                    {
                        SendKeys.SendWait(Percentage());

                    }else{
                        SendKeys.SendWait("{" + c + "}");
                    }
                }
                else
                {
                    SendKeys.SendWait(c + "");
                }
                SendKeys.Flush();
                Thread.Sleep(20);
            }
        }

        public static void Copy(List<PlayerData> playerDatas)
        {
            //stop if nothing to paste
            if (playerDatas == null || playerDatas.Count == 0) return;
            IEnumerable<PlayerData> playerDatasOrdered =
                playerDatas.OrderByDescending(
                    playerData => playerData.PlayerInfo.Dealt.Damage + playerData.PlayerInfo.Dealt.Heal);
            var dpsString = "";
            foreach (var playerStats in playerDatasOrdered)
            {
                double damageFraction;
                if (playerStats.TotalDamage == 0)
                {
                    damageFraction = 0;
                }
                else
                {
                    damageFraction = (double) playerStats.PlayerInfo.Dealt.Damage/playerStats.TotalDamage;
                }
                var dpsResult =
                    $"|{playerStats.PlayerInfo.Name}: {Math.Round(damageFraction*100.0, 2)}% ({Helpers.FormatValue(playerStats.PlayerInfo.Dealt.Damage)}) - damage {Helpers.FormatValue(playerStats.PlayerInfo.Received.Damage)}";
                dpsString += dpsResult;
            }
            if (dpsString != "")
            {
                Clipboard.SetText(dpsString);
            }
        }
    }
}