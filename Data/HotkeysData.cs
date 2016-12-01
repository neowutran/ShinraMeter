using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Lang;

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

        private readonly FileStream _filestream;

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
            catch
            {
                return;
            }
            // Get Keys
            var root = xml.Root;
            if (root == null) return;

            var topmostKey = ReadElement(root, "topmost", false);
            if (topmostKey != null)
            {
                Topmost = (KeyValuePair<Keys, ModifierKeys>)topmostKey;
            }

            var pasteKey = ReadElement(root, "paste", false);
            if (pasteKey != null)
            {
                Paste = (KeyValuePair<Keys, ModifierKeys>) pasteKey;
            }

            var resetKey = ReadElement(root, "reset", true);
            if (resetKey != null)
            {
                Reset = (KeyValuePair<Keys, ModifierKeys>) resetKey;
            }

            var activateKey = ReadElement(root, "click_throu", true);
            if (activateKey != null)
            {
                ClickThrou = (KeyValuePair<Keys, ModifierKeys>) activateKey;
            }

            var resetCurrentKey = ReadElement(root, "reset_current", true);
            if (resetCurrentKey != null)
            {
                ResetCurrent = (KeyValuePair<Keys, ModifierKeys>) resetCurrentKey;
            }

            var excelSaveKey = ReadElement(root, "excel_save", true);
            if (excelSaveKey != null)
            {
                ExcelSave = (KeyValuePair<Keys, ModifierKeys>) excelSaveKey;
            }

            Copy = new List<CopyKey>();
            CopyData(xml);
        }

        public List<CopyKey> Copy { get; private set; }
        public KeyValuePair<Keys, ModifierKeys> Topmost { get; private set; }
        public KeyValuePair<Keys, ModifierKeys> Reset { get; private set; }
        public KeyValuePair<Keys, ModifierKeys> Paste { get; private set; }

        public KeyValuePair<Keys, ModifierKeys> ResetCurrent { get; private set; }

        public KeyValuePair<Keys, ModifierKeys> ClickThrou { get; private set; }

        public KeyValuePair<Keys, ModifierKeys> ExcelSave { get; private set; }


        private static KeyValuePair<Keys, ModifierKeys>? ReadElement(XContainer root, string element, bool readAlt)
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
                    var message = LP.Unable_to_get_key_from_string + " " + keyValue + ". "+LP.Your_hotkeys_xml_file_is_invalid;
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
            Topmost = new KeyValuePair<Keys, ModifierKeys>(Keys.None, ModifierKeys.None);
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
                    "descending",
                    @"[{class}] {name}: Hits: {hits_received} = {damage_received}; Death {deaths} = {death_duration} Aggro {aggro} = {aggro_duration}\",
                    4
                    ),
                new CopyKey(
                    @"Damage Done @ {encounter} {timer} {partyDps} {enrage}:\",
                    "",
                    @"[{class}] {name}: {damage_percentage} ({damage_dealt}) | Crit: {crit_rate} | {global_dps}\",
                    ModifierKeys.Shift,
                    Keys.End,
                    "damage_percentage",
                    "descending",
                    @"[{class}] {name}: {debuff_list}\",
                    4
                    )
            };
            ExcelSave = new KeyValuePair<Keys, ModifierKeys>(Keys.PageDown, ModifierKeys.Control);
            ClickThrou = new KeyValuePair<Keys, ModifierKeys>(Keys.PageUp, ModifierKeys.Control);
        }

        private void CopyData(XDocument xml)
        {
            try
            {
                foreach (

                    var copy in xml.Root.Elements("copys").Elements("copy"))
                {
                    bool ctrl, shift, window;

                    bool.TryParse(copy.Element("shift").Value, out shift);
                    bool.TryParse(copy.Element("window").Value, out window);
                    bool.TryParse(copy.Element("ctrl").Value, out ctrl);

                    var header = copy.Element("string").Element("header").Value;
                    var footer = copy.Element("string").Element("footer").Value;
                    var content = copy.Element("string").Element("content").Value;
                    var lowDpsContent = copy.Element("string").Element("low_dps_content")?.Value??content;
                    var lowDpsTreshold = int.Parse(copy.Element("string").Element("low_dps_threshold")?.Value ?? "6");
                    Keys key;
                    var keyValue = copy.Element("key").Value;
                    if (!Enum.TryParse(keyValue, out key))
                    {
                        var message = LP.Unable_to_get_key_from_string+ " " + keyValue + ". "+LP.Your_hotkeys_xml_file_is_invalid;
                        MessageBox.Show(message);
                        continue;
                    }
                    var order = copy.Element("string").Element("order").Value;
                    var orderBy = copy.Element("string").Element("order_by").Value;
                    var modifier = ConvertToModifierKey(ctrl, false, shift, window);
                    Copy.Add(new CopyKey(header, footer, content, modifier, key, orderBy, order, lowDpsContent,lowDpsTreshold));
                }
            }
            catch
            {
                MessageBox.Show(LP.Your_hotkeys_xml_file_is_invalid);
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

        public void SaveKey(XDocument xml, string keyName, KeyValuePair<Keys, ModifierKeys> keyValue, bool saveAlt = true)
        {
            xml.Root.Add(new XElement(keyName));

            var xmlCtrl = (keyValue.Value & ModifierKeys.Control) != ModifierKeys.None;
            var xmlShift = (keyValue.Value & ModifierKeys.Shift) != ModifierKeys.None;
            var xmlWindow = (keyValue.Value & ModifierKeys.Win) != ModifierKeys.None;
            var xmlAlt = (keyValue.Value & ModifierKeys.Alt) != ModifierKeys.None;
            var xmlKey = keyValue.Key;
            xml.Root.Element(keyName).Add(new XElement("ctrl", xmlCtrl.ToString()));
            xml.Root.Element(keyName).Add(new XElement("shift", xmlShift.ToString()));
            xml.Root.Element(keyName).Add(new XElement("window", xmlWindow.ToString()));
            if (saveAlt) xml.Root.Element(keyName).Add(new XElement("alt", xmlAlt.ToString()));
            xml.Root.Element(keyName).Add(new XElement("key", xmlKey.ToString()));
        }

        public void Save()
        {
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("hotkeys"));
            SaveKey(xml, "topmost", Topmost);
            SaveKey(xml, "paste",Paste,false);
            SaveKey(xml, "reset", Reset);
            SaveKey(xml, "reset_current", ResetCurrent);
            SaveKey(xml, "excel_save", ExcelSave);
            SaveKey(xml, "click_throu", ClickThrou);

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
                stringElement.Add(new XElement("low_dps_content", copy.LowDpsContent));
                stringElement.Add(new XElement("low_dps_threshold", copy.LowDpsThreshold));
                stringElement.Add(new XElement("footer", copy.Footer));
                stringElement.Add(new XElement("order_by", copy.OrderBy));
                stringElement.Add(new XElement("order", copy.Order));
                copyElement.Add(stringElement);

                xml.Root.Element("copys").Add(copyElement);
            }

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