using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Data
{
    public struct WindowStatus
    {
        public Point Location;
        public bool Visible;
        public double Scale;

        public WindowStatus(Point p, bool v, double s)
        {
            Location = p;
            Visible = v;
            Scale = s;
        }
    }

    public class WindowData
    {
        private readonly FileStream _filestream;
        private readonly XDocument _xml;


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
            catch { return; }

            Parse("lf_delay", "LFDelay");
            Parse("number_of_players_displayed", "NumberOfPlayersDisplayed");
            Parse("meter_user_on_top", "MeterUserOnTop");
            Parse("excel_save_directory", "ExcelSaveDirectory");

            if (ExcelSaveDirectory == "") { ExcelSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShinraMeter/"); }


            Parse("show_crit_damage_rate", "ShowCritDamageRate");
            Parse("showhealcrit", "ShowHealCrit");
            Parse("showtimeleft", "ShowTimeLeft");
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
            Parse("excel_path_template", "ExcelPathTemplate");
            Parse("low_priority", "LowPriority");
            Parse("format_paste_string", "FormatPasteString");
            Parse("remove_tera_alt_enter_hotkey", "RemoveTeraAltEnterHotkey");
            Parse("enable_chat_and_notifications", "EnableChat");
            Parse("copy_inspect", "CopyInspect");
            Parse("excel_cma_dps_seconds", "ExcelCMADPSSeconds");
            Parse("disable_party_event", "DisablePartyEvent");
            Parse("show_afk_events_ingame", "ShowAfkEventsIngame");
            Parse("mute_sound", "MuteSound");
            Parse("idle_reset_timeout", "IdleResetTimeout");
            Parse("no_paste", "NoPaste");
            ParseColor("say_color", "SayColor");
            ParseColor("alliance_color", "AllianceColor");
            ParseColor("area_color", "AreaColor");
            ParseColor("guild_color", "GuildColor");
            ParseColor("whisper_color", "WhisperColor");
            ParseColor("general_color", "GeneralColor");
            ParseColor("group_color", "GroupColor");
            ParseColor("trading_color", "TradingColor");
            ParseColor("emotes_color", "EmotesColor");
            ParseColor("private_channel_color", "PrivateChannelColor");
            PopupNotificationLocation = ParseLocation(_xml.Root, "popup_notification_location");
            Location = ParseLocation(_xml.Root);
            ParseWindowStatus("boss_gage_window", "BossGageStatus");
            ParseWindowStatus("debuff_uptime_window", "DebuffsStatus");
            ParseWindowStatus("upload_history_window", "HistoryStatus");
            ParseOpacity();
            ParseTeraDps();
            ParseLanguage();
            ParseUILanguage();
            Parse("date_in_excel_path", "DateInExcelPath");
            if (DateInExcelPath) { ExcelPathTemplate = "{Area}/{Date}/{Boss} {Time} {User}"; }
        }

        public int LFDelay { get; set; }
        public Point Location { get; set; }
        public Point PopupNotificationLocation { get; set; }
        public WindowStatus BossGageStatus { get; set; }
        public WindowStatus DebuffsStatus { get; set; }
        public WindowStatus HistoryStatus { get; set; }
        public string ExcelSaveDirectory { get; set; }
        public double Scale { get; set; }
        public bool PartyOnly { get; set; }
        public double MainWindowOpacity { get; private set; }
        public double OtherWindowOpacity { get; private set; }
        public bool RememberPosition { get; private set; }
        public string Language { get; private set; }
        public string UILanguage { get; private set; }
        public bool AutoUpdate { get; private set; }
        public bool Winpcap { get; private set; }
        public bool InvisibleUi { get; set; }
        public bool NoPaste { get; set; }
        public bool AllowTransparency { get; set; }
        public string TeraDpsUser { get; private set; }
        public string TeraDpsToken { get; set; }
        public bool AlwaysVisible { get; set; }
        public bool Topmost { get; set; }
        public bool Debug { get; set; }
        public bool Excel { get; set; }
        public int ExcelCMADPSSeconds { get; set; }
        public bool SiteExport { get; set; }
        public bool ShowHealCrit { get; set; }
        public bool ShowCritDamageRate { get; set; }
        public bool OnlyBoss { get; set; }
        public bool DetectBosses { get; set; }

        public string ExcelPathTemplate { get; set; }

        public int NumberOfPlayersDisplayed { get; set; }
        public bool MeterUserOnTop { get; set; }
        public bool LowPriority { get; set; }
        public bool EnableChat { get; set; }
        public bool CopyInspect { get; set; }
        public bool ShowAfkEventsIngame { get; set; }

        public bool DisablePartyEvent { get; set; }

        public Color WhisperColor { get; set; }
        public Color AllianceColor { get; set; }
        public Color AreaColor { get; set; }
        public Color GeneralColor { get; set; }
        public Color GroupColor { get; set; }
        public Color GuildColor { get; set; }
        public Color RaidColor { get; set; }
        public Color SayColor { get; set; }
        public Color TradingColor { get; set; }
        public Color EmotesColor { get; set; }

        public Color PrivateChannelColor { get; set; }
        public bool RemoveTeraAltEnterHotkey { get; set; }
        public bool FormatPasteString { get; set; }
        public bool PrivateServerExport { get; set; }
        public List<string> PrivateDpsServers { get; set; }
        public bool MuteSound { get; set; }
        public int IdleResetTimeout { get; set; }
        public bool DateInExcelPath { get; set; }
        public bool ShowTimeLeft { get; set; }

        private void DefaultValue()
        {
            ShowTimeLeft = false;
            DateInExcelPath = false;
            Location = new Point(0, 0);
            PopupNotificationLocation = new Point(0,0);
            Language = "Auto";
            UILanguage = "Auto";
            MainWindowOpacity = 0.5;
            OtherWindowOpacity = 0.9;
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
            ExcelCMADPSSeconds = 1;
            AlwaysVisible = false;
            Scale = 1;
            PartyOnly = false;
            SiteExport = false;
            ShowHealCrit = true;
            ShowCritDamageRate = false;
            ExcelSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShinraMeter/");
            LFDelay = 150;
            OnlyBoss = false;
            DetectBosses = false;
            ExcelPathTemplate = "{Area}/{Boss} {Date} {Time} {User}";
            MeterUserOnTop = false;
            NumberOfPlayersDisplayed = 5;
            FormatPasteString = true;
            WhisperColor = Brushes.Pink.Color;
            AllianceColor = Brushes.Green.Color;
            AreaColor = Brushes.Purple.Color;
            GeneralColor = Brushes.Yellow.Color;
            GroupColor = Brushes.Cyan.Color;
            GuildColor = Brushes.LightGreen.Color;
            RaidColor = Brushes.Orange.Color;
            SayColor = Brushes.White.Color;
            TradingColor = Brushes.Sienna.Color;
            EmotesColor = Brushes.White.Color;
            PrivateChannelColor = Brushes.Red.Color;
            LowPriority = true;
            RemoveTeraAltEnterHotkey = false;
            EnableChat = true;
            CopyInspect = true;
            ShowAfkEventsIngame = false;
            DisablePartyEvent = false;
            PrivateServerExport = false;
            PrivateDpsServers = new List<string> {""};
            MuteSound = false;
            IdleResetTimeout = 0;
            NoPaste = false;
            BossGageStatus = new WindowStatus(new Point(0, 0), true, 1);
            HistoryStatus = new WindowStatus(new Point(0, 0), false, 1);
            DebuffsStatus = new WindowStatus(new Point(0, 0), false, 1);
        }

        private void ParseWindowStatus(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetProperty(settingName);
            var currentSetting = (WindowStatus) setting.GetValue(this);
            var location = ParseLocation(xml);

            var xmlVisible = xml.Attribute("visible");
            var visibleSuccess = bool.TryParse(xmlVisible?.Value ?? "false", out bool visible);
            var xmlScale = xml.Attribute("scale");
            var scaleSuccess = double.TryParse(xmlScale?.Value ?? "0" , NumberStyles.Float, CultureInfo.InvariantCulture, out double scale);
            setting.SetValue(this, new WindowStatus(location, visibleSuccess ? visible : currentSetting.Visible, scaleSuccess ? scale>0 ? scale : Scale : Scale));
        }

        private void ParseColor(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetProperty(settingName);
            setting.SetValue(this, (Color) ColorConverter.ConvertFromString(xml.Value), null);
        }

        private void ParseTeraDps()
        {
            var root = _xml.Root;
            var teradps = root?.Element("teradps.io");
            var user = teradps?.Element("user");
            if (user == null) { return; }
            var token = teradps.Element("token");
            if (token == null) { return; }

            TeraDpsToken = token.Value;
            TeraDpsUser = user.Value;

            if (TeraDpsToken == null || TeraDpsUser == null)
            {
                TeraDpsToken = "";
                TeraDpsUser = "";
            }
            var exp = teradps.Element("enabled");
            if (exp == null) { return; }
            bool val;
            var parseSuccess = bool.TryParse(exp.Value, out val);
            if (parseSuccess) { SiteExport = val; }
            var privateS = teradps.Element("private_servers");
            if (privateS == null) { return; }
            var exp1 = privateS.Attribute("enabled");
            parseSuccess = bool.TryParse(exp1?.Value ?? "false", out val);
            if (parseSuccess) { PrivateServerExport = val; }
            if (privateS.HasElements) { PrivateDpsServers = new List<string>(); }
            else { return; }
            foreach (var server in privateS.Elements()) { PrivateDpsServers.Add(server.Value); }
        }

        private Point ParseLocation(XElement root, string elementName = "location")
        {
            double x, y;
            var location = root?.Element(elementName);
            if (location == null) { return new Point(); }
            var xElement = location.Element("x");
            var yElement = location.Element("y");
            if (xElement == null || yElement == null) { return new Point(); }

            var xParsed = double.TryParse(xElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
            var yParsed = double.TryParse(yElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
            return xParsed && yParsed ? new Point(x, y) : new Point();
        }

        private void ParseLanguage()
        {
            var root = _xml.Root;
            var languageElement = root?.Element("language");
            if (languageElement == null) { return; }
            Language = languageElement.Value;
            if (!Array.Exists(new[] {"Auto", "EU-EN", "EU-FR", "EU-GER", "NA", "RU", "JP", "TW", "KR"}, s => s.Equals(Language))) { Language = "Auto"; }
        }

        private void ParseUILanguage()
        {
            var root = _xml.Root;
            var languageElement = root?.Element("ui_language");
            if (languageElement == null) { return; }
            UILanguage = languageElement.Value;
            try { CultureInfo.GetCultureInfo(UILanguage); }
            catch { UILanguage = "Auto"; }
        }

        private void ParseOpacity()
        {
            var root = _xml.Root;
            var opacity = root?.Element("opacity");
            var mainWindowElement = opacity?.Element("mainWindow");
            if (mainWindowElement != null)
            {
                int mainWindowOpacity;
                if (int.TryParse(mainWindowElement.Value, out mainWindowOpacity)) { MainWindowOpacity = (double) mainWindowOpacity / 100; }
            }
            var otherWindowElement = opacity?.Element("otherWindow");
            if (otherWindowElement == null) { return; }
            int otherWindowOpacity;
            if (int.TryParse(otherWindowElement.Value, out otherWindowOpacity)) { OtherWindowOpacity = (double) otherWindowOpacity / 100; }
        }

        public void Save()
        {
            if (_filestream == null) { return; }


            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("window"));
            xml.Root.Add(new XElement("location"));
            xml.Root.Element("location").Add(new XElement("x", Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("location").Add(new XElement("y", Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("scale", Scale.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("popup_notification_location"));
            xml.Root.Element("popup_notification_location").Add(new XElement("x", PopupNotificationLocation.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("popup_notification_location").Add(new XElement("y", PopupNotificationLocation.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("boss_gage_window", new XAttribute("visible", BossGageStatus.Visible), new XAttribute("scale", BossGageStatus.Scale)));
            xml.Root.Element("boss_gage_window").Add(new XElement("location"));
            xml.Root.Element("boss_gage_window").Element("location").Add(new XElement("x", BossGageStatus.Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("boss_gage_window").Element("location").Add(new XElement("y", BossGageStatus.Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("debuff_uptime_window", new XAttribute("visible", DebuffsStatus.Visible), new XAttribute("scale", DebuffsStatus.Scale)));
            xml.Root.Element("debuff_uptime_window").Add(new XElement("location"));
            xml.Root.Element("debuff_uptime_window").Element("location").Add(new XElement("x", DebuffsStatus.Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("debuff_uptime_window").Element("location").Add(new XElement("y", DebuffsStatus.Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("upload_history_window", new XAttribute("visible", HistoryStatus.Visible), new XAttribute("scale", HistoryStatus.Scale)));
            xml.Root.Element("upload_history_window").Add(new XElement("location"));
            xml.Root.Element("upload_history_window").Element("location").Add(new XElement("x", HistoryStatus.Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("upload_history_window").Element("location").Add(new XElement("y", HistoryStatus.Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("language", Language));
            xml.Root.Add(new XElement("ui_language", UILanguage));
            xml.Root.Add(new XElement("opacity"));
            xml.Root.Element("opacity").Add(new XElement("mainWindow", MainWindowOpacity * 100));
            xml.Root.Element("opacity").Add(new XElement("otherWindow", OtherWindowOpacity * 100));
            xml.Root.Add(new XElement("autoupdate", AutoUpdate));
            xml.Root.Add(new XElement("remember_position", RememberPosition));
            xml.Root.Add(new XElement("winpcap", Winpcap));
            xml.Root.Add(new XElement("invisible_ui_when_no_stats", InvisibleUi));
            xml.Root.Add(new XElement("allow_transparency", AllowTransparency));
            xml.Root.Add(new XElement("topmost", Topmost));
            xml.Root.Add(new XElement("debug", Debug));
            xml.Root.Add(new XElement("excel", Excel));
            xml.Root.Add(new XElement("excel_path_template", ExcelPathTemplate));
            xml.Root.Add(new XElement("excel_save_directory", ExcelSaveDirectory));
            xml.Root.Add(new XElement("excel_cma_dps_seconds", ExcelCMADPSSeconds));
            xml.Root.Add(new XElement("always_visible", AlwaysVisible));
            xml.Root.Add(new XElement("lf_delay", LFDelay));
            xml.Root.Add(new XElement("partyonly", PartyOnly));
            xml.Root.Add(new XElement("showhealcrit", ShowHealCrit));
            xml.Root.Add(new XElement("showtimeleft", ShowTimeLeft));
            xml.Root.Add(new XElement("show_crit_damage_rate", ShowCritDamageRate));
            xml.Root.Add(new XElement("detect_bosses_only_by_hp_bar", DetectBosses));
            xml.Root.Add(new XElement("only_bosses", OnlyBoss));
            xml.Root.Add(new XElement("low_priority", LowPriority));
            xml.Root.Add(new XElement("number_of_players_displayed", NumberOfPlayersDisplayed));
            xml.Root.Add(new XElement("meter_user_on_top", MeterUserOnTop));
            xml.Root.Add(new XElement("remove_tera_alt_enter_hotkey", RemoveTeraAltEnterHotkey));
            xml.Root.Add(new XElement("enable_chat_and_notifications", EnableChat));
            xml.Root.Add(new XElement("mute_sound", MuteSound));
            xml.Root.Add(new XElement("copy_inspect", CopyInspect));
            xml.Root.Add(new XElement("format_paste_string", FormatPasteString));
            xml.Root.Add(new XElement("say_color", SayColor.ToString()));
            xml.Root.Add(new XElement("alliance_color", AllianceColor.ToString()));
            xml.Root.Add(new XElement("area_color", AreaColor.ToString()));
            xml.Root.Add(new XElement("guild_color", GuildColor.ToString()));
            xml.Root.Add(new XElement("whisper_color", WhisperColor.ToString()));
            xml.Root.Add(new XElement("general_color", GeneralColor.ToString()));
            xml.Root.Add(new XElement("group_color", GroupColor.ToString()));
            xml.Root.Add(new XElement("trading_color", TradingColor.ToString()));
            xml.Root.Add(new XElement("emotes_color", EmotesColor.ToString()));
            xml.Root.Add(new XElement("private_channel_color", PrivateChannelColor.ToString()));
            xml.Root.Add(new XElement("disable_party_event", DisablePartyEvent));
            xml.Root.Add(new XElement("show_afk_events_ingame", ShowAfkEventsIngame));
            xml.Root.Add(new XElement("idle_reset_timeout", IdleResetTimeout));
            xml.Root.Add(new XElement("no_paste", NoPaste));

            xml.Root.Add(new XElement("teradps.io"));
            xml.Root.Element("teradps.io").Add(new XElement("user", TeraDpsUser));
            xml.Root.Element("teradps.io").Add(new XElement("token", TeraDpsToken));
            xml.Root.Element("teradps.io").Add(new XElement("enabled", SiteExport));
            xml.Root.Element("teradps.io").Add(new XElement("private_servers", new XAttribute("enabled", PrivateServerExport)));
            PrivateDpsServers.ForEach(x => xml.Root.Element("teradps.io").Element("private_servers").Add(new XElement("server", x)));

            _filestream.SetLength(0);
            using (var sw = new StreamWriter(_filestream, new UTF8Encoding(true))) { sw.Write(xml.Declaration + Environment.NewLine + xml); }
            _filestream.Close();
        }

        private void Parse(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetProperty(settingName);
            if (setting.PropertyType == typeof(int))
            {
                int value;
                var parseSuccess = int.TryParse(xml.Value, out value);
                if (parseSuccess) { setting.SetValue(this, value, null); }
            }
            if (setting.PropertyType == typeof(double))
            {
                double value;
                var parseSuccess = double.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (parseSuccess) { setting.SetValue(this, value, null); }
            }
            if (setting.PropertyType == typeof(float))
            {
                float value;
                var parseSuccess = float.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (parseSuccess) { setting.SetValue(this, value, null); }
            }
            if (setting.PropertyType == typeof(bool))
            {
                bool value;
                var parseSuccess = bool.TryParse(xml.Value, out value);
                if (parseSuccess) { setting.SetValue(this, value, null); }
            }
            if (setting.PropertyType == typeof(string)) { setting.SetValue(this, xml.Value, null); }
        }
    }
}