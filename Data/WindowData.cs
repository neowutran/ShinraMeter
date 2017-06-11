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
            Parse("no_abnormals_in_hud", "NoAbnormalsInHUD");
            Parse("enable_overlay", "EnableOverlay");
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
            ParseOldDpsServers();
            ParseDpsServers();
            ParseLanguage();
            ParseUILanguage();
            Parse("date_in_excel_path", "DateInExcelPath");
            if (DateInExcelPath) { ExcelPathTemplate = "{Area}/{Date}/{Boss} {Time} {User}"; }
        }

        public bool EnableOverlay = false;
        public Color WhisperColor = Brushes.Pink.Color;
        public Color AllianceColor = Brushes.Green.Color;
        public Color AreaColor = Brushes.Purple.Color;
        public Color GeneralColor = Brushes.Yellow.Color;
        public Color GroupColor = Brushes.Cyan.Color;
        public Color GuildColor = Brushes.LightGreen.Color;
        public Color RaidColor = Brushes.Orange.Color;
        public Color SayColor = Brushes.White.Color;
        public Color TradingColor = Brushes.Sienna.Color;
        public Color EmotesColor = Brushes.White.Color;
        public Color PrivateChannelColor = Brushes.Red.Color;
        public Point Location = new Point(0, 0);
        public Point PopupNotificationLocation = new Point(0, 0);
        public string Language = "Auto";
        public string UILanguage = "Auto";
        public double MainWindowOpacity = 0.5;
        public double OtherWindowOpacity = 0.9;
        public int LFDelay = 150;
        public WindowStatus BossGageStatus = new WindowStatus(new Point(0, 0), true, 1);
        public WindowStatus DebuffsStatus = new WindowStatus(new Point(0, 0), false, 1);
        public WindowStatus HistoryStatus = new WindowStatus(new Point(0, 0), false, 1);
        public string ExcelSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShinraMeter/");
        public double Scale = 1;
        public bool PartyOnly = false;
        public bool RememberPosition = true;
        public bool AutoUpdate = true;
        public bool Winpcap = true;
        public bool InvisibleUi = false;
        public bool NoPaste = false;
        public bool AllowTransparency = true;
        public bool AlwaysVisible = false;
        public bool Topmost = true;
        public bool Debug = true;
        public bool Excel = false;
        public int ExcelCMADPSSeconds = 1;
        public bool ShowHealCrit = true;
        public bool ShowCritDamageRate = false;
        public bool OnlyBoss = false;
        public bool DetectBosses = false;

        public List<DpsServerData> DpsServers = new List<DpsServerData> { DpsServerData.Moongourd, DpsServerData.TeraLogs };
        public List<int> WhiteListAreaId = new List<int>();

        public string ExcelPathTemplate = "{Area}/{Boss} {Date} {Time} {User}";
        public int NumberOfPlayersDisplayed = 5;
        public bool MeterUserOnTop = false;
        public bool LowPriority = true;
        public bool EnableChat = true;
        public bool CopyInspect = true;
        public bool ShowAfkEventsIngame = false;
        public bool DisablePartyEvent = false;
        public bool RemoveTeraAltEnterHotkey = true;
        public bool FormatPasteString = true;
        public bool MuteSound = false;
        public int IdleResetTimeout = 0;
        public bool DateInExcelPath = false;
        public bool ShowTimeLeft = false;
        public bool NoAbnormalsInHUD = false;

        private void ParseWindowStatus(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetField(settingName);
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
            var setting = GetType().GetField(settingName);
            setting.SetValue(this, (Color) ColorConverter.ConvertFromString(xml.Value));
        }


        private void ParseOldDpsServers()
        {
            var root = _xml.Root;
            var teradps = root.Element("teradps.io");
            if(teradps == null) { return; }
            var token = teradps.Element("token");
            if (token == null) { return; }
            DpsServerData.Moongourd.Token = token.Value;
            var exp = teradps.Element("enabled");
            if (exp == null) { return; }
            var parseSuccess = bool.TryParse(exp.Value, out bool val);
            DpsServerData.Moongourd.Enabled = val;
            DpsServerData.TeraLogs.Enabled = val;
        }


        private void ParseDpsServers()
        {
            var root = _xml.Root;
            var teradps = root.Element("dps_servers");
            if(teradps == null) { return; }
            foreach(var server in teradps.Elements())
            {
                var username = server.Element("username");
                var token = server.Element("token");
                var enabled = server.Element("enabled");
                var uploadUrl = server.Element("upload_url");
                var allowedAreaUrl = server.Element("allowed_area_url");
                var parseSuccess = bool.TryParse(enabled?.Value ?? "false", out bool enabledBool);
                if(uploadUrl == null || String.IsNullOrWhiteSpace(uploadUrl.Value)) { continue; }
                DpsServerData serverData = new DpsServerData()
                {
                    UploadUrl = new Uri(uploadUrl.Value),
                    AllowedAreaUrl = new Uri(allowedAreaUrl?.Value ?? ""),
                    Enabled = enabledBool,
                    Username = username?.Value ?? "",
                    Token = token?.Value ?? ""
                };
            }
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
            xml.Root.Add(new XElement("no_abnormals_in_hud", NoAbnormalsInHUD));
            xml.Root.Add(new XElement("enable_overlay", EnableOverlay));

            xml.Root.Add(new XElement("dps_servers"));
            foreach(var server in DpsServers)
            {
                var serverXml = new XElement("server");
                serverXml.Add(new XElement("username", server.Username));
                serverXml.Add(new XElement("token", server.Token));
                serverXml.Add(new XElement("enabled", server.Enabled));
                serverXml.Add(new XElement("upload_url", server.UploadUrl));
                serverXml.Add(new XElement("allowed_area_url", server.AllowedAreaUrl));
                xml.Root.Element("dps_servers").Add(serverXml);
            }

            _filestream.SetLength(0);
            using (var sw = new StreamWriter(_filestream, new UTF8Encoding(true))) { sw.Write(xml.Declaration + Environment.NewLine + xml); }
            _filestream.Close();
        }

        private void Parse(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetField(settingName);
            if (setting.FieldType == typeof(int))
            {
                int value;
                var parseSuccess = int.TryParse(xml.Value, out value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(double))
            {
                double value;
                var parseSuccess = double.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(float))
            {
                float value;
                var parseSuccess = float.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(bool))
            {
                bool value;
                var parseSuccess = bool.TryParse(xml.Value, out value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(string)) { setting.SetValue(this, xml.Value); }
        }
    }
}