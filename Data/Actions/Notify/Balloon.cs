using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Notify
{
    public class Balloon
    {
        public string TitleText { get; set; }
        public string BodyText { get; set; }
        public int DisplayTime { get; set; }

        public Balloon(string titleText, string bodyText, int displayTime)
        {
            TitleText = titleText;
            BodyText = bodyText;
            DisplayTime = displayTime;
        }
    }
}
