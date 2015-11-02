using System.Windows.Forms;

namespace Tera.Data
{
    public class CopyKey
    {
        public CopyKey(string header, string footer, string content, HotkeysData.ModifierKeys modifier, Keys key, string orderBy, string order)
        {
            Content = content;
            Header = header;
            Footer = footer;
            Modifier = modifier;
            Key = key;
            OrderBy = orderBy;
            Order = order;
        }

        public string Order { get; }

        public string OrderBy { get; }

        public Keys Key { get; }
        public string Header { get; }
        public string Footer { get; }
        public string Content { get; }
        public bool Alt { get; }
        public bool Ctrl { get; }
        public bool Shift { get; }
        public bool Window { get; }
        public HotkeysData.ModifierKeys Modifier { get; }
    }
}