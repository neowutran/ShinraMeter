using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Tera.DamageMeter.UI.Handler
{
    public static class CopyPaste
    {
        public static void Paste()
        {
            var text = Clipboard.GetText();
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(300);
            const int cr = 13;
            const int lf = 10;


            char[] specialChars = { '{', '}', '(', ')', '+', '^', '%', '~', '[', ']' };
            foreach (char c in text.Where(c => (int)c != lf && (int)c != cr))
            {
                if (specialChars.Contains(c))
                {
                    SendKeys.SendWait("{" + c + "}");
                }
                else
                {
                    SendKeys.SendWait(c + "");
                }
                SendKeys.Flush();
                Thread.Sleep(20);
            }
        }    
    
    }
}
