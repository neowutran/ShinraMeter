using DamageMeter.AutoUpdate;
using Data;
using Hardcodet.Wpf.TaskbarNotification;
using Lang;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour NotifyIcon.xaml
    /// </summary>
    public partial class NotifyIcon : UserControl
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
    }
}
