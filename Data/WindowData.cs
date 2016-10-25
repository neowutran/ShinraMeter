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
        public string UILanguage { get; private set; }
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

        public int SoundNotifyDuration { get; set; }
        public int PopupDisplayTime { get; set; }
        public bool DateInExcelPath { get; set; }

        public string NotifySound { get; set; }

        public bool SoundConsoleBeepFallback { get; set; }

        public int NumberOfPlayersDisplayed { get; set; }
        public float Volume { get; set; }
        public bool LowPriority { get; set; }
        public bool EnableChat { get; set; }
        public bool CopyInspect { get; set; }

     
        public string DiscordLogin { get; set; }
        public string DiscordPassword { get; set; }


       public Dictionary<string, DiscordInfoByGuild> DiscordInfoByGuild { get; set; }

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
        public bool DoNotWarnOnCB { get; set; }
        public bool FormatPasteString { get; set; }
        
      
        private void DefaultValue()
        {
            Location = new Point(0, 0);
            Language = "Auto";
            UILanguage = "Auto";
            MainWindowOpacity = 0.5;
            SkillWindowOpacity = 0.7;
            SoundConsoleBeepFallback = true;
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
            SiteExport = false;
            ShowHealCrit = true;
            ExcelSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShinraMeter/");
            LFDelay = 150;
            OnlyBoss = false;
            DetectBosses = false;
            DateInExcelPath = false;
            NumberOfPlayersDisplayed = 5;
            PopupDisplayTime = 10000;
            SoundNotifyDuration = 3000;
            Volume = 1.0f;
            NotifySound = "ElinuDance.mp3";
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
            DoNotWarnOnCB = false;
            DiscordInfoByGuild = new Dictionary<string, Data.DiscordInfoByGuild>();
            DiscordLogin = "";
            DiscordPassword = "";
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
            Parse("number_of_players_displayed", "NumberOfPlayersDisplayed");
            Parse("excel_save_directory", "ExcelSaveDirectory");

            if (ExcelSaveDirectory == "")
            {
                ExcelSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShinraMeter/");
            }


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
            Parse("date_in_excel_path", "DateInExcelPath");
            Parse("low_priority", "LowPriority");
            Parse("format_paste_string", "FormatPasteString");           
            Parse("notify_sound", "NotifySound");
            Parse("popup_display_time", "PopupDisplayTime");
            Parse("sound_notify_duration", "SoundNotifyDuration");
            Parse("volume", "Volume");
            Parse("sound_console_beep_fallback", "SoundConsoleBeepFallback");
            Parse("remove_tera_alt_enter_hotkey", "RemoveTeraAltEnterHotkey");
            Parse("enable_chat_and_notifications", "EnableChat");
            Parse("copy_inspect", "CopyInspect");
            Parse("do_not_warn_on_crystalbind", "DoNotWarnOnCB");
            
            ParseColor("say_color","SayColor");
            ParseColor("alliance_color", "AllianceColor");
            ParseColor("area_color", "AreaColor");
            ParseColor("guild_color", "GuildColor");
            ParseColor("whisper_color", "WhisperColor");
            ParseColor("general_color", "GeneralColor");
            ParseColor("group_color", "GroupColor");
            ParseColor("trading_color", "TradingColor");
            ParseColor("emotes_color", "EmotesColor");
            ParseColor("private_channel_color", "PrivateChannelColor");

            ParseLocation();
            ParseOpacity();
            ParseTeraDps();
            ParseDiscord();
            ParseLanguage();
            ParseUILanguage();
        }

        private void ParseColor(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) return;
            var setting = this.GetType().GetProperty(settingName);
            setting.SetValue(this, (Color)ColorConverter.ConvertFromString(xml.Value), null);
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
            var exp = teradps.Element("enabled");
            if (exp == null) return;
            bool val;
            var parseSuccess = bool.TryParse(exp.Value, out val);
            if (parseSuccess)
            {
                SiteExport = val;
            }
        }


        private void ParseDiscord()
        {
            var root = _xml.Root;
            var discord = root?.Element("discord");
            var user = discord?.Element("login");
            if (user == null) return;
            var password = discord.Element("password");
            if (password == null) return;

            DiscordPassword = password.Value;
            DiscordLogin = user.Value;

            if (DiscordPassword == null || DiscordLogin == null)
            {
                DiscordPassword = "";
                DiscordLogin = "";
            }

            var guilds = discord.Element("guilds");
            if (guilds == null) return;
            foreach(var guild in guilds.Elements())
            {

                ulong discordServer = 0;
                ulong discordChannelGuildInfo = 0;
                ulong discordChannelGuildQuest = 0;

                var server = guild.Element("server");
                if (server == null) return;
               

                ulong val;
                var parseSuccess = ulong.TryParse(server.Value, out val);
                if (parseSuccess)
                {
                    discordServer = val;
                }
                

                var guild_infos_channel = guild.Element("guild_infos_channel");
                if (guild_infos_channel == null) return;
                
                parseSuccess = ulong.TryParse(guild_infos_channel.Value, out val);
                if (parseSuccess)
                {
                    discordChannelGuildInfo = val;
                }
                

                var guild_quests_channel = guild.Element("guild_quests_channel");
                if (guild_quests_channel == null) return;
                
                parseSuccess = ulong.TryParse(guild_quests_channel.Value, out val);
                if (parseSuccess)
                {
                    discordChannelGuildQuest = val;
                }

                string guildInfosText = ":dart: {guild_guildname}  :dart:\n\n{guild_master} - {guild_size}\n{gold_label}: {guild_gold}\n{xp_label} for next level: {guild_xp_to_next_level}\nCreation time: {guild_creationtime}\nQuest done status: {guild_number_quest_done}/{guild_total_number_quest}\n";
                string questInfoText = ":dart: {quest_guildname} - {quest_type} - {quest_size} :dart:\n\nTime remaining: {quest_time_remaining}\nIs bam quest: {quest_is_bam_quest}\n{targets}\n{rewards}\n";
                string questListInfoText = "{quest_type} - {targets}\n";
                string questListHeaderText = "----NoActiveQuest----\n\n";
                string rewardFooterText = "";
                string rewardContentText = "{reward_name}: {reward_amount}\n";
                string rewardHeaderText = "---------\n";

                string targetHeaderText = "";
                string targetContentText = "{target_name}: {target_current_count}/{target_total_count}\n";
                string targetFooterText = "";
                string questNoActiveText = ":dart:   {guild_guildname}   :dart:\n\n{no_quest_text}\n\n{quest_list}\n";

                var guildInfosTextElement = guild.Element("guild_infos_text");
                if (guildInfosTextElement != null) guildInfosText = guildInfosTextElement.Value;

                var questInfoTextElement = guild.Element("quest_infos_text");
                if (questInfoTextElement != null) questInfoText = questInfoTextElement.Value;

                var questListInfoTextElement = guild.Element("quest_list_infos_text");
                if (questListInfoTextElement != null) questListInfoText = questListInfoTextElement.Value;

                var questListHeaderTextElement = guild.Element("quest_list_infos_header_text");
                if (questListHeaderTextElement != null) questListHeaderText = questListHeaderTextElement.Value;

                var rewardFooterTextElement = guild.Element("reward_footer_text");
                if (rewardFooterTextElement != null) rewardFooterText = rewardFooterTextElement.Value;

                var rewardContentTextElement = guild.Element("reward_content_text");
                if (rewardContentTextElement != null) rewardContentText = rewardContentTextElement.Value;

                var rewardHeaderTextElement = guild.Element("reward_header_text");
                if (rewardHeaderTextElement != null) rewardHeaderText = rewardHeaderTextElement.Value;

                var targetHeaderTextElement = guild.Element("target_header_text");
                if (targetHeaderTextElement != null) targetHeaderText = targetHeaderTextElement.Value;

                var targetContentTextElement = guild.Element("target_content_text");
                if (targetContentTextElement != null) targetContentText = targetContentTextElement.Value;

                var targetFooterTextElement = guild.Element("target_footer_text");
                if (targetFooterTextElement != null) targetFooterText = targetFooterTextElement.Value;

                var questNoActiveTextElement = guild.Element("no_active_quest_text");
                if (questNoActiveTextElement != null) questNoActiveText = questNoActiveTextElement.Value;

                DiscordInfoByGuild.Add(guild.Name.ToString().ToLowerInvariant(), new DiscordInfoByGuild(
                    discordServer,
                    discordChannelGuildInfo,
                    discordChannelGuildQuest,
                    guildInfosText,
                    questInfoText,
                    questListInfoText,
                    questListHeaderText,
                    rewardFooterText,
                    rewardContentText,
                    rewardHeaderText,
                    targetHeaderText,
                    targetContentText,
                    targetFooterText,
                    questNoActiveText
                    
                    ));
            }

         
        }

        private void ParseLocation()
        {
            double x, y;
            var root = _xml.Root;

            var location = root?.Element("location");
            if (location == null) return;
            var xElement = location.Element("x");
            var yElement = location.Element("y");
            if (xElement == null || yElement == null) return;

            var xParsed = double.TryParse(xElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
            var yParsed = double.TryParse(yElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
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

        private void ParseUILanguage()
        {
            var root = _xml.Root;
            var languageElement = root?.Element("ui_language");
            if (languageElement == null) return;
            UILanguage = languageElement.Value;
            try
            {
                CultureInfo.GetCultureInfo(UILanguage);
            }
            catch
            {
                UILanguage = "Auto";
            }
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


            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("window"));
            xml.Root.Add(new XElement("location"));
            xml.Root.Element("location").Add(new XElement("x", Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("location").Add(new XElement("y", Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("language", Language));
            xml.Root.Add(new XElement("ui_language", UILanguage));
            xml.Root.Add(new XElement("opacity"));
            xml.Root.Element("opacity").Add(new XElement("mainWindow", MainWindowOpacity*100));
            xml.Root.Element("opacity").Add(new XElement("skillWindow", SkillWindowOpacity*100));
            xml.Root.Add(new XElement("autoupdate", AutoUpdate));
            xml.Root.Add(new XElement("remember_position", RememberPosition));
            xml.Root.Add(new XElement("winpcap", Winpcap));
            xml.Root.Add(new XElement("invisible_ui_when_no_stats", InvisibleUi));
            xml.Root.Add(new XElement("allow_transparency", AllowTransparency));
            xml.Root.Add(new XElement("topmost", Topmost));
            xml.Root.Add(new XElement("debug", Debug));
            xml.Root.Add(new XElement("excel", Excel));
            xml.Root.Add(new XElement("date_in_excel_path", DateInExcelPath));
            xml.Root.Add(new XElement("excel_save_directory", ExcelSaveDirectory));
            xml.Root.Add(new XElement("always_visible", AlwaysVisible));
            xml.Root.Add(new XElement("scale", Scale.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("lf_delay", LFDelay));
            xml.Root.Add(new XElement("partyonly", PartyOnly));
            xml.Root.Add(new XElement("showhealcrit", ShowHealCrit));
            xml.Root.Add(new XElement("detect_bosses_only_by_hp_bar", DetectBosses));
            xml.Root.Add(new XElement("only_bosses", OnlyBoss));
            xml.Root.Add(new XElement("low_priority", LowPriority));
            xml.Root.Add(new XElement("number_of_players_displayed", NumberOfPlayersDisplayed));
            xml.Root.Add(new XElement("remove_tera_alt_enter_hotkey", RemoveTeraAltEnterHotkey));
            xml.Root.Add(new XElement("enable_chat_and_notifications", EnableChat));
            xml.Root.Add(new XElement("copy_inspect", CopyInspect));
            xml.Root.Add(new XElement("do_not_warn_on_crystalbind", DoNotWarnOnCB));
            xml.Root.Add(new XElement("format_paste_string", FormatPasteString));
            xml.Root.Add(new XElement("notify_sound", NotifySound));
            xml.Root.Add(new XElement("volume", Volume));
            xml.Root.Add(new XElement("popup_display_time", PopupDisplayTime));
            xml.Root.Add(new XElement("sound_notify_duration", SoundNotifyDuration));
            xml.Root.Add(new XElement("sound_console_beep_fallback", SoundConsoleBeepFallback));


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

            xml.Root.Add(new XElement("teradps.io"));
            xml.Root.Element("teradps.io").Add(new XElement("user", TeraDpsUser));
            xml.Root.Element("teradps.io").Add(new XElement("token", TeraDpsToken));
            xml.Root.Element("teradps.io").Add(new XElement("enabled", SiteExport));

            xml.Root.Add(new XElement("discord"));
            xml.Root.Element("discord").Add(new XElement("login", DiscordLogin));
            xml.Root.Element("discord").Add(new XElement("password", DiscordPassword));
            xml.Root.Element("discord").Add(new XElement("guilds"));
            foreach (var discordData in DiscordInfoByGuild)
            {
                var name = discordData.Key.ToString().ToLowerInvariant();
                xml.Root.Element("discord").Element("guilds").Add(new XElement(name));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("guild_infos_channel", discordData.Value.DiscordChannelGuildInfo));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("guild_quests_channel", discordData.Value.DiscordChannelGuildQuest));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("server", discordData.Value.DiscordServer));

                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("guild_infos_text", discordData.Value.GuildInfosText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("quest_infos_text", discordData.Value.QuestInfoText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("quest_list_infos_text", discordData.Value.QuestListInfoText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("quest_list_infos_header_text", discordData.Value.QuestListHeaderText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("reward_footer_text", discordData.Value.RewardFooterText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("reward_content_text", discordData.Value.RewardContentText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("reward_header_text", discordData.Value.RewardHeaderText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("target_header_text", discordData.Value.TargetHeaderText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("target_content_text", discordData.Value.TargetContentText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("target_footer_text", discordData.Value.TargetFooterText));
                xml.Root.Element("discord").Element("guilds").Element(name).Add(new XElement("no_active_quest_text", discordData.Value.QuestNoActiveText));
            }


            _filestream.SetLength(0); 
            using (var sw = new StreamWriter(_filestream, new UTF8Encoding(true)))
            {
                sw.Write(xml.Declaration + Environment.NewLine + xml);
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
            if (setting.PropertyType == typeof(float))
            {
                float value;
                var parseSuccess = float.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
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