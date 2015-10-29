using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Tera.Data
{
    public class HotkeysData
    {
        public  KeyValuePair<Keys, Dictionary<string, bool>> Copy { get; }
        public  KeyValuePair<Keys, Dictionary<string, bool>> Reset { get; }
        public  KeyValuePair<Keys, Dictionary<string, bool>> Paste { get; }

        public HotkeysData(BasicTeraData basicData)
        {
            // Load XML File
            var xml = XDocument.Load(Path.Combine(basicData.ResourceDirectory, "hotkeys.xml"));

            // Get Keys
            var pasteQuery = from hotkeys in xml.Root.Descendants("paste")
                select hotkeys.Element("key");
            var copyQuery = from hotkeys in xml.Root.Descendants("copy")
                        select hotkeys.Element("key");
            var resetQuery = from hotkeys in xml.Root.Descendants("reset")
                        select hotkeys.Element("key");

            Keys copyKey, resetKey, pasteKey;
            if (!Enum.TryParse(copyQuery.First().Value, out copyKey))
            {
                Console.WriteLine("Unable to convert string {0} to key", Copy);
            }
            if (!Enum.TryParse(resetQuery.First().Value, out resetKey))
            {
                Console.WriteLine("Unable to convert string {0} to key", Reset);
            }
            if (!Enum.TryParse(pasteQuery.First().Value, out pasteKey))
            {
                Console.WriteLine("Unable to convert string {0} to key", Paste);
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

            var copyShiftQuery = from hotkeys in xml.Root.Descendants("copy")
                                  select hotkeys.Element("shift");
            var copyCtrlQuery = from hotkeys in xml.Root.Descendants("copy")
                                 select hotkeys.Element("ctrl");
            var copyWindowQuery = from hotkeys in xml.Root.Descendants("copy")
                                   select hotkeys.Element("window");
            var copyAltQuery = from hotkeys in xml.Root.Descendants("copy")
                                select hotkeys.Element("alt");


            var resetShiftQuery = from hotkeys in xml.Root.Descendants("reset")
                                  select hotkeys.Element("shift");
            var resetCtrlQuery = from hotkeys in xml.Root.Descendants("reset")
                                 select hotkeys.Element("ctrl");
            var resetWindowQuery = from hotkeys in xml.Root.Descendants("reset")
                                   select hotkeys.Element("window");
            var resetAltQuery = from hotkeys in xml.Root.Descendants("reset")
                                select hotkeys.Element("alt");

         
            bool pasteShift, pasteCtrl, pasteWindow, pasteAlt, copyShift, copyCtrl, copyWindow, copyAlt, resetShift, resetCtrl, resetWindow, resetAlt;
            bool.TryParse(pasteShiftQuery.First().Value, out pasteShift);
            bool.TryParse(pasteCtrlQuery.First().Value, out pasteCtrl);
            bool.TryParse(pasteWindowQuery.First().Value, out pasteWindow);
            bool.TryParse(pasteAltQuery.First().Value, out pasteAlt);

            bool.TryParse(copyShiftQuery.First().Value, out copyShift);
            bool.TryParse(copyCtrlQuery.First().Value, out copyCtrl);
            bool.TryParse(copyWindowQuery.First().Value, out copyWindow);
            bool.TryParse(copyAltQuery.First().Value, out copyAlt);

            bool.TryParse(resetShiftQuery.First().Value, out resetShift);
            bool.TryParse(resetCtrlQuery.First().Value, out resetCtrl);
            bool.TryParse(resetWindowQuery.First().Value, out resetWindow);
            bool.TryParse(resetAltQuery.First().Value, out resetAlt);

            //Format modifier
            var pasteModifier = new Dictionary<string, bool>
            {
                {"shift", pasteShift},
                {"ctrl", pasteCtrl},
                {"alt", pasteAlt},
                {"window", pasteWindow}
            };

            var copyModifier = new Dictionary<string, bool>
            {
                {"shift", copyShift},
                {"ctrl", copyCtrl},
                {"alt", copyAlt},
                {"window", copyWindow}
            };

            var resetModifier = new Dictionary<string, bool>
            {
                {"shift", resetShift},
                {"ctrl", resetCtrl},
                {"alt", resetAlt},
                {"window", resetWindow}
            };

            Copy = new KeyValuePair<Keys, Dictionary<string, bool>>(copyKey, copyModifier);
            Paste = new KeyValuePair<Keys, Dictionary<string, bool>>(pasteKey, pasteModifier);
            Reset = new KeyValuePair<Keys, Dictionary<string, bool>>(resetKey, resetModifier);

        }
    }
}
