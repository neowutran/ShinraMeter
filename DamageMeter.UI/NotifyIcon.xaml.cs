using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.AutoUpdate;
using DamageMeter.D3D9Render;
using Data;
using Lang;
using Newtonsoft.Json;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace DamageMeter.UI
{
    public partial class NotifyIcon
    {
        private int _animationSpeed = 150;
        private MainWindow _mainWindow;

        public NotifyIcon()
        {
            InitializeComponent();
            Tray.ToolTipText = "Shinra Meter V" + UpdateManager.Version + ": " + LP.SystemTray_No_server;
        }

        public void InitializeServerList(List<TeraDpsApi.DpsServer> servers)
        {
            foreach(var server in servers)
            {
                DpsServer dpsServerUi = new DpsServer(server, this);
                dpsServerUi.SetData(server.Data);
                DpsServers.Children.Add(dpsServerUi);
            }
        }

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            AutoExcelExport.Status = BasicTeraData.Instance.WindowData.Excel;
            ExcelCMADPSSpinner.Value = BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds;
            CountOnlyBoss.Status = BasicTeraData.Instance.WindowData.OnlyBoss;
            BossByHpBar.Status = BasicTeraData.Instance.WindowData.DetectBosses;
            PartyOnly.Status = BasicTeraData.Instance.WindowData.PartyOnly;
            InvisibleWhenNoStats.Status = BasicTeraData.Instance.WindowData.InvisibleUi;
            ShowAlways.Status = BasicTeraData.Instance.WindowData.AlwaysVisible;
            StayTopMost.Status = BasicTeraData.Instance.WindowData.Topmost;
            NumberPlayersSpinner.Value = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed;
            LFDelaySpinner.Value = BasicTeraData.Instance.WindowData.LFDelay;
            RemoveTeraAltEnterHotkey.Status = BasicTeraData.Instance.WindowData.RemoveTeraAltEnterHotkey;
            ChatEnabled.Status = BasicTeraData.Instance.WindowData.EnableChat;
            CopyInspect.Status = BasicTeraData.Instance.WindowData.CopyInspect;
            FormatPasteString.Status = BasicTeraData.Instance.WindowData.FormatPasteString;
            SayColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.SayColor;
            GroupColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.GroupColor;
            AllianceColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.AllianceColor;
            AreaColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.AreaColor;
            WhisperColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.WhisperColor;
            GuildColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.GuildColor;
            EmotesColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.SayColor;
            TradingColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.TradingColor;
            PrivateChannelColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.PrivateChannelColor;
            GeneralColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.GeneralColor;
            RaidColorSelecter.SelectedColor = BasicTeraData.Instance.WindowData.RaidColor;
            PartyEvent.Status = BasicTeraData.Instance.WindowData.DisablePartyEvent;
            ShowAfkIventsIngame.Status = BasicTeraData.Instance.WindowData.ShowAfkEventsIngame;
            MuteSound.Status = BasicTeraData.Instance.WindowData.MuteSound;
            ShowSelfOnTop.Status = BasicTeraData.Instance.WindowData.MeterUserOnTop;
            IdleRTOSpinner.Value = BasicTeraData.Instance.WindowData.IdleResetTimeout;
            NoPaste.Status = BasicTeraData.Instance.WindowData.NoPaste;
            NoAbnormalsInHUD.Status = BasicTeraData.Instance.WindowData.NoAbnormalsInHUD;
            OverlaySwitch.Status = BasicTeraData.Instance.WindowData.EnableOverlay;
            ChatSettingsVisible(BasicTeraData.Instance.WindowData.EnableChat);
            //PerformanceTabIcon.Source = BasicTeraData.Instance.ImageDatabase.Performance.Source;
            SettingsTabIcon.Source = BasicTeraData.Instance.ImageDatabase.Settings.Source;
            //LinksTabIcon.Source = BasicTeraData.Instance.ImageDatabase.Links.Source;

            TopLeftLogo.Source = BasicTeraData.Instance.ImageDatabase.Icon;

            CloseIcon.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            ChatBoxIcon.Source = BasicTeraData.Instance.ImageDatabase.Chat.Source;
            SiteExportIcon.Source = BasicTeraData.Instance.ImageDatabase.Link.Source;
            ExcelExportIcon.Source = BasicTeraData.Instance.ImageDatabase.Excel.Source;
            ResetIcon.Source = BasicTeraData.Instance.ImageDatabase.Reset.Source;
            UploadGlyphIcon.Source = BasicTeraData.Instance.ImageDatabase.Upload.Source;

            GitHubIcon.Source = BasicTeraData.Instance.ImageDatabase.GitHub.Source;
            DiscordIcon.Source = BasicTeraData.Instance.ImageDatabase.Discord.Source;
            RankSitesIcon.Source = BasicTeraData.Instance.ImageDatabase.Cloud.Source;
            MoongourdIcon.Source = BasicTeraData.Instance.ImageDatabase.Moongourd.Source;
            TeralogsIcon.Source = BasicTeraData.Instance.ImageDatabase.Teralogs.Source;
        }

        private void ResetAction(object sender, RoutedEventArgs e)
        {
            NetworkController.Instance.NeedToReset = true;
        }

        private void CloseAction(object sender, RoutedEventArgs e)
        {
            _mainWindow.VerifyClose();
        }

        private void WikiAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki");
        }

        private void PatchAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki/Patch-note");
        }

        private void IssueAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/issues");
        }

        private void DiscordAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://discord.gg/anUXQTp");
        }

        public void SwitchStayTop(object sender = null, EventArgs e = null)
        {
            BasicTeraData.Instance.WindowData.Topmost = !BasicTeraData.Instance.WindowData.Topmost;
            StayTopMost.Status = BasicTeraData.Instance.WindowData.Topmost;
            UpdateTopMost();
        }

        private void MoongourdAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "http://moongourd.com");
        }
        private void TeralogsAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "http://teralogs.com");
        }

        private void ExcelExportAction(object sender, RoutedEventArgs e)
        {
            NetworkController.Instance.NeedToExport = DataExporter.Dest.Excel | DataExporter.Dest.Manual;
        }
        private long _lastSend;

        private void SiteExportAction(object sender, RoutedEventArgs e)
        {
            if (_lastSend + TimeSpan.TicksPerSecond * 60 >= DateTime.Now.Ticks) { return; }
            NetworkController.Instance.NeedToExport = DataExporter.Dest.Site;
            _lastSend = DateTime.Now.Ticks;
        }

        private void EnableAutoExcelExportAction(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Excel = true;
        }

        private void DisableExcelExportAction(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Excel = false;
        }

        private void DisableStayTopMost(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Topmost = false;
            UpdateTopMost();
        }

        private void EnableStayTopMost(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Topmost = true;
            UpdateTopMost();
        }

        private void UpdateTopMost()
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.Topmost = BasicTeraData.Instance.WindowData.Topmost;
                window.ShowInTaskbar = !BasicTeraData.Instance.WindowData.Topmost;
            }
        }

        private void EnableShowAlways(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.AlwaysVisible = true;
        }

        private void DisableShowAlways(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.AlwaysVisible = false;
        }

        private void EnableInvisibleWhenNoStats(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.InvisibleUi = true;
            if (_mainWindow.ForceWindowVisibilityHidden) { return; }
            _mainWindow.Visibility = Visibility.Visible;
        }

        private void DisableInvisibleWhenNoStats(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.InvisibleUi = false;
            if (_mainWindow.ForceWindowVisibilityHidden) { return; }
            _mainWindow.Visibility = _mainWindow.Controls.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void EnableClickThrou(object sender, RoutedEventArgs e)
        {
            NetworkController.Instance.SwitchClickThrou(true);
        }

        private void DisableClickThrou(object sender, RoutedEventArgs e)
        {
            NetworkController.Instance.SwitchClickThrou(false);
        }

        private void EnablePartyOnly(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.PartyOnly = true;
        }

        private void DisablePartyOnly(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.PartyOnly = false;
        }

        private void EnableBossByHpBar(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.DetectBosses = true;
            if (BasicTeraData.Instance.MonsterDatabase != null) { BasicTeraData.Instance.MonsterDatabase.DetectBosses = true; }
        }

        private void DisableBossByHpBar(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.DetectBosses = false;
            if (BasicTeraData.Instance.MonsterDatabase != null) { BasicTeraData.Instance.MonsterDatabase.DetectBosses = false; }
        }

        private void EnableCountOnlyBoss(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.OnlyBoss = true;
        }

        private void DisableCountOnlyBoss(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.OnlyBoss = false;
        }

        private void NumberPlayersChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed = NumberPlayersSpinner?.Value ?? 5;
        }

        private void LFDelayChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.LFDelay = LFDelaySpinner?.Value ?? 150;
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ConfigScrollViewer.ScrollToVerticalOffset(ConfigScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;            
        }

        private void PrivateChannelColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.PrivateChannelColor = (Color)e.NewValue;
        }

        private void EmotesColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.EmotesColor = (Color)e.NewValue;
        }

        private void TradingColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.TradingColor = (Color)e.NewValue;
        }

        private void SayColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.SayColor = (Color)e.NewValue;
        }

        private void RaidColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.RaidColor = (Color)e.NewValue;
        }

        private void GuildColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.GuildColor = (Color)e.NewValue;
        }

        private void GroupColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.GroupColor = (Color)e.NewValue;
        }

        private void GeneralColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.GeneralColor = (Color)e.NewValue;
        }

        private void AreaColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.AreaColor = (Color)e.NewValue;
        }

        private void AllianceColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.AllianceColor = (Color)e.NewValue;
        }

        private void WhisperColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BasicTeraData.Instance.WindowData.WhisperColor = (Color)e.NewValue;
        }

        private void EnableRemoveTeraAltEnterHotkey(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.RemoveTeraAltEnterHotkey = true;
            KeyboardHook.Instance.Update();
        }

        private void DisableRemoveTeraAltEnterHotkey(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.RemoveTeraAltEnterHotkey = false;
            KeyboardHook.Instance.Update();
        }

        private void EnableChat(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.EnableChat = true;
            ChatSettingsVisible(true);
        }

        private void DisableChat(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.EnableChat = false;
            ChatSettingsVisible(false);
        }

        private void ChatSettingsVisible(bool show)
        {
            DoubleAnimation an;
            if (show)
            {
                an = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(_animationSpeed)) { EasingFunction = new QuadraticEase() } ;
            }
            else
            {
                an = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(_animationSpeed)) { EasingFunction = new QuadraticEase() };
            }

            CopyInspect.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            MuteSound.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            NoAbnormalsInHUD.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            ShowAfkIventsIngame.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            PartyEvent.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);

            ColorSettingsContainer.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
        }

        private void EnableCopyInspect(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.CopyInspect = true;
        }

        private void DisableCopyInspect(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.CopyInspect = false;
        }

        private void ClickUploadGlyphAction(object sender, RoutedEventArgs e)
        { 
            DataExporter.ExportGlyph();
        }

        private void EnableFormatPasteString(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.FormatPasteString = true;
        }

        private void DisableFormatPasteString(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.FormatPasteString = false;
        }


        private void ExcelCMADPSChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds = ExcelCMADPSSpinner?.Value ?? 1;
        }

        private void DisablePartyEvent(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.DisablePartyEvent = true;
        }

        private void EnablePartyEvent(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.DisablePartyEvent = false;
        }

        private void EnableAfkIventsIngame(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.ShowAfkEventsIngame = true;
        }

        private void DisableAfkIventsIngame(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.ShowAfkEventsIngame = false;
        }

        private void ClickOpenChatBox(object sender, RoutedEventArgs e)
        {
            _mainWindow._chatbox = new Chatbox { Owner = _mainWindow };
            _mainWindow._chatbox.ShowWindow();
        }

        private void EnableMuteSound(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.MuteSound = true;
        }

        private void DisableMuteSound(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.MuteSound = false;
        }

        private void EnableShowSelfOnTop(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.MeterUserOnTop = true;
        }

        private void DisableShowSelfOnTop(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.MeterUserOnTop = false;
        }

        private void IdleRtoChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.IdleResetTimeout = IdleRTOSpinner?.Value ?? 0;
            if (BasicTeraData.Instance.WindowData.IdleResetTimeout == 0) { DamageTracker.Instance.LastIdleStartTime = 0; }
        }

        private void NoPaste_OnUnchecked(object sender, RoutedEventArgs e) { BasicTeraData.Instance.WindowData.NoPaste = false; }

        private void NoPaste_OnChecked(object sender, RoutedEventArgs e) { BasicTeraData.Instance.WindowData.NoPaste = true; }

        private void EnableNoAbnormalsInHUD(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.NoAbnormalsInHUD = true;
            foreach (var boss in HudManager.Instance.CurrentBosses)
            {
                boss.Buffs.DisposeAll();
            }
        }

        private void DisableNoAbnormalsInHUD(object sender, RoutedEventArgs e) { BasicTeraData.Instance.WindowData.NoAbnormalsInHUD = false; }

        private void gitButton_Click(object sender, RoutedEventArgs e)
        {
            gitPopup.Placement = PlacementMode.Bottom;
            gitPopup.PlacementTarget = gitButton;
            var h = gitPopup.Height;
            gitPopup.Height = 0;
            var an = new DoubleAnimation(0, h, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            gitPopup.IsOpen = true;
            gitPopup.BeginAnimation(HeightProperty, an);

        }

        private void gitPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            gitPopup.IsOpen = false;
        }
        private void rankPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            rankPopup.IsOpen = false;
        }

        private void EnableOverlay(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.EnableOverlay = true;
            if (_mainWindow.DXrender!=null) return;
            _mainWindow.DXrender = new Renderer();
        }

        private void DisableOverlay(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.EnableOverlay = false;
            var render = _mainWindow.DXrender;
            _mainWindow.DXrender = null;
            render.Dispose();
        }

        private void sitesButton_Click(object sender, RoutedEventArgs e)
        {
            rankPopup.Placement = PlacementMode.Bottom;
            rankPopup.PlacementTarget = sitesButton;
            var h = rankPopup.Height;
            rankPopup.Height = 0;
            var an = new DoubleAnimation(0, h, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            rankPopup.IsOpen = true;
            rankPopup.BeginAnimation(HeightProperty, an);

        }

        private void AddServerButton_OnClick(object sender, RoutedEventArgs e)
        {
            var server = new DamageMeter.TeraDpsApi.DpsServer(new DpsServerData(null, null, null, null, null, true), false);
            BasicTeraData.Instance.WindowData.DpsServers.Add(server.Data);
            DataExporter.DpsServers.Add(server);
            DpsServer dpsServerUi = new DpsServer(server, this);
            dpsServerUi.SetData(server.Data);
            DpsServers.Children.Add(dpsServerUi);
        }
    }
}