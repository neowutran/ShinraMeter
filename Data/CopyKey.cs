using System.Windows.Forms;

namespace Data
{
    public class CopyKey
    {
        public CopyKey(string header, string footer, string content, HotKey hotkey, string orderBy, string order, string lowDpsContent,
            int lowDpsThreshold, int limitNameLength)
        {
            Hotkey = hotkey;
            Content = content;
            Header = header;
            Footer = footer;
            OrderBy = orderBy;
            Order = order;
            LowDpsContent = lowDpsContent;
            LowDpsThreshold = lowDpsThreshold;
            LimitNameLength = limitNameLength;
        }

        public string Order { get; }

        public string OrderBy { get; }

        public HotKey Hotkey { get; set; }
        public string Header { get; }
        public string Footer { get; }
        public string Content { get; }
        public string LowDpsContent { get; }
        public int LowDpsThreshold { get; }
        public int LimitNameLength { get; }
    }
}