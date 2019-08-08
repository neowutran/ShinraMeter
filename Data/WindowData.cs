using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private FileStream _filestream;
        private readonly XDocument _xml;
        private readonly String _windowFile;


        public WindowData(BasicTeraData basicData)
        {

            // Load XML File
            _windowFile = Path.Combine(basicData.ResourceDirectory, "config/window.xml");

            try
            {
                var attrs = File.GetAttributes(_windowFile);
                File.SetAttributes(_windowFile, attrs & ~FileAttributes.ReadOnly);
            }
            catch
            {
                //ignore
            }

            try
            {
                _filestream = new FileStream(_windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _xml = XDocument.Load(_filestream);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                Save();
                return;
            }
            catch { return; }
            Parse("lf_delay", "lFDelay");
            Parse("number_of_players_displayed", "numberOfPlayersDisplayed");
            Parse("meter_user_on_top", "meterUserOnTop");
            Parse("excel_save_directory", "excelSaveDirectory");

            if (excelSaveDirectory == "") { excelSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShinraMeter/"); }
            Parse("display_only_boss_hit_by_meter_user", "displayOnlyBossHitByMeterUser");
            Parse("show_crit_damage_rate", "showCritDamageRate");
            Parse("showhealcrit", "showHealCrit");
            Parse("showtimeleft", "showTimeLeft");
            Parse("partyonly", "partyOnly");
            Parse("excel", "excel");
            Parse("scale", "scale");
            Parse("always_visible", "alwaysVisible");
            Parse("remember_position", "rememberPosition");
            Parse("debug", "debug");
            Parse("topmost", "topmost");
            Parse("invisible_ui_when_no_stats", "invisibleUi");
            Parse("allow_transparency", "allowTransparency");
            Parse("winpcap", "winpcap");
            Parse("autoupdate", "autoUpdate");
            Parse("only_bosses", "onlyBoss");
            Parse("detect_bosses_only_by_hp_bar", "detectBosses");
            Parse("excel_path_template", "excelPathTemplate");
            Parse("low_priority", "lowPriority");
            Parse("format_paste_string", "formatPasteString");
            Parse("remove_tera_alt_enter_hotkey", "removeTeraAltEnterHotkey");
            Parse("enable_chat_and_notifications", "enableChat");
            Parse("copy_inspect", "copyInspect");
            Parse("excel_cma_dps_seconds", "excelCMADPSSeconds");
            Parse("disable_party_event", "disablePartyEvent");
            Parse("show_afk_events_ingame", "showAfkEventsIngame");
            Parse("mute_sound", "muteSound");
            Parse("idle_reset_timeout", "idleResetTimeout");
            Parse("no_paste", "noPaste");
            Parse("no_abnormals_in_hud", "noAbnormalsInHUD");
            Parse("enable_overlay", "enableOverlay");
            Parse("click_throu", "clickThrou");
            Parse("packets_collect", "packetsCollect");
            Parse("display_timer_based_on_aggro", "displayTimerBasedOnAggro");
            Parse("max_tts_size", "maxTTSSize");
            Parse("tts_size_exceeded_truncate", "ttsSizeExceededTruncate");

            ParseColor("say_color", "sayColor");
            ParseColor("alliance_color", "allianceColor");
            ParseColor("area_color", "areaColor");
            ParseColor("guild_color", "guildColor");
            ParseColor("whisper_color", "whisperColor");
            ParseColor("general_color", "generalColor");
            ParseColor("group_color", "groupColor");
            ParseColor("trading_color", "tradingColor");
            ParseColor("emotes_color", "emotesColor");
            ParseColor("private_channel_color", "privateChannelColor");
            ParseColor("stat_dps_color", nameof(_dpsColor));
            ParseColor("stat_healer_color", nameof(_healerColor));
            ParseColor("stat_tank_color", nameof(_tankColor));
            ParseColor("stat_player_color", nameof(_playerColor));
            PopupNotificationLocation = ParseLocation(_xml.Root, "popup_notification_location");
            Location = ParseLocation(_xml.Root);
            ParseWindowStatus("boss_gage_window", "bossGageStatus");
            ParseWindowStatus("debuff_uptime_window", "debuffsStatus");
            ParseWindowStatus("upload_history_window", "historyStatus");
            ParseOpacity();
            ParseOldDpsServers();
            ParseDpsServers();
            ParseLanguage();
            ParseUILanguage();
            ParseRichPresence();
            ParseRealtimeGraph();

            Parse("date_in_excel_path", "dateInExcelPath");
            if (dateInExcelPath) { excelPathTemplate = "{Area}/{Date}/{Boss} {Time} {User}"; }
            DpsServers.CollectionChanged += DpsServers_CollectionChanged;
        }

        private void DpsServers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Save();
        }

        private bool enableOverlay = false;
        private Color whisperColor = Brushes.Pink.Color;
        private Color allianceColor = Brushes.Green.Color;
        private Color areaColor = Brushes.Purple.Color;
        private Color generalColor = Brushes.Yellow.Color;
        private Color groupColor = Brushes.Cyan.Color;
        private Color guildColor = Brushes.LightGreen.Color;
        private Color raidColor = Brushes.Orange.Color;
        private Color sayColor = Brushes.White.Color;
        private Color tradingColor = Brushes.Sienna.Color;
        private Color emotesColor = Brushes.White.Color;
        private Color privateChannelColor = Brushes.Red.Color;

        private Color _dpsColor = Color.FromArgb(255, 255, 68, 102);
        private Color _playerColor = Color.FromArgb(255, 244, 164, 66);
        private Color _tankColor = Color.FromArgb(255, 68, 178, 252);
        private Color _healerColor = Color.FromArgb(255, 59, 226, 75);

        private Point location = new Point(0, 0);
        private Point popupNotificationLocation = new Point(0, 0);
        private string language = "Auto";
        private string uILanguage = "Auto";
        private double mainWindowOpacity = 0.5;
        private int maxTTSSize = 40;
        private bool ttsSizeExceededTruncate = true;
        private double otherWindowOpacity = 0.9;
        private int lFDelay = 150;
        private WindowStatus bossGageStatus = new WindowStatus(new Point(0, 0), true, 1);
        private WindowStatus debuffsStatus = new WindowStatus(new Point(0, 0), false, 1);
        private WindowStatus historyStatus = new WindowStatus(new Point(0, 0), false, 1);
        private string excelSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShinraMeter/");
        private double scale = 1;
        private bool partyOnly = false;
        private bool rememberPosition = true;
        private bool autoUpdate = true;
        private bool winpcap = true;
        private bool invisibleUi = false;
        private bool noPaste = false;
        private bool allowTransparency = true;
        private bool alwaysVisible = false;
        private bool topmost = true;
        private bool debug = true;
        private bool excel = false;
        private int excelCMADPSSeconds = 1;
        private bool showHealCrit = true;
        private bool showCritDamageRate = false;
        private bool onlyBoss = false;
        private bool detectBosses = false;
        private bool displayOnlyBossHitByMeterUser = false;
        private bool clickThrou = false;
        private bool packetsCollect = false;

        public ObservableCollection<DpsServerData> DpsServers = new ObservableCollection<DpsServerData> { DpsServerData.Moongourd };
        private List<int> blackListAreaId = new List<int>();

        private string excelPathTemplate = "{Area}/{Boss} {Date} {Time} {User}";
        private int numberOfPlayersDisplayed = 5;
        private bool meterUserOnTop = false;
        private bool lowPriority = true;
        private bool enableChat = true;
        private bool copyInspect = true;
        private bool showAfkEventsIngame = false;
        private bool disablePartyEvent = false;
        private bool removeTeraAltEnterHotkey = true;
        private bool formatPasteString = true;
        private bool muteSound = false;
        private int idleResetTimeout = 0;
        private bool dateInExcelPath = false;
        private bool showTimeLeft = false;
        private bool noAbnormalsInHUD = false;
        private bool _userPaused = false;
        private bool displayTimerBasedOnAggro = true;

        private bool enableRichPresence = true;
        private bool richPresenceShowLocation = true;
        private bool richPresenceShowCharacter = false;
        private bool richPresenceShowStatus = true;
        private bool richPresenceShowParty = true;
        private bool realtimeGraphEnabled = false;
        private int realtimeGraphDisplayedInterval = 120;
        private int realtimeGraphCMAseconds = 10;

        public bool DisplayTimerBasedOnAggro { get => displayTimerBasedOnAggro; set { displayTimerBasedOnAggro = value; Save(); } }

        public bool EnableOverlay { get => enableOverlay; set { enableOverlay = value; Save(); } }
        public Color WhisperColor { get => whisperColor; set { whisperColor = value; Save(); } }
        public Color AllianceColor { get => allianceColor; set { allianceColor = value; Save(); } }
        public Color AreaColor { get => areaColor; set { areaColor = value; Save(); } }
        public Color GeneralColor { get => generalColor; set { generalColor = value; Save(); } }
        public Color GroupColor { get => groupColor; set { groupColor = value; Save(); } }
        public Color GuildColor { get => guildColor; set { guildColor = value; Save(); } }
        public Color RaidColor { get => raidColor; set { raidColor = value; Save(); } }
        public Color SayColor { get => sayColor; set { sayColor = value; Save(); } }
        public Color TradingColor { get => tradingColor; set { tradingColor = value; Save(); } }
        public Color EmotesColor { get => emotesColor; set { emotesColor = value; Save(); } }
        public Color PrivateChannelColor { get => privateChannelColor; set { privateChannelColor = value; Save(); } }
        public Point Location { get => location; set { location = value; Save(); } }
        public Point PopupNotificationLocation { get => popupNotificationLocation; set { popupNotificationLocation = value; Save(); } }
        public string Language { get => language; set { language = value; Save(); } }
        public string UILanguage { get => uILanguage; set { uILanguage = value; Save(); } }
        public double MainWindowOpacity { get => mainWindowOpacity; set { mainWindowOpacity = value; Save(); } }
        public double OtherWindowOpacity { get => otherWindowOpacity; set { otherWindowOpacity = value; Save(); } }
        public int LFDelay { get => lFDelay; set { lFDelay = value; Save(); } }
        public WindowStatus BossGageStatus { get => bossGageStatus; set { bossGageStatus = value; Save(); } }
        public WindowStatus DebuffsStatus { get => debuffsStatus; set { debuffsStatus = value; Save(); } }
        public WindowStatus HistoryStatus { get => historyStatus; set { historyStatus = value; Save(); } }
        public string ExcelSaveDirectory { get => excelSaveDirectory; set { excelSaveDirectory = value; Save(); } }
        public double Scale { get => scale; set { scale = value; Save(); } }
        public bool PartyOnly { get => partyOnly; set { partyOnly = value; Save(); } }
        public bool RememberPosition { get => rememberPosition; set { rememberPosition = value; Save(); } }
        public bool AutoUpdate { get => autoUpdate; set { autoUpdate = value; Save(); } }
        public bool Winpcap { get => winpcap; set { winpcap = value; Save(); } }
        public bool InvisibleUi { get => invisibleUi; set { invisibleUi = value; Save(); } }
        public bool NoPaste { get => noPaste; set { noPaste = value; Save(); } }
        public int MaxTTSSize { get => maxTTSSize; set { maxTTSSize = value; Save(); } }
        public bool TTSSizeExceededTruncate { get => ttsSizeExceededTruncate; set { ttsSizeExceededTruncate = value; Save(); } }

        public bool AllowTransparency { get => allowTransparency; set { allowTransparency = value; Save(); } }
        public bool AlwaysVisible { get => alwaysVisible; set { alwaysVisible = value; Save(); } }
        public bool Topmost { get => topmost; set { topmost = value; Save(); } }
        public bool Debug { get => debug; set { debug = value; Save(); } }
        public bool Excel { get => excel; set { excel = value; Save(); } }
        public int ExcelCMADPSSeconds { get => excelCMADPSSeconds; set { excelCMADPSSeconds = value; Save(); } }
        public bool ShowHealCrit { get => showHealCrit; set { showHealCrit = value; Save(); } }
        public bool ShowCritDamageRate { get => showCritDamageRate; set { showCritDamageRate = value; Save(); } }
        public bool OnlyBoss { get => onlyBoss; set { onlyBoss = value; Save(); } }
        public bool DetectBosses { get => detectBosses; set { detectBosses = value; Save(); } }
        public bool DisplayOnlyBossHitByMeterUser { get => displayOnlyBossHitByMeterUser; set { displayOnlyBossHitByMeterUser = value; Save(); } }
        public bool ClickThrou { get => clickThrou; set { clickThrou = value; Save(); } }
        public bool PacketsCollect { get => packetsCollect; set { packetsCollect = value; Save(); } }

        public List<int> BlackListAreaId { get => blackListAreaId; set { blackListAreaId = value; Save(); } }
        public string ExcelPathTemplate { get => excelPathTemplate; set { excelPathTemplate = value; Save(); } }
        public int NumberOfPlayersDisplayed { get => numberOfPlayersDisplayed; set { numberOfPlayersDisplayed = value; Save(); } }
        public bool MeterUserOnTop { get => meterUserOnTop; set { meterUserOnTop = value; Save(); } }
        public bool LowPriority { get => lowPriority; set { lowPriority = value; Save(); } }
        public bool EnableChat { get => enableChat; set { enableChat = value; Save(); } }
        public bool CopyInspect { get => copyInspect; set { copyInspect = value; Save(); } }
        public bool ShowAfkEventsIngame { get => showAfkEventsIngame; set { showAfkEventsIngame = value; Save(); } }
        public bool DisablePartyEvent { get => disablePartyEvent; set { disablePartyEvent = value; Save(); } }
        public bool RemoveTeraAltEnterHotkey { get => removeTeraAltEnterHotkey; set { removeTeraAltEnterHotkey = value; Save(); } }
        public bool FormatPasteString { get => formatPasteString; set { formatPasteString = value; Save(); } }
        public bool MuteSound { get => muteSound; set { muteSound = value; Save(); } }
        public int IdleResetTimeout { get => idleResetTimeout; set { idleResetTimeout = value; Save(); } }
        public bool DateInExcelPath { get => dateInExcelPath; set { dateInExcelPath = value; Save(); } }
        public bool ShowTimeLeft { get => showTimeLeft; set { showTimeLeft = value; Save(); } }
        public bool NoAbnormalsInHUD { get => noAbnormalsInHUD; set { noAbnormalsInHUD = value; Save(); } }

        public bool UserPaused { get => _userPaused; set { _userPaused = value; } }

        public bool EnableRichPresence { get => enableRichPresence; set { enableRichPresence = value; Save(); } }
        public bool RichPresenceShowLocation { get => richPresenceShowLocation; set { richPresenceShowLocation = value; Save(); } }
        public bool RichPresenceShowCharacter { get => richPresenceShowCharacter; set { richPresenceShowCharacter = value; Save(); } }
        public bool RichPresenceShowStatus { get => richPresenceShowStatus; set { richPresenceShowStatus = value; Save(); } }
        public bool RichPresenceShowParty { get => richPresenceShowParty; set { richPresenceShowParty = value; Save(); } }

        public bool RealtimeGraphEnabled { get => realtimeGraphEnabled; set { realtimeGraphEnabled = value; Save(); } }
        public int RealtimeGraphDisplayedInterval { get => realtimeGraphDisplayedInterval; set { realtimeGraphDisplayedInterval = value; Save(); } }
        public int RealtimeGraphCMAseconds { get => realtimeGraphCMAseconds; set { realtimeGraphCMAseconds = value; Save(); } }

        public Color DpsColor
        {
            get => _dpsColor;
            set
            {
                _dpsColor = value;
                Save();
            }
        }
        public Color PlayerColor
        {
            get => _playerColor;
            set
            {
                _playerColor = value;
                Save();
            }
        }
        public Color TankColor
        {
            get => _tankColor;
            set
            {
                _tankColor = value;
                Save();
            }
        }
        public Color HealerColor
        {
            get => _healerColor;
            set
            {
                _healerColor = value;
                Save();
            }
        }

        private void ParseWindowStatus(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetField(settingName, BindingFlags.NonPublic | BindingFlags.Instance);
            var currentSetting = (WindowStatus)setting.GetValue(this);
            var location = ParseLocation(xml);

            var xmlVisible = xml.Attribute("visible");
            var visibleSuccess = bool.TryParse(xmlVisible?.Value ?? "false", out bool visible);
            var xmlScale = xml.Attribute("scale");
            var scaleSuccess = double.TryParse(xmlScale?.Value ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out double scale);
            setting.SetValue(this, new WindowStatus(location, visibleSuccess ? visible : currentSetting.Visible, scaleSuccess ? scale > 0 ? scale : scale : scale));
        }

        private void ParseColor(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetField(settingName, BindingFlags.NonPublic | BindingFlags.Instance);
            setting.SetValue(this, (Color)ColorConverter.ConvertFromString(xml.Value));
        }


        private void ParseOldDpsServers()
        {
            var root = _xml.Root;
            var teradps = root.Element("teradps.io");
            if (teradps == null) { return; }
            var token = teradps.Element("token");
            if (token != null) { DpsServerData.Moongourd.Token = token.Value; }
            var exp = teradps.Element("enabled");
            if (exp != null)
            {
                var parseSuccess = bool.TryParse(exp.Value, out bool val);
                DpsServerData.Moongourd.Enabled = val;
            }
            var privateS = teradps.Element("private_servers");
            if (privateS == null) { return; }
            var exp1 = privateS.Attribute("enabled");
            var parseS = bool.TryParse(exp1?.Value ?? "false", out bool enabled);
            if (!parseS || !privateS.HasElements) { return; }
            foreach (var server in privateS.Elements())
            {
                if (String.IsNullOrWhiteSpace(server?.Value)) { continue; }
                DpsServerData serverData = new DpsServerData(new Uri(server.Value), null, null, null, null, enabled);
                DpsServers.Add(serverData);
            }
        }


        private void ParseDpsServers()
        {
            var root = _xml.Root;
            var teradps = root.Element("dps_servers");
            if (teradps == null) { return; }
            DpsServers = new ObservableCollection<DpsServerData>();
            if (teradps.Elements("server") == null) { return; }
            foreach (var server in teradps.Elements("server"))
            {
                var username = server.Element("username");
                var token = server.Element("token");
                var enabled = server.Element("enabled");
                var uploadUrl = server.Element("dps_url");
                var allowedAreaUrl = server.Element("allowed_area_url");
                var glyphUrl = server.Element("glyph_url");
                var parseSuccess = bool.TryParse(enabled?.Value ?? "false", out bool enabledBool);
                if (String.IsNullOrWhiteSpace(uploadUrl?.Value)) { continue; }
                DpsServerData serverData = new DpsServerData(
                    new Uri(uploadUrl.Value),
                    String.IsNullOrWhiteSpace(allowedAreaUrl?.Value) ? null : new Uri(allowedAreaUrl.Value),
                    String.IsNullOrWhiteSpace(glyphUrl?.Value) ? null : new Uri(glyphUrl.Value),
                    username?.Value ?? null, token?.Value ?? null, enabledBool
                );
                DpsServers.Add(serverData);
            }

            if (teradps.Element("blacklist") == null) { return; }
            if (teradps.Element("blacklist").Elements("id") == null) { return; }
            foreach (var blacklistedAreaId in teradps.Element("blacklist").Elements("id"))
            {
                var parseSuccess = int.TryParse(blacklistedAreaId.Value, out int areaId);
                if (parseSuccess) { blacklistedAreaId.Add(areaId); }
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
            language = languageElement.Value;
            if (!Array.Exists(new[] { "Auto", "EU-EN", "EU-FR", "EU-GER", "EUC-EN", "EUC-FR", "EUC-GER", "NA", "RU", "JP", "JPC", "TW", "KR", "KRC", "KR-PTS" }, s => s.Equals(language))) { language = "Auto"; }
        }

        private void ParseUILanguage()
        {
            var root = _xml.Root;
            var languageElement = root?.Element("ui_language");
            if (languageElement == null) { return; }
            uILanguage = languageElement.Value;
            try { CultureInfo.GetCultureInfo(uILanguage); }
            catch { uILanguage = "Auto"; }
        }

        private void ParseOpacity()
        {
            var root = _xml.Root;
            var opacity = root?.Element("opacity");
            var mainWindowElement = opacity?.Element("mainWindow");
            if (mainWindowElement != null)
            {
                int mainWindowOpacity;
                if (int.TryParse(mainWindowElement.Value, out mainWindowOpacity)) { this.mainWindowOpacity = (double)mainWindowOpacity / 100; }
            }
            var otherWindowElement = opacity?.Element("otherWindow");
            if (otherWindowElement == null) { return; }
            if (int.TryParse(otherWindowElement.Value, out int otherWindowOpacity)) { this.otherWindowOpacity = (double)otherWindowOpacity / 100; }
        }
        private void ParseRealtimeGraph()
        {
            var root = _xml.Root;
            var rg = root?.Element("realtime_graph");
            rg?.Descendants().ToList().ForEach(e =>
            {
                if (e.Name == "enabled")
                {
                    if (bool.TryParse(e.Value, out var enabled)) realtimeGraphEnabled = enabled;
                }
                else if (e.Name == "displayed_interval")
                {
                    if (int.TryParse(e.Value, out var interval)) realtimeGraphDisplayedInterval = interval;
                }
                else if (e.Name == "cma_seconds")
                {
                    if (int.TryParse(e.Value, out var cma)) realtimeGraphCMAseconds = cma;
                }
            });
        }
        private void ParseRichPresence()
        {
            var root = _xml.Root;
            var rp = root?.Element("rich_presence");
            var enabled = rp?.Element("enabled");
            if (enabled != null)
            {
                if (bool.TryParse(enabled.Value, out var enableRichPresence)) { this.enableRichPresence = enableRichPresence; }
            }

            var showLocation = rp?.Element("show_location");
            if (showLocation != null)
            {
                if (bool.TryParse(showLocation.Value, out var richPresenceShowLocation)) { this.richPresenceShowLocation = richPresenceShowLocation; }
            }

            var showCharacter = rp?.Element("show_character");
            if (showCharacter != null)
            {
                if (bool.TryParse(showCharacter.Value, out var richPresenceShowCharacter)) { this.richPresenceShowCharacter = richPresenceShowCharacter; }
            }

            var showStatus = rp?.Element("show_status");
            if (showStatus != null)
            {
                if (bool.TryParse(showStatus.Value, out var richPresenceShowStatus)) { this.richPresenceShowStatus = richPresenceShowStatus; }
            }

            var showParty = rp?.Element("show_party");
            if (showParty != null)
            {
                if (bool.TryParse(showParty.Value, out var richPresenceShowParty)) { this.richPresenceShowParty = richPresenceShowParty; }
            }
        }

        public void Save()
        {
            if (_filestream == null) { return; }


            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("window"));
            xml.Root.Add(new XElement("location"));
            xml.Root.Element("location").Add(new XElement("x", location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("location").Add(new XElement("y", location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("scale", scale.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("popup_notification_location"));
            xml.Root.Element("popup_notification_location").Add(new XElement("x", popupNotificationLocation.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("popup_notification_location").Add(new XElement("y", popupNotificationLocation.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("boss_gage_window", new XAttribute("visible", bossGageStatus.Visible), new XAttribute("scale", BossGageStatus.Scale)));
            xml.Root.Element("boss_gage_window").Add(new XElement("location"));
            xml.Root.Element("boss_gage_window").Element("location").Add(new XElement("x", bossGageStatus.Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("boss_gage_window").Element("location").Add(new XElement("y", bossGageStatus.Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("debuff_uptime_window", new XAttribute("visible", debuffsStatus.Visible), new XAttribute("scale", DebuffsStatus.Scale)));
            xml.Root.Element("debuff_uptime_window").Add(new XElement("location"));
            xml.Root.Element("debuff_uptime_window").Element("location").Add(new XElement("x", debuffsStatus.Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("debuff_uptime_window").Element("location").Add(new XElement("y", debuffsStatus.Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("upload_history_window", new XAttribute("visible", historyStatus.Visible), new XAttribute("scale", HistoryStatus.Scale)));
            xml.Root.Element("upload_history_window").Add(new XElement("location"));
            xml.Root.Element("upload_history_window").Element("location").Add(new XElement("x", historyStatus.Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("upload_history_window").Element("location").Add(new XElement("y", historyStatus.Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("language", language));
            xml.Root.Add(new XElement("ui_language", uILanguage));
            xml.Root.Add(new XElement("opacity"));
            xml.Root.Element("opacity").Add(new XElement("mainWindow", mainWindowOpacity * 100));
            xml.Root.Element("opacity").Add(new XElement("otherWindow", otherWindowOpacity * 100));
            xml.Root.Add(new XElement("autoupdate", autoUpdate));
            xml.Root.Add(new XElement("remember_position", rememberPosition));
            xml.Root.Add(new XElement("winpcap", winpcap));
            xml.Root.Add(new XElement("invisible_ui_when_no_stats", invisibleUi));
            xml.Root.Add(new XElement("allow_transparency", allowTransparency));
            xml.Root.Add(new XElement("topmost", topmost));
            xml.Root.Add(new XElement("debug", debug));
            xml.Root.Add(new XElement("excel", excel));
            xml.Root.Add(new XElement("excel_path_template", excelPathTemplate));
            xml.Root.Add(new XElement("excel_save_directory", excelSaveDirectory));
            xml.Root.Add(new XElement("excel_cma_dps_seconds", excelCMADPSSeconds));
            xml.Root.Add(new XElement("always_visible", alwaysVisible));
            xml.Root.Add(new XElement("lf_delay", lFDelay));
            xml.Root.Add(new XElement("partyonly", partyOnly));
            xml.Root.Add(new XElement("showhealcrit", showHealCrit));
            xml.Root.Add(new XElement("showtimeleft", showTimeLeft));
            xml.Root.Add(new XElement("show_crit_damage_rate", showCritDamageRate));
            xml.Root.Add(new XElement("detect_bosses_only_by_hp_bar", detectBosses));
            xml.Root.Add(new XElement("only_bosses", onlyBoss));
            xml.Root.Add(new XElement("low_priority", lowPriority));
            xml.Root.Add(new XElement("number_of_players_displayed", numberOfPlayersDisplayed));
            xml.Root.Add(new XElement("meter_user_on_top", meterUserOnTop));
            xml.Root.Add(new XElement("remove_tera_alt_enter_hotkey", removeTeraAltEnterHotkey));
            xml.Root.Add(new XElement("enable_chat_and_notifications", enableChat));
            xml.Root.Add(new XElement("mute_sound", muteSound));
            xml.Root.Add(new XElement("click_throu", clickThrou));
            xml.Root.Add(new XElement("copy_inspect", copyInspect));
            xml.Root.Add(new XElement("format_paste_string", formatPasteString));
            xml.Root.Add(new XElement("say_color", sayColor.ToString()));
            xml.Root.Add(new XElement("alliance_color", allianceColor.ToString()));
            xml.Root.Add(new XElement("area_color", areaColor.ToString()));
            xml.Root.Add(new XElement("guild_color", guildColor.ToString()));
            xml.Root.Add(new XElement("whisper_color", whisperColor.ToString()));
            xml.Root.Add(new XElement("general_color", generalColor.ToString()));
            xml.Root.Add(new XElement("group_color", groupColor.ToString()));
            xml.Root.Add(new XElement("trading_color", tradingColor.ToString()));
            xml.Root.Add(new XElement("emotes_color", emotesColor.ToString()));
            xml.Root.Add(new XElement("private_channel_color", privateChannelColor.ToString()));
            xml.Root.Add(new XElement("stat_dps_color", _dpsColor.ToString()));
            xml.Root.Add(new XElement("stat_tank_color", _tankColor.ToString()));
            xml.Root.Add(new XElement("stat_healer_color", _healerColor.ToString()));
            xml.Root.Add(new XElement("stat_player_color", _playerColor.ToString()));
            xml.Root.Add(new XElement("disable_party_event", disablePartyEvent));
            xml.Root.Add(new XElement("show_afk_events_ingame", showAfkEventsIngame));
            xml.Root.Add(new XElement("idle_reset_timeout", idleResetTimeout));
            xml.Root.Add(new XElement("no_paste", noPaste));
            xml.Root.Add(new XElement("packets_collect", packetsCollect));
            xml.Root.Add(new XElement("no_abnormals_in_hud", noAbnormalsInHUD));
            xml.Root.Add(new XElement("enable_overlay", enableOverlay));
            xml.Root.Add(new XElement("display_timer_based_on_aggro", displayTimerBasedOnAggro));
            xml.Root.Add(new XElement("display_only_boss_hit_by_meter_user", displayOnlyBossHitByMeterUser));
            xml.Root.Add(new XElement("max_tts_size", maxTTSSize));
            xml.Root.Add(new XElement("tts_size_exceeded_truncate", ttsSizeExceededTruncate));
            xml.Root.Add(new XElement("realtime_graph"));
            xml.Root.Element("realtime_graph").Add(new XElement("enabled", realtimeGraphEnabled));
            xml.Root.Element("realtime_graph").Add(new XElement("displayed_interval", realtimeGraphDisplayedInterval));
            xml.Root.Element("realtime_graph").Add(new XElement("cma_seconds", realtimeGraphCMAseconds));
            xml.Root.Add(new XElement("rich_presence"));
            xml.Root.Element("rich_presence").Add(new XElement("enabled", enableRichPresence));
            xml.Root.Element("rich_presence").Add(new XElement("show_location", richPresenceShowLocation));
            xml.Root.Element("rich_presence").Add(new XElement("show_character", richPresenceShowCharacter));
            xml.Root.Element("rich_presence").Add(new XElement("show_status", richPresenceShowStatus));
            xml.Root.Element("rich_presence").Add(new XElement("show_party", richPresenceShowParty));

            xml.Root.Add(new XElement("dps_servers"));
            foreach (var server in DpsServers)
            {
                var serverXml = new XElement("server");
                serverXml.Add(new XElement("username", server.Username));
                serverXml.Add(new XElement("token", server.Token));
                serverXml.Add(new XElement("enabled", server.Enabled));
                serverXml.Add(new XElement("dps_url", server.UploadUrl));
                serverXml.Add(new XElement("glyph_url", server.GlyphUrl));
                serverXml.Add(new XElement("allowed_area_url", server.AllowedAreaUrl));
                xml.Root.Element("dps_servers").Add(serverXml);
            }

            xml.Root.Element("dps_servers").Add(new XElement("blacklist"));
            foreach (var areaId in BlackListAreaId)
            {
                var blacklistId = new XElement("id", areaId);
                xml.Root.Element("dps_servers").Element("blacklist").Add(blacklistId);
            }

            _filestream.SetLength(0);
            using (var sw = new StreamWriter(_filestream, new UTF8Encoding(true))) { sw.Write(xml.Declaration + Environment.NewLine + xml); }
            _filestream.Close();
            _filestream = new FileStream(_windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);


        }

        // I choose to keep the filestream open with Sharing set to None to avoid receiving 
        // help request about "why the config file I manually edited while the meter was running got erased?"
        public void Close()
        {
            _filestream.Close();
        }

        private void Parse(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) { return; }
            var setting = GetType().GetField(settingName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (setting.FieldType == typeof(int))
            {
                var parseSuccess = int.TryParse(xml.Value, out int value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(double))
            {
                var parseSuccess = double.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(float))
            {
                var parseSuccess = float.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(bool))
            {
                var parseSuccess = bool.TryParse(xml.Value, out bool value);
                if (parseSuccess) { setting.SetValue(this, value); }
            }
            if (setting.FieldType == typeof(string)) { setting.SetValue(this, xml.Value); }
        }
    }
}