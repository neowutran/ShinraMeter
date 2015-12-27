using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DamageMeter
{
    public class ChatBuilder
    {

        public ChatBuilder Add(string text, Color color)
        {
            var colorHexa = HexColor(color);
            _chatMessage += "<FONT COLOR=\"" + colorHexa + "\" KERNING=\"0\" SIZE=\"18\" FACE=\"$ChatFont\">" + text + "</FONT>";
            return this;
        }

        public ChatBuilder Add(string text)
        {
            _chatMessage += "<FONT>" + text + "</FONT>";
            return this;
        }

        private string _chatMessage = "";

        private static string HexColor(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public byte[] ToTeraPacket()
        {
            return Encoding.Unicode.GetBytes(_chatMessage);
        }

    }
}
