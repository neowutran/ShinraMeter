using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace Data
{
    public class WindowData
    {
        private readonly FileStream _filestream;
        private readonly XDocument _xml;
        public int LFDelay { get; set; }
        public Point Location { get; set; }
        public string ExcelSaveDirectory { get; set; }
        public double Scale { get; set; }
        public bool PartyOnly { get; set; }
        public double MainWindowOpacity { get; private set; }
        public double SkillWindowOpacity { get; private set; }
        public bool RememberPosition { get; private set; }
        public string Language { get; private set; }
        public bool AutoUpdate { get; private set; }
        public bool Winpcap { get; private set; }
        public bool InvisibleUi { get; set; }
        public bool AllowTransparency { get; set; }
        public string TeraDpsUser { get; private set; }
        public string TeraDpsToken { get; private set; }
        public bool AlwaysVisible { get; set; }
        public bool Topmost { get; set; }
        public bool Debug { get; set; }
        public bool Excel { get; set; }
        public bool SiteExport { get; set; }
        public bool ShowHealCrit { get; set; }
        public bool OnlyBoss { get; set; }
        public bool DetectBosses { get; set; }
        public string SaveMode { get; set; }

        private void DefaultValue()
        {
            Location = new Point(0, 0);
            Language = "Auto";
            MainWindowOpacity = 0.5;
            SkillWindowOpacity = 0.7;
            AutoUpdate = true;
            RememberPosition = true;
            InvisibleUi = false;
            Winpcap = true;
            Topmost = true;
            AllowTransparency = true;
            Debug = true;
            TeraDpsToken = "";
            TeraDpsUser = "";
            Excel = false;
            AlwaysVisible = false;
            Scale = 1;
            PartyOnly = false;
            SiteExport = true;
            ShowHealCrit = true;
            ExcelSaveDirectory = "";
            LFDelay = 150;
            OnlyBoss = false;
            DetectBosses = false;
            SaveMode = "Standard";
        }


        public WindowData(BasicTeraData basicData)
        {
            DefaultValue();
            // Load XML File
            var windowFile = Path.Combine(basicData.ResourceDirectory, "config/window.xml");

            try
            {
                var attrs = File.GetAttributes(windowFile);
                File.SetAttributes(windowFile, attrs & ~FileAttributes.ReadOnly);
            }
            catch
            {
                //ignore
            }

            try
            {
                _filestream = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _xml = XDocument.Load(_filestream);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                Save();
                _filestream = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch
            {
                return;
            }

            Parse("lf_delay", "LFDelay");
            Parse("excel_save_directory", "ExcelSaveDirectory");
            Parse("showhealcrit", "ShowHealCrit");
            Parse("partyonly", "PartyOnly");
            Parse("excel", "Excel");
            Parse("scale", "Scale");
            Parse("always_visible", "AlwaysVisible");
            Parse("remember_position", "RememberPosition");
            Parse("debug", "Debug");
            Parse("topmost", "Topmost");
            Parse("invisible_ui_when_no_stats", "InvisibleUi");
            Parse("allow_transparency", "AllowTransparency");
            Parse("winpcap", "Winpcap");
            Parse("autoupdate", "AutoUpdate");
            Parse("only_bosses", "OnlyBoss");
            Parse("detect_bosses_only_by_hp_bar", "DetectBosses");
            Parse("excel_save_mode", "SaveMode");
            ParseLocation();
            ParseOpacity();
            ParseTeraDps();
            ParseLanguage();
        }

        private void ParseTeraDps()
        {
            var root = _xml.Root;
            var teradps = root?.Element("teradps.io");
            var user = teradps?.Element("user");
            if (user == null) return;
            var token = teradps.Element("token");
            if (token == null) return;

            TeraDpsToken = token.Value;
            TeraDpsUser = user.Value;

            if (TeraDpsToken == null || TeraDpsUser == null)
            {
                TeraDpsToken = "";
                TeraDpsUser = "";
            }
            var exp = teradps.Element("export");
            if (exp == null) return;
            bool val;
            var parseSuccess = bool.TryParse(exp.Value, out val);
            if (parseSuccess)
            {
                SiteExport = val;
            }
        }

 
        private void ParseLocation()
        {
            int x, y;
            var root = _xml.Root;

            var location = root?.Element("location");
            if (location == null) return;
            var xElement = location.Element("x");
            var yElement = location.Element("y");
            if (xElement == null || yElement == null) return;

            var xParsed = int.TryParse(xElement.Value, out x);
            var yParsed = int.TryParse(yElement.Value, out y);
            if (xParsed && yParsed)
            {
                Location = new Point(x, y);
            }
        }

        private void ParseLanguage()
        {
            var root = _xml.Root;
            var languageElement = root?.Element("language");
            if (languageElement == null) return;
            Language = languageElement.Value;
            if (
                !Array.Exists(new[] {"Auto", "EU-EN", "EU-FR", "EU-GER", "NA", "RU", "JP", "TW", "KR"},
                    s => s.Equals(Language))) Language = "Auto";
        }

        private void ParseOpacity()
        {
            var root = _xml.Root;
            var opacity = root?.Element("opacity");
            var mainWindowElement = opacity?.Element("mainWindow");
            if (mainWindowElement != null)
            {
                int mainWindowOpacity;
                if (int.TryParse(mainWindowElement.Value, out mainWindowOpacity))
                {
                    MainWindowOpacity = (double) mainWindowOpacity/100;
                }
            }
            var skillWindowElement = opacity?.Element("skillWindow");
            if (skillWindowElement == null) return;
            int skillWindowOpacity;
            if (int.TryParse(skillWindowElement.Value, out skillWindowOpacity))
            {
                SkillWindowOpacity = (double)skillWindowOpacity / 100;
            }
        }

        public void Save()
        {
            if (_filestream == null)
            {
                return;
            }


            var xml = new XDocument(new XElement("window"));
            xml.Root.Add(new XElement("location"));
            xml.Root.Element("location").Add(new XElement("x", Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("location").Add(new XElement("y", Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("language", Language));
            xml.Root.Add(new XElement("opacity"));
            xml.Root.Element("opacity").Add(new XElement("mainWindow", MainWindowOpacity*100));
            xml.Root.Element("opacity").Add(new XElement("skillWindow", SkillWindowOpacity*100));
            xml.Root.Add(new XElement("autoupdate", AutoUpdate));
            xml.Root.Add(new XElement("remember_position", RememberPosition));
            xml.Root.Add(new XElement("winpcap", Winpcap));
            xml.Root.Add(new XElement("invisible_ui_when_no_stats", InvisibleUi));
            xml.Root.Add(new XElement("allow_transparency", AllowTransparency));
            xml.Root.Add(new XElement("topmost", Topmost));
            xml.Root.Add(new XElement("teradps.io"));
            xml.Root.Element("teradps.io").Add(new XElement("user", TeraDpsUser));
            xml.Root.Element("teradps.io").Add(new XElement("token", TeraDpsToken));
            xml.Root.Element("teradps.io").Add(new XElement("export", SiteExport));
            xml.Root.Add(new XElement("debug", Debug));
            xml.Root.Add(new XElement("excel", Excel));
            xml.Root.Add(new XElement("excel_save_mode", SaveMode));
            xml.Root.Add(new XElement("excel_save_directory", ExcelSaveDirectory));
            xml.Root.Add(new XElement("always_visible", AlwaysVisible));
            xml.Root.Add(new XElement("scale", Scale));
            xml.Root.Add(new XElement("lf_delay", LFDelay));
            xml.Root.Add(new XElement("partyonly", PartyOnly));
            xml.Root.Add(new XElement("showhealcrit", ShowHealCrit));
            xml.Root.Add(new XElement("detect_bosses_only_by_hp_bar", DetectBosses));
            xml.Root.Add(new XElement("only_bosses", OnlyBoss));
            

            _filestream.SetLength(0);
            using (var sr = new StreamWriter(_filestream))
            {
                // File writing as usual
                sr.Write(xml);
            }
            _filestream.Close();
        }

        private void Parse(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) return;
            var setting = this.GetType().GetProperty(settingName);
            if (setting.PropertyType == typeof(int))
            {
                int value;
                var parseSuccess = int.TryParse(xml.Value, out value);
                if (parseSuccess)
                {
                    setting.SetValue(this, value, null);
                }
            }
            if (setting.PropertyType == typeof(double))
            {
                double value;
                var parseSuccess = double.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (parseSuccess)
                {
                    setting.SetValue(this, value, null);
                }
            }
            if (setting.PropertyType == typeof(bool))
            {
                bool value;
                var parseSuccess = bool.TryParse(xml.Value, out value);
                if (parseSuccess)
                {
                    setting.SetValue(this, value, null);
                }
            }
            if (setting.PropertyType == typeof(string))
            {
                setting.SetValue(this, xml.Value, null);
            }
        }
    }
}