using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Lang;

namespace Data
{
    public struct HotKey
    {
        public HotKey(Keys k, HotkeysData.ModifierKeys m) : this()
        {
            Key = k;
            Modifier = m;
        }

        public Keys Key { get; set; }
        public HotkeysData.ModifierKeys Modifier { get; set; }

        public override bool Equals(object? obj)
        {
            if (!(obj is HotKey other)) return false;
            return other.Key == Key && other.Modifier == Modifier;
        }

        public override string ToString()
        {
            var control = (Modifier & HotkeysData.ModifierKeys.Control) != 0;
            var shift = (Modifier & HotkeysData.ModifierKeys.Shift) != 0;
            var alt = (Modifier & HotkeysData.ModifierKeys.Alt) != 0;
            var win = (Modifier & HotkeysData.ModifierKeys.Win) != 0;

            return $"{(control ? "Ctrl + " : "")}{(shift ? "Shift + " : "")}{(alt ? "Alt + " : "")}{(win ? "Win + " : "")}{Key.ToString()}";
        }

        public KeyValuePair<Keys, HotkeysData.ModifierKeys> ToKeyValuePair()
        {
            return new KeyValuePair<Keys, HotkeysData.ModifierKeys>(Key, Modifier);
        }

        public static HotKey From(in KeyValuePair<Keys, HotkeysData.ModifierKeys> hotkeysTopmost)
        {
            return new HotKey(hotkeysTopmost.Key, hotkeysTopmost.Value);
        }
    }

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

        private FileStream _filestream;

