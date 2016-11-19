using DamageMeter.AutoUpdate;
using Data;
using Lang;
using System;
using System.Diagnostics;
using System.Windows;
using NAudio.Wave;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.TeraDpsApi;
using NAudio.Vorbis;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;
using Data.Actions.Notify;
using System.Collections.Generic;
using Data.Actions.Notify.SoundElements;
using System.Windows.Interop;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour NotifyIcon.xaml
    /// </summary>
    public partial class NotifyIcon
    {

        public NotifyIcon()
        {
            InitializeComponent();
            Tray.ToolTipText = "Shinra Meter V" + UpdateManager.Version + ": " + LP.SystemTray_No_server;
        }

        public void UpdatePacketWaiting(int packetWaiting)
        {
            PacketWaitingLabel.Content = $"{packetWaiting} /3000 {LP.SystemTray_before_crash}";
            PacketWaitingProgressBar.Value = packetWaiting;
        }

        private MainWindow _mainWindow;
        private long _lastSend = 0;
        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            DpsWebsiteExport.IsChecked = BasicTeraData.Instance.WindowData.SiteExport;
            AutoExcelExport.IsChecked = BasicTeraData.Instance.WindowData.Excel;
            ExcelCMADPSSpinner.Value = BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds;
            CountOnlyBoss.IsChecked = BasicTeraData.Instance.WindowData.OnlyBoss;
            BossByHpBar.IsChecked = BasicTeraData.Instance.WindowData.DetectBosses;
            PartyOnly.IsChecked = BasicTeraData.Instance.WindowData.PartyOnly;
            InvisibleWhenNoStats.IsChecked = BasicTeraData.Instance.WindowData.InvisibleUi;
            ShowAlways.IsChecked = BasicTeraData.Instance.WindowData.AlwaysVisible;
            StayTopMost.IsChecked = BasicTeraData.Instance.WindowData.Topmost;
            NumberPlayersSpinner.Value = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed;
            LFDelaySpinner.Value = BasicTeraData.Instance.WindowData.LFDelay;
            RemoveTeraAltEnterHotkey.IsChecked = BasicTeraData.Instance.WindowData.RemoveTeraAltEnterHotkey;
            ChatEnabled.IsChecked = BasicTeraData.Instance.WindowData.EnableChat;
            CopyInspect.IsChecked = BasicTeraData.Instance.WindowData.CopyInspect;
            FormatPasteString.IsChecked = BasicTeraData.Instance.WindowData.FormatPasteString;
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

            DiscordLoginTextBox.Text = BasicTeraData.Instance.WindowData.DiscordLogin;
            DiscordPasswordTextBox.Password = BasicTeraData.Instance.WindowData.DiscordPassword;

            ChatSettingsVisible(BasicTeraData.Instance.WindowData.EnableChat);
        }

     

        public void ShowBallon(NotifyAction flash)
        {
            if (flash == null) return;

            Tray.HideBalloonTip();
            if (flash.Balloon.DisplayTime >= 500 && flash.Balloon != null)
            {
                var balloon = new Balloon();
                balloon.Value(flash.Balloon.TitleText, flash.Balloon.BodyText);
                Tray.ShowCustomBalloon(balloon, System.Windows.Controls.Primitives.PopupAnimation.Fade, flash.Balloon.DisplayTime);
            }

            flash.Sound?.Play();
            
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
            StayTopMost.IsChecked = BasicTeraData.Instance.WindowData.Topmost;
            UpdateTopMost();
        }

        private void DpsWebsiteAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "http://moongourd.com");
        }

        private void EnableDpsWebsiteExportAction(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.SiteExport = true;
        }

        private void DisableDpsWebsiteExportAction(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.SiteExport = false;
        }

        private void ExcelExportAction(object sender, RoutedEventArgs e)
        {
            NetworkController.Instance.NeedToExport = DataExporter.Dest.Excel;
        }

        private void SiteExportAction(object sender, RoutedEventArgs e)
        {
            if (_lastSend + TimeSpan.TicksPerSecond * 60 >= DateTime.Now.Ticks) return;
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
            if (_mainWindow.ForceWindowVisibilityHidden) return;
            _mainWindow.Visibility = Visibility.Visible;
           
        }

        private void DisableInvisibleWhenNoStats(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.InvisibleUi = false;
            if (_mainWindow.ForceWindowVisibilityHidden) return;
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
            if (BasicTeraData.Instance.MonsterDatabase != null) BasicTeraData.Instance.MonsterDatabase.DetectBosses = true;
        }

        private void DisableBossByHpBar(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.DetectBosses = false;
            if (BasicTeraData.Instance.MonsterDatabase != null) BasicTeraData.Instance.MonsterDatabase.DetectBosses = false;
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
            BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed = (int?)NumberPlayersSpinner?.Value??5;
        }

        private void LFDelayChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.LFDelay = (int?)LFDelaySpinner?.Value??150;
        }
        
        private void Grid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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
            CopyInspect.Height = show ? Double.NaN : 0;
            WhisperColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            AllianceColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            AreaColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            GeneralColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            GroupColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            GuildColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            RaidColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            SayColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            TradingColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            EmotesColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            PrivateChannelColor.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            DiscordLogin.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            DiscordPassword.Parent.SetValue(HeightProperty, show ? Double.NaN : 0);
            //for (int i = 14; i <= 28; i++) GridS.RowDefinitions[i].Height = show ? new GridLength(0, GridUnitType.Auto) : new GridLength(0);
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
            if (_lastSend+TimeSpan.TicksPerSecond*30 >= DateTime.Now.Ticks) return;
            if (string.IsNullOrEmpty(NetworkController.Instance.Glyphs.playerName)) return;
            if (NetworkController.Instance.EntityTracker.MeterUser.Level<65) return;
            _lastSend = DateTime.Now.Ticks;
            var json = JsonConvert.SerializeObject(NetworkController.Instance.Glyphs, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Debug.WriteLine(json);
            Task.Run(() =>
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            //client.DefaultRequestHeaders.Add("X-Auth-Token", BasicTeraData.Instance.WindowData.TeraDpsToken);
                            //client.DefaultRequestHeaders.Add("X-User-Id", BasicTeraData.Instance.WindowData.TeraDpsUser);

                            client.Timeout = TimeSpan.FromSeconds(40);
                            var response = client.PostAsync("http://moongourd.com/shared/glyph_data.php", new StringContent(
                                json,
                                Encoding.UTF8,
                                "application/json")
                                );

                            var responseString = response.Result.Content.ReadAsStringAsync();
                            Debug.WriteLine(responseString.Result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                    }
                }
            );
        }

        private void DiscordPasswordChanged(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.DiscordPassword = DiscordPasswordTextBox.Password;
        }

        private void DiscordLoginChanged(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.DiscordLogin = DiscordLoginTextBox.Text;
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
            BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds = (int?)ExcelCMADPSSpinner?.Value??1;
        }
    }
}
