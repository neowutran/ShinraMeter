using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml;
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

        private readonly string _hotkeyFile;
        private readonly FileStream _filestream;

        public HotkeysData(BasicTeraData basicData)
        {
            DefaultValue();

            // Load XML File
            XDocument xml;
            _hotkeyFile = Path.Combine(basicData.ResourceDirectory, "config/hotkeys.xml");

            try
            {
                FileAttributes attrs = File.GetAttributes(_hotkeyFile);
                File.SetAttributes(_hotkeyFile, attrs & ~FileAttributes.ReadOnly);
            }
            catch
            {
                //ignore
            }

            try
            {
                _filestream = new FileStream(_hotkeyFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                xml = XDocument.Load(_filestream);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                Save();
                _filestream = new FileStream(_hotkeyFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch
            {
                return;
            }
            // Get Keys
            var root = xml.Root;
            if (root == null) return;

            var pasteKey = ReadElement(root, "paste",false);
            if (pasteKey != null)
            {
                Paste = (KeyValuePair<Keys, ModifierKeys>)pasteKey;
            }

            var resetKey = ReadElement(root, "reset", true);
            if (resetKey != null)
            {
                Reset = (KeyValuePair<Keys, ModifierKeys>)resetKey;
            }

            Copy = new List<CopyKey>();

            var activateKey = ReadElement(root, "click_throu", true);
            if (activateKey != null)
            {
                ClickThrou = (KeyValuePair<Keys,ModifierKeys>)activateKey;
            }

            var resetCurrentKey = ReadElement(root, "reset_current", true);
            if (resetCurrentKey != null)
            {
                ResetCurrent = (KeyValuePair<Keys, ModifierKeys>)resetCurrentKey;
            }


            CopyData(xml);
        }

        public List<CopyKey> Copy { get; private set; }
        public KeyValuePair<Keys, ModifierKeys> Reset { get; private set; }
        public KeyValuePair<Keys, ModifierKeys> Paste { get; private set; }

        public KeyValuePair<Keys, ModifierKeys> ResetCurrent { get; private set; }


        private static KeyValuePair<Keys, ModifierKeys>? ReadElement(XContainer root,  string element, bool readAlt)
        {
            try
            {
                var query = from hotkeys in root.Descendants(element)
                    select hotkeys.Element("key");
                Keys key;
                var xelements = query as XElement[] ?? query.ToArray();
                var keyValue = xelements.First().Value;
                keyValue = char.ToUpper(keyValue[0]) + keyValue.Substring(1);
                if (!Enum.TryParse(keyValue, out key))
                {
                    var message = "Unable to convert string " + keyValue + " to key. Your hotkeys.xml file is invalid.";
                    MessageBox.Show(message);
                    return null;
                }

                var shiftQuery = from hotkeys in root.Descendants(element)
                    select hotkeys.Element("shift");
                var ctrlQuery = from hotkeys in root.Descendants(element)
                    select hotkeys.Element("ctrl");
                var windowQuery = from hotkeys in root.Descendants(element)
                    select hotkeys.Element("window");
                bool alt = false, shift, ctrl, window;
                bool.TryParse(shiftQuery.First().Value, out shift);
                bool.TryParse(ctrlQuery.First().Value, out ctrl);
                bool.TryParse(windowQuery.First().Value, out window);
                if (readAlt)
                {
                    var altQuery = from hotkeys in root.Descendants(element)
                        select hotkeys.Element("alt");
                    bool.TryParse(altQuery.First().Value, out alt);

                }

                var modifier = ConvertToModifierKey(ctrl, alt, shift, window);
                return new KeyValuePair<Keys, ModifierKeys>(key, modifier);

            }
            catch
            {
                return null;
            }
        } 

        private void DefaultValue()
        {
            Paste = new KeyValuePair<Keys, ModifierKeys>(Keys.Home, ModifierKeys.None);
            Reset = new KeyValuePair<Keys, ModifierKeys>(Keys.Delete, ModifierKeys.None);
            ResetCurrent = new KeyValuePair<Keys, ModifierKeys>(Keys.Delete, ModifierKeys.Control);
            Copy = new List<CopyKey>
            {
                new CopyKey(
                    @"Damage Taken @ {encounter} {timer}:\",
                    "",
                    @"[{class}] {name}: Hits: {hits_received} = {damage_received}; Death {deaths} = {death_duration} Aggro {aggro} = {aggro_duration}\",
                    ModifierKeys.Control,
                    Keys.End,
                    "hits_received",
                    "descending"
                    ),
                new CopyKey(
                    @"Damage Done @ {encounter} {timer} {partyDps} {enrage}:\",
                    "",
                    @"[{class}] {name}: {damage_percentage} | {crit_rate} Crit | {global_dps}\",
                    ModifierKeys.Shift,
                    Keys.End,
                    "damage_percentage",
                    "descending"
                    )
            };
            ClickThrou = new KeyValuePair<Keys, ModifierKeys>(Keys.PageUp, ModifierKeys.Control);

        }

        public KeyValuePair<Keys, ModifierKeys> ClickThrou { get; private set; }

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


            var resetCurrentCtrl = (ResetCurrent.Value & ModifierKeys.Control) != ModifierKeys.None;
            var resetCurrentShift = (ResetCurrent.Value & ModifierKeys.Shift) != ModifierKeys.None;
            var resetCurrentWindow = (ResetCurrent.Value & ModifierKeys.Win) != ModifierKeys.None;
            var resetCurrentAlt = (ResetCurrent.Value & ModifierKeys.Alt) != ModifierKeys.None;
            var resetCurrentKey = ResetCurrent.Key;

            xml.Root.Add(new XElement("reset_current"));

            xml.Root.Element("reset_current").Add(new XElement("ctrl", resetCurrentCtrl.ToString()));
            xml.Root.Element("reset_current").Add(new XElement("shift", resetCurrentShift.ToString()));
            xml.Root.Element("reset_current").Add(new XElement("window", resetCurrentWindow.ToString()));
            xml.Root.Element("reset_current").Add(new XElement("alt", resetCurrentAlt.ToString()));
            xml.Root.Element("reset_current").Add(new XElement("key", resetCurrentKey.ToString()));


            var activateClickThrouCtrl = (ClickThrou.Value & ModifierKeys.Control) != ModifierKeys.None;
            var activateClickThrouShift = (ClickThrou.Value & ModifierKeys.Shift) != ModifierKeys.None;
            var activateClickThrouWindow = (ClickThrou.Value & ModifierKeys.Win) != ModifierKeys.None;
            var activateClickThrouAlt = (ClickThrou.Value & ModifierKeys.Alt) != ModifierKeys.None;
            var activateClickThrouKey = ClickThrou.Key;

            xml.Root.Add(new XElement("click_throu"));

            xml.Root.Element("click_throu").Add(new XElement("ctrl", activateClickThrouCtrl.ToString()));
            xml.Root.Element("click_throu").Add(new XElement("shift", activateClickThrouShift.ToString()));
            xml.Root.Element("click_throu").Add(new XElement("window", activateClickThrouWindow.ToString()));
            xml.Root.Element("click_throu").Add(new XElement("alt", activateClickThrouAlt.ToString()));
            xml.Root.Element("click_throu").Add(new XElement("key", activateClickThrouKey.ToString()));


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

            _filestream.SetLength(0);
            using (StreamWriter sr = new StreamWriter(_filestream))
            {
                // File writing as usual
                sr.Write(xml);
            }
            _filestream.Close();
        }
    }
}