        public HotkeysData(BasicTeraData basicData)
        {
            DefaultValue();

            // Load XML File
            XDocument xml;
            var hotkeyFile = Path.Combine(basicData.ResourceDirectory, "config/hotkeys.xml");

            try
            {
                var attrs = File.GetAttributes(hotkeyFile);
                File.SetAttributes(hotkeyFile, attrs & ~FileAttributes.ReadOnly);
            }
            catch
            {
                //ignore
            }

            try
            {
                _filestream = new FileStream(hotkeyFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                xml = XDocument.Load(_filestream);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                Save();
                _filestream = new FileStream(hotkeyFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch { return; }
            // Get Keys
            var root = xml.Root;
            if (root == null) { return; }

            var topmostKey = ReadElement(root, "topmost", false);
            if (topmostKey != null) { Topmost = (HotKey)topmostKey; }

            var pasteKey = ReadElement(root, "paste", false);
            if (pasteKey != null) { Paste = (HotKey)pasteKey; }

            var resetKey = ReadElement(root, "reset", true);
            if (resetKey != null) { Reset = (HotKey)resetKey; }

            var activateKey = ReadElement(root, "click_throu", true);
            if (activateKey != null) { ClickThrou = (HotKey)activateKey; }

            var resetCurrentKey = ReadElement(root, "reset_current", true);
            if (resetCurrentKey != null) { ResetCurrent = (HotKey)resetCurrentKey; }

            var excelSaveKey = ReadElement(root, "excel_save", true);
            if (excelSaveKey != null) { ExcelSave = (HotKey)excelSaveKey; }

            Copy = new List<CopyKey>();
            CopyData(xml);
        }

        public List<CopyKey> Copy { get; private set; }
        public HotKey Topmost { get; set; }
        public HotKey Reset { get; set; }
        public HotKey Paste { get; set; }

        public HotKey ResetCurrent { get; set; }

        public HotKey ClickThrou { get; set; }

        public HotKey ExcelSave { get; set; }


        private static HotKey? ReadElement(XContainer root, string element, bool readAlt)
        {
            try
            {
                var query = from hotkeys in root.Descendants(element) select hotkeys.Element("key");
                Keys key;
                var xelements = query as XElement[] ?? query.ToArray();
                var keyValue = xelements.First().Value;
                keyValue = char.ToUpper(keyValue[0]) + keyValue.Substring(1);
                if (!Enum.TryParse(keyValue, out key))
                {
                    var message = LP.Unable_to_get_key_from_string + " " + keyValue + ". " + LP.Your_hotkeys_xml_file_is_invalid;
                    MessageBox.Show(message);
                    return null;
                }

                var shiftQuery = from hotkeys in root.Descendants(element) select hotkeys.Element("shift");
                var ctrlQuery = from hotkeys in root.Descendants(element) select hotkeys.Element("ctrl");
                var windowQuery = from hotkeys in root.Descendants(element) select hotkeys.Element("window");
                bool alt = false, shift, ctrl, window;
                bool.TryParse(shiftQuery.First().Value, out shift);
                bool.TryParse(ctrlQuery.First().Value, out ctrl);
                bool.TryParse(windowQuery.First().Value, out window);
                if (readAlt)
                {
                    var altQuery = from hotkeys in root.Descendants(element) select hotkeys.Element("alt");
                    bool.TryParse(altQuery.First().Value, out alt);
                }

                var modifier = ConvertToModifierKey(ctrl, alt, shift, window);
                return new HotKey(key, modifier);
            }
            catch { return null; }
        }

        private void DefaultValue()
        {
            Topmost = new HotKey(Keys.None, ModifierKeys.None);
            Paste = new HotKey(Keys.Home, ModifierKeys.None);
            Reset = new HotKey(Keys.Delete, ModifierKeys.None);
            ResetCurrent = new HotKey(Keys.Delete, ModifierKeys.Control);
            Copy = new List<CopyKey>
            {
                new CopyKey(@"Damage Taken @ {encounter} {timer}:\", "",
                    @"[{class}] {name}: Hits: {hits_received} = {damage_received}; Death {deaths} = {death_duration} Aggro {aggro} = {aggro_duration}\",
                    new HotKey(Keys.End,ModifierKeys.Control), "hits_received", "descending",
                    @"[{class}] {name}: Hits: {hits_received} = {damage_received}; Death {deaths} = {death_duration} Aggro {aggro} = {aggro_duration}\", 4, 15),
                new CopyKey(@"{encounter} {timer} ({enrage}) {partyDps}:\", @"{debuff_list}\",
                    @"{class}: {name}: {global_dps} | {death_duration} - {deaths}\", new HotKey(Keys.End, ModifierKeys.Shift), "damage_percentage",
                    "descending", "", 0, 10)
            };
            ExcelSave = new HotKey(Keys.PageDown, ModifierKeys.Control);
            ClickThrou = new HotKey(Keys.PageUp, ModifierKeys.Control);
        }

        private void CopyData(XDocument xml)
        {
            try
            {
                foreach (var copy in xml.Root.Elements("copys").Elements("copy"))
                {
                    bool ctrl, shift, window;

                    bool.TryParse(copy.Element("shift").Value, out shift);
                    bool.TryParse(copy.Element("window").Value, out window);
                    bool.TryParse(copy.Element("ctrl").Value, out ctrl);

                    var header = copy.Element("string").Element("header").Value.Replace("{body}", "");
                    var footer = copy.Element("string").Element("footer").Value.Replace("{body}", "");
                    var content = copy.Element("string").Element("content").Value.Replace("{body}", "");
                    var lowDpsContent = copy.Element("string").Element("low_dps_content")?.Value.Replace("{body}", "") ?? content;
                    var lowDpsTreshold = int.Parse(copy.Element("string").Element("low_dps_threshold")?.Value ?? "4");
                    var limitNameLength = int.Parse(copy.Element("string").Element("limit_name_length")?.Value ?? "0");
                    Keys key;
                    var keyValue = copy.Element("key").Value;
                    if (!Enum.TryParse(keyValue, out key))
                    {
                        var message = LP.Unable_to_get_key_from_string + " " + keyValue + ". " + LP.Your_hotkeys_xml_file_is_invalid;
                        MessageBox.Show(message);
                        continue;
                    }
                    var order = copy.Element("string").Element("order").Value;
                    var orderBy = copy.Element("string").Element("order_by").Value;
                    var modifier = ConvertToModifierKey(ctrl, false, shift, window);
                    Copy.Add(new CopyKey(header, footer, content, new HotKey(key, modifier), orderBy, order, lowDpsContent, lowDpsTreshold, limitNameLength));
                }
            }
            catch { MessageBox.Show(LP.Your_hotkeys_xml_file_is_invalid); }
        }

        public static ModifierKeys ConvertToModifierKey(bool ctrl, bool alt, bool shift, bool window)
        {
            var modifier = ModifierKeys.None;
            if (ctrl) { modifier |= ModifierKeys.Control; }
            if (alt) { modifier |= ModifierKeys.Alt; }
            if (shift) { modifier |= ModifierKeys.Shift; }
            if (window) { modifier |= ModifierKeys.Win; }
            return modifier;
        }

        public void SaveKey(XDocument xml, string keyName, HotKey keyValue, bool saveAlt = true)
        {
            xml.Root.Add(new XElement(keyName));

            var xmlCtrl = (keyValue.Modifier & ModifierKeys.Control) != ModifierKeys.None;
            var xmlShift = (keyValue.Modifier & ModifierKeys.Shift) != ModifierKeys.None;
            var xmlWindow = (keyValue.Modifier & ModifierKeys.Win) != ModifierKeys.None;
            var xmlAlt = (keyValue.Modifier & ModifierKeys.Alt) != ModifierKeys.None;
            var xmlKey = keyValue.Key;
            xml.Root.Element(keyName).Add(new XElement("ctrl", xmlCtrl.ToString()));
            xml.Root.Element(keyName).Add(new XElement("shift", xmlShift.ToString()));
            xml.Root.Element(keyName).Add(new XElement("window", xmlWindow.ToString()));
            if (saveAlt) { xml.Root.Element(keyName).Add(new XElement("alt", xmlAlt.ToString())); }
            xml.Root.Element(keyName).Add(new XElement("key", xmlKey.ToString()));
        }

        public void Save()
        {
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("hotkeys"));
            SaveKey(xml, "topmost", Topmost);
            SaveKey(xml, "paste", Paste, false);
            SaveKey(xml, "reset", Reset);
            SaveKey(xml, "reset_current", ResetCurrent);
            SaveKey(xml, "excel_save", ExcelSave);
            SaveKey(xml, "click_throu", ClickThrou);

            xml.Root.Add(new XElement("copys"));

            foreach (var copy in Copy)
            {
                var copyElement = new XElement("copy");

                var copyCtrl = (copy.Hotkey.Modifier & ModifierKeys.Control) != ModifierKeys.None;
                var copyShift = (copy.Hotkey.Modifier & ModifierKeys.Shift) != ModifierKeys.None;
                var copyWindow = (copy.Hotkey.Modifier & ModifierKeys.Win) != ModifierKeys.None;
                var copyKey = copy.Hotkey.Key;

                copyElement.Add(new XElement("ctrl", copyCtrl.ToString()));
                copyElement.Add(new XElement("shift", copyShift.ToString()));
                copyElement.Add(new XElement("window", copyWindow.ToString()));
                copyElement.Add(new XElement("key", copyKey.ToString()));
                var stringElement = new XElement("string");

                stringElement.Add(new XElement("header", copy.Header));
                stringElement.Add(new XElement("content", copy.Content));
                stringElement.Add(new XElement("low_dps_content", copy.LowDpsContent));
                stringElement.Add(new XElement("low_dps_threshold", copy.LowDpsThreshold));
                stringElement.Add(new XElement("limit_name_length", copy.LimitNameLength));
                stringElement.Add(new XElement("footer", copy.Footer));
                stringElement.Add(new XElement("order_by", copy.OrderBy));
                stringElement.Add(new XElement("order", copy.Order));
                copyElement.Add(stringElement);

                xml.Root.Element("copys").Add(copyElement);
            }

            if(!_filestream.CanRead)
                _filestream = new FileStream(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "config/hotkeys.xml"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            _filestream.SetLength(0);
            using (var sr = new StreamWriter(_filestream, new UTF8Encoding(true)))
            {
                // File writing as usual
                sr.Write(xml.Declaration + Environment.NewLine + xml);
            }
            _filestream.Close();
        }
    }
}