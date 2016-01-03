using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Data
{
    public class HotkeysData
    {
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

        private readonly bool _fileExist;

        private readonly string _hotkeyFile;

        public HotkeysData(BasicTeraData basicData)
        {
            _fileExist = false;
            DefaultValue();

            // Load XML File
            XDocument xml;
            _hotkeyFile = Path.Combine(basicData.ResourceDirectory, "config/hotkeys.xml");
            try
            {
                xml = XDocument.Load(_hotkeyFile);
            }
            catch (FileNotFoundException)
            {
                return;
            }
            // Get Keys
            var root = xml.Root;
            if (root == null) return;


            _fileExist = true;


            var pasteQuery = from hotkeys in root.Descendants("paste")
                select hotkeys.Element("key");
            var resetQuery = from hotkeys in root.Descendants("reset")
                select hotkeys.Element("key");

            Keys resetKey, pasteKey;

            var xElements = resetQuery as XElement[] ?? resetQuery.ToArray();
            var keyValue = xElements.First().Value;
            keyValue = char.ToUpper(keyValue[0]) + keyValue.Substring(1);
            if (!Enum.TryParse(keyValue, out resetKey))
            {
                var message = "Unable to convert string " + keyValue + " to key. Your hotkeys.xml file is invalid.";
                MessageBox.Show(message);
                throw new InvalidConfigFileException(message);
            }
            var enumerable = pasteQuery as XElement[] ?? pasteQuery.ToArray();
            keyValue = enumerable.First().Value;
            keyValue = char.ToUpper(keyValue[0]) + keyValue.Substring(1);
            if (!Enum.TryParse(keyValue, out pasteKey))
            {
                var message = "Unable to convert string " + keyValue + " to key. Your hotkeys.xml file is invalid.";
                MessageBox.Show(message);
                throw new InvalidConfigFileException(message);
            }


            //Get modifier
            var pasteShiftQuery = from hotkeys in root.Descendants("paste")
                select hotkeys.Element("shift");
            var pasteWindowQuery = from hotkeys in root.Descendants("paste")
                select hotkeys.Element("window");
            var pasteCtrlQuery = from hotkeys in root.Descendants("paste")
                select hotkeys.Element("ctrl");

            var resetShiftQuery = from hotkeys in root.Descendants("reset")
                select hotkeys.Element("shift");
            var resetCtrlQuery = from hotkeys in root.Descendants("reset")
                select hotkeys.Element("ctrl");
            var resetWindowQuery = from hotkeys in root.Descendants("reset")
                select hotkeys.Element("window");
            var resetAltQuery = from hotkeys in root.Descendants("reset")
                select hotkeys.Element("alt");


            bool pasteShift, pasteWindow, resetShift, resetCtrl, resetWindow, resetAlt, pasteCtrl;
            bool.TryParse(pasteShiftQuery.First().Value, out pasteShift);
            bool.TryParse(pasteWindowQuery.First().Value, out pasteWindow);
            bool.TryParse(pasteCtrlQuery.First().Value, out pasteCtrl);


            bool.TryParse(resetShiftQuery.First().Value, out resetShift);
            bool.TryParse(resetCtrlQuery.First().Value, out resetCtrl);
            bool.TryParse(resetWindowQuery.First().Value, out resetWindow);
            bool.TryParse(resetAltQuery.First().Value, out resetAlt);

            var pasteModifier = ConvertToModifierKey(pasteCtrl, false, pasteShift, pasteWindow);
            var resetModifier = ConvertToModifierKey(resetCtrl, resetAlt, resetShift, resetWindow);

            Paste = new KeyValuePair<Keys, ModifierKeys>(pasteKey, pasteModifier);
            Reset = new KeyValuePair<Keys, ModifierKeys>(resetKey, resetModifier);
            Copy = new List<CopyKey>();

            CopyData(xml);
        }

        public List<CopyKey> Copy { get; private set; }
        public KeyValuePair<Keys, ModifierKeys> Reset { get; private set; }
        public KeyValuePair<Keys, ModifierKeys> Paste { get; private set; }

        private void DefaultValue()
        {
            Paste = new KeyValuePair<Keys, ModifierKeys>(Keys.Home, ModifierKeys.None);
            Reset = new KeyValuePair<Keys, ModifierKeys>(Keys.Delete, ModifierKeys.None);
            Copy = new List<CopyKey>
            {
                new CopyKey(
                    @"Damage Taken @ {encounter}:\",
                    "",
                    @"[{class}] {name}: Hits: {hits_received} = {damage_received}\",
                    ModifierKeys.Control,
                    Keys.End,
                    "hits_received",
                    "descending"
                    ),
                new CopyKey(
                    @"Damage Done @ {encounter} {timer}:\",
                    "",
                    @"[{class}] {name}: {damage_percentage} | {crit_rate} Crit | {dps}\",
                    ModifierKeys.Shift,
                    Keys.End,
                    "damage_percentage",
                    "descending"
                    )
            };
        }

        private void CopyData(XDocument xml)
        {
            foreach (var copy in xml.Root.Elements("copys").Elements("copy"))
            {
                bool ctrl, shift, window;

                bool.TryParse(copy.Element("shift").Value, out shift);
                bool.TryParse(copy.Element("window").Value, out window);
                bool.TryParse(copy.Element("ctrl").Value, out ctrl);

                var header = copy.Element("string").Element("header").Value;
                var footer = copy.Element("string").Element("footer").Value;
                var content = copy.Element("string").Element("content").Value;
                Keys key;
                var keyValue = copy.Element("key").Value;
                keyValue = char.ToUpper(keyValue[0]) + keyValue.Substring(1);
                if (!Enum.TryParse(keyValue, out key))
                {
                    var message = "Unable to convert string " + keyValue + " to key. Your hotkeys.xml file is invalid.";
                    MessageBox.Show(message);
                    throw new InvalidConfigFileException(message);
                }
                var order = copy.Element("string").Element("order").Value;
                var orderBy = copy.Element("string").Element("order_by").Value;
                var modifier = ConvertToModifierKey(ctrl, false, shift, window);
                Copy.Add(new CopyKey(header, footer, content, modifier, key, orderBy, order));
            }
        }

        private static ModifierKeys ConvertToModifierKey(bool ctrl, bool alt, bool shift, bool window)
        {
            var modifier = ModifierKeys.None;
            if (ctrl)
            {
                modifier |= ModifierKeys.Control;
            }
            if (alt)
            {
                modifier |= ModifierKeys.Alt;
            }
            if (shift)
            {
                modifier |= ModifierKeys.Shift;
            }
            if (window)
            {
                modifier |= ModifierKeys.Win;
            }
            return modifier;
        }

        public void Save()
        {
            if (_fileExist) return;

            var xml = new XDocument(new XElement("hotkeys"));
            xml.Root.Add(new XElement("paste"));

            var pasteCtrl = (Paste.Value & ModifierKeys.Control) != ModifierKeys.None;
            var pasteShift = (Paste.Value & ModifierKeys.Shift) != ModifierKeys.None;
            var pasteWindow = (Paste.Value & ModifierKeys.Win) != ModifierKeys.None;
            var pasteKey = Paste.Key;
            xml.Root.Element("paste").Add(new XElement("ctrl", pasteCtrl.ToString()));
            xml.Root.Element("paste").Add(new XElement("shift", pasteShift.ToString()));
            xml.Root.Element("paste").Add(new XElement("window", pasteWindow.ToString()));
            xml.Root.Element("paste").Add(new XElement("key", pasteKey.ToString()));

            var resetCtrl = (Reset.Value & ModifierKeys.Control) != ModifierKeys.None;
            var resetShift = (Reset.Value & ModifierKeys.Shift) != ModifierKeys.None;
            var resetWindow = (Reset.Value & ModifierKeys.Win) != ModifierKeys.None;
            var resetAlt = (Reset.Value & ModifierKeys.Alt) != ModifierKeys.None;
            var resetKey = Reset.Key;

            xml.Root.Add(new XElement("reset"));

            xml.Root.Element("reset").Add(new XElement("ctrl", resetCtrl.ToString()));
            xml.Root.Element("reset").Add(new XElement("shift", resetShift.ToString()));
            xml.Root.Element("reset").Add(new XElement("window", resetWindow.ToString()));
            xml.Root.Element("reset").Add(new XElement("alt", resetAlt.ToString()));
            xml.Root.Element("reset").Add(new XElement("key", resetKey.ToString()));

            xml.Root.Add(new XElement("copys"));

            foreach (var copy in Copy)
            {
                var copyElement = new XElement("copy");

                var copyCtrl = (copy.Modifier & ModifierKeys.Control) != ModifierKeys.None;
                var copyShift = (copy.Modifier & ModifierKeys.Shift) != ModifierKeys.None;
                var copyWindow = (copy.Modifier & ModifierKeys.Win) != ModifierKeys.None;
                var copyKey = copy.Key;

                copyElement.Add(new XElement("ctrl", copyCtrl.ToString()));
                copyElement.Add(new XElement("shift", copyShift.ToString()));
                copyElement.Add(new XElement("window", copyWindow.ToString()));
                copyElement.Add(new XElement("key", copyKey.ToString()));
                var stringElement = new XElement("string");

                stringElement.Add(new XElement("header", copy.Header));
                stringElement.Add(new XElement("content", copy.Content));
                stringElement.Add(new XElement("footer", copy.Footer));
                stringElement.Add(new XElement("order_by", copy.OrderBy));
                stringElement.Add(new XElement("order", copy.Order));
                copyElement.Add(stringElement);

                xml.Root.Element("copys").Add(copyElement);
            }

            xml.Save(_hotkeyFile);
        }
    }
}