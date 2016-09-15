using DamageMeter.AutoUpdate;
using Data;
using Lang;
using System;
using System.Diagnostics;
using System.Windows;
using NAudio.Wave;
using System.IO;
using System.Windows.Media;

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

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            DpsWebsiteExport.IsChecked = BasicTeraData.Instance.WindowData.SiteExport;
            AutoExcelExport.IsChecked = BasicTeraData.Instance.WindowData.Excel;
            CountOnlyBoss.IsChecked = BasicTeraData.Instance.WindowData.OnlyBoss;
            BossByHpBar.IsChecked = BasicTeraData.Instance.WindowData.DetectBosses;
            PartyOnly.IsChecked = BasicTeraData.Instance.WindowData.PartyOnly;
            InvisibleWhenNoStats.IsChecked = BasicTeraData.Instance.WindowData.InvisibleUi;
            ShowAlways.IsChecked = BasicTeraData.Instance.WindowData.AlwaysVisible;
            StayTopMost.IsChecked = BasicTeraData.Instance.WindowData.Topmost;
            NumberPlayersSpinner.Value = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed;
            LFDelaySpinner.Value = BasicTeraData.Instance.WindowData.LFDelay;
            SoundTimeSpinner.Value = BasicTeraData.Instance.WindowData.SoundNotifyDuration;
            PopupTimeSpinner.Value = BasicTeraData.Instance.WindowData.PopupDisplayTime;
            SoundFileTextbox.Text = BasicTeraData.Instance.WindowData.NotifySound;
            SoundVolumeSpinner.Value = BasicTeraData.Instance.WindowData.Volume;
            RemoveTeraAltEnterHotkey.IsChecked = BasicTeraData.Instance.WindowData.RemoveTeraAltEnterHotkey;

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

        }

        private IWavePlayer _waveOutDevice = null;
        private AudioFileReader _audioFileReader = null;
        private static readonly object _lock = new object();
        private bool _needToStop = false;

        public void ShowBallon(Tuple<string, string> flash)
        {
            if (flash == null) return;
            Tray.HideBalloonTip();
            var balloon = new Balloon();
            balloon.Value(flash.Item1, flash.Item2);
            Tray.ShowCustomBalloon(balloon, System.Windows.Controls.Primitives.PopupAnimation.Fade, BasicTeraData.Instance.WindowData.PopupDisplayTime);

            if(_waveOutDevice != null)
            {
                lock (_lock)
                {
                    if (_needToStop)
                    {
                        return;
                    }
                    _audioFileReader.Dispose();
                    _waveOutDevice.Dispose();
                }
            }
            _waveOutDevice = new WaveOut();
            _audioFileReader = new AudioFileReader(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "sound/", BasicTeraData.Instance.WindowData.NotifySound));
            _waveOutDevice.Init(_audioFileReader);
            _audioFileReader.Volume = BasicTeraData.Instance.WindowData.Volume;
            _waveOutDevice.Play();
            _needToStop = true;

            var timer = new System.Threading.Timer((obj) =>
            {
                lock (_lock)
                {
                    _needToStop = false;
                    _waveOutDevice.Stop();
                }
             }, null, BasicTeraData.Instance.WindowData.SoundNotifyDuration, System.Threading.Timeout.Infinite);
         
        }

        private void PlaySound()
        {
            var waveOutDevice = new WaveOut();
            var audioFileReader = new AudioFileReader(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "sound/", BasicTeraData.Instance.WindowData.NotifySound));
            waveOutDevice.Init(audioFileReader);
            audioFileReader.Volume = BasicTeraData.Instance.WindowData.Volume;
            waveOutDevice.Play();
            var timer = new System.Threading.Timer((obj) =>
            {  
               waveOutDevice.Stop();
            }, null, BasicTeraData.Instance.WindowData.SoundNotifyDuration, System.Threading.Timeout.Infinite);
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
            _mainWindow.Topmost = BasicTeraData.Instance.WindowData.Topmost;
        }

        private void DpsWebsiteAction(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "http://moongourd.net");
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
            NetworkController.Instance.NeedToExport = true;
        }

        private void EnableAutoExcelExportAction(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Excel = true;
        }

        private void DisableExcelExportAction(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Excel = false;
        }

        private void ClickStayTopMostAction(object sender, RoutedEventArgs e)
        {
            _mainWindow.StayTopMost();
        }

        private void DisableStayTopMost(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Topmost = false;
            _mainWindow.Topmost = BasicTeraData.Instance.WindowData.Topmost;
        }

        private void EnableStayTopMost(object sender, RoutedEventArgs e)
        {
            BasicTeraData.Instance.WindowData.Topmost = true;
            _mainWindow.Topmost = BasicTeraData.Instance.WindowData.Topmost;
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
            BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed = (int)NumberPlayersSpinner.Value;
        }

        private void LFDelayChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.LFDelay = (int)LFDelaySpinner.Value;
        }

        private void PopupTimeChange(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.PopupDisplayTime = (int)PopupTimeSpinner.Value;
        }

        private void SoundTimeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.SoundNotifyDuration = (int)SoundTimeSpinner.Value;

        }

        private void SoundFileChanged(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "sound/", SoundFileTextbox.Text)))
            {
                SoundFileTextbox.Text = BasicTeraData.Instance.WindowData.NotifySound;
                return;
            }
            BasicTeraData.Instance.WindowData.NotifySound = SoundFileTextbox.Text;

        }

        private void SoundVolumeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BasicTeraData.Instance.WindowData.Volume = (float)SoundVolumeSpinner.Value;
        }

        private void TestSoundAction(object sender, RoutedEventArgs e)
        {
            PlaySound();
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
    }
}
