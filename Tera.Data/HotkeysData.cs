using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Tera.Data
{
    public class HotkeysData
    {
        public  List<CopyKey> Copy { get; }
        public  KeyValuePair<Keys, ModifierKeys> Reset { get; }
        public  KeyValuePair<Keys, ModifierKeys> Paste { get; }


        /// <summary>
        ///     The enumeration of possible modifiers.
        /// </summary>
        [Flags]
        public enum ModifierKeys : uint
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8,
            None = 0
        }

        private void CopyData(XDocument xml)
        {

            foreach (var copy in xml.Root.Elements("copys").Elements("copy"))
            {
                bool ctrl, alt, shift, window;
                bool.TryParse(copy.Element("ctrl").Value, out ctrl);
                bool.TryParse(copy.Element("alt").Value, out alt);
                bool.TryParse(copy.Element("shift").Value, out shift);
                bool.TryParse(copy.Element("window").Value, out window);
                string header = copy.Element("string").Element("header").Value;
                string footer = copy.Element("string").Element("footer").Value;
                string content = copy.Element("string").Element("content").Value;
                Keys key;
                if (!Enum.TryParse(copy.Element("key").Value, out key))
                {
                    Console.WriteLine("Unable to convert string {0} to key", copy.Element("key").Value);
                    throw new Exception("Unable to convert string "+ copy.Element("key").Value + " to key");
                }

                var modifier = ConvertToModifierKey(ctrl, alt, shift, window);
                Copy.Add(new CopyKey(header,footer,content, modifier, key));
            }

        }

        private ModifierKeys ConvertToModifierKey(bool ctrl, bool alt, bool shift, bool window)
        {
            ModifierKeys modifier = ModifierKeys.None;
            if (ctrl) { modifier |= ModifierKeys.Control; }
            if (alt) { modifier |= ModifierKeys.Alt; }
            if (shift) { modifier |= ModifierKeys.Shift; }
            if (window) { modifier |= ModifierKeys.Win; }
            return modifier;
        }


        public HotkeysData(BasicTeraData basicData)
        {
            // Load XML File
            var xml = XDocument.Load(Path.Combine(basicData.ResourceDirectory, "hotkeys.xml"));

            // Get Keys
            var pasteQuery = from hotkeys in xml.Root.Descendants("paste")
                select hotkeys.Element("key");
            var resetQuery = from hotkeys in xml.Root.Descendants("reset")
                        select hotkeys.Element("key");

            Keys resetKey, pasteKey;
          
            if (!Enum.TryParse(resetQuery.First().Value, out resetKey))
            {
                Console.WriteLine("Unable to convert string {0} to key", resetQuery.First().Value);
            }
            if (!Enum.TryParse(pasteQuery.First().Value, out pasteKey))
            {
                Console.WriteLine("Unable to convert string {0} to key", pasteQuery.First().Value);
            }

            
            //Get modifier
            var pasteShiftQuery = from hotkeys in xml.Root.Descendants("paste")
                select hotkeys.Element("shift");
            var pasteCtrlQuery = from hotkeys in xml.Root.Descendants("paste")
                             select hotkeys.Element("ctrl");
            var pasteWindowQuery = from hotkeys in xml.Root.Descendants("paste")
                             select hotkeys.Element("window");
            var pasteAltQuery = from hotkeys in xml.Root.Descendants("paste")
                             select hotkeys.Element("alt");

            var resetShiftQuery = from hotkeys in xml.Root.Descendants("reset")
                                  select hotkeys.Element("shift");
            var resetCtrlQuery = from hotkeys in xml.Root.Descendants("reset")
                                 select hotkeys.Element("ctrl");
            var resetWindowQuery = from hotkeys in xml.Root.Descendants("reset")
                                   select hotkeys.Element("window");
            var resetAltQuery = from hotkeys in xml.Root.Descendants("reset")
                                select hotkeys.Element("alt");

         
            bool pasteShift, pasteCtrl, pasteWindow, pasteAlt, resetShift, resetCtrl, resetWindow, resetAlt;
            bool.TryParse(pasteShiftQuery.First().Value, out pasteShift);
            bool.TryParse(pasteCtrlQuery.First().Value, out pasteCtrl);
            bool.TryParse(pasteWindowQuery.First().Value, out pasteWindow);
            bool.TryParse(pasteAltQuery.First().Value, out pasteAlt);

         
            bool.TryParse(resetShiftQuery.First().Value, out resetShift);
            bool.TryParse(resetCtrlQuery.First().Value, out resetCtrl);
            bool.TryParse(resetWindowQuery.First().Value, out resetWindow);
            bool.TryParse(resetAltQuery.First().Value, out resetAlt);

            var pasteModifier = ConvertToModifierKey(pasteCtrl, pasteAlt, pasteShift, pasteWindow);
            var resetModifier = ConvertToModifierKey(resetCtrl, resetAlt, resetShift, resetWindow);

            Paste = new KeyValuePair<Keys, ModifierKeys>(pasteKey, pasteModifier);
            Reset = new KeyValuePair<Keys, ModifierKeys>(resetKey, resetModifier);
            Copy = new List<CopyKey>();

            CopyData(xml);
        }
    }
}
