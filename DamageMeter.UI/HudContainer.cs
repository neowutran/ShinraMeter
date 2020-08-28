using DamageMeter.AutoUpdate;
using DamageMeter.D3D9Render;
using DamageMeter.TeraDpsApi;
using DamageMeter.UI.EntityStats;
using DamageMeter.UI.HUD.Windows;
using Data;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using Tera.Game;

namespace DamageMeter.UI
{
    public class HudContainer
    {
        public bool NeedRefreshClickThrou = false;

        public bool TopMostOverride = true;

        public MainWindow MainWindow; // set from its own ctor

        public readonly EntityStatsMain EntityStats;
        public readonly BossGageWindow BossGage;
        public readonly PopupNotification Notifications;
        public readonly TeradpsHistory UploadHistory;
        public readonly TrayIcon TrayIcon;

        public Renderer DXrender;


        public HudContainer()
        {
            EntityStats = new EntityStatsMain { Scale = BasicTeraData.Instance.WindowData.DebuffsStatus.Scale, DontClose = true };
            BossGage = new BossGageWindow { Scale = BasicTeraData.Instance.WindowData.BossGageStatus.Scale, DontClose = true };
            Notifications = new PopupNotification { DontClose = true };
            UploadHistory = new TeradpsHistory(new ConcurrentDictionary<UploadData, NpcEntity>()) { Scale = BasicTeraData.Instance.WindowData.HistoryStatus.Scale, DontClose = true };

#if DX_ENABLED
            if (BasicTeraData.Instance.WindowData.EnableOverlay) DXrender = new D3D9Render.Renderer(); ///*** fix me
#endif

            TrayIcon = new TrayIcon();

            PacketProcessor.Instance.TickUpdated += Update;

            PacketProcessor.Instance.SetClickThrouAction += SetClickThrou;
            PacketProcessor.Instance.UnsetClickThrouAction += UnsetClickThrou;

            PacketProcessor.Instance.GuildIconAction += OnGuildIconChanged;

            PacketProcessor.Instance.Connected += OnConnected;
        }


        private void OnConnected(string servername)
        {
            TrayIcon.Text = $"Shinra Meter v{UpdateManager.Version}: {servername}";
        }

        private void OnGuildIconChanged(Bitmap icon)
        {
            App.MainDispatcher.Invoke(() =>
            {
                TrayIcon.Icon = icon?.GetIcon() ?? BasicTeraData.Instance.ImageDatabase.Tray;
                MainWindow.Icon = icon?.ToImageSource() ?? BasicTeraData.Instance.ImageDatabase.Icon;
            });

        }

        private void Update(UiUpdateMessage message)
        {
            EntityStats.Update(message.StatsSummary.EntityInformation, message.Abnormals);
            UploadHistory.Update(message.BossHistory);
            Notifications.AddNotification(message.Flash);
            DXrender?.Draw(message.StatsSummary.PlayerDamageDealt.ToClassInfo(message.StatsSummary.EntityInformation.TotalDamage, message.StatsSummary.EntityInformation.Interval));

            RefreshClickThrou();

            if (TeraWindow.IsTeraActive() && BasicTeraData.Instance.WindowData.Topmost)
            {
                StayTopMost();
            }

        }

        public void UnsetClickThrou()
        {
            if (new WindowInteropHelper(MainWindow).Handle.ToInt64() == 0)
            {
                NeedRefreshClickThrou = true;
                return;
            }

            NeedRefreshClickThrou = false;

            MainWindow.UnsetClickThrou();
            EntityStats.UnsetClickThrou();
            Notifications.UnsetClickThrou();
            BossGage.UnsetClickThrou();
        }

        public void SetClickThrou()
        {
            if (new WindowInteropHelper(MainWindow).Handle.ToInt64() == 0)
            {
                NeedRefreshClickThrou = true;
                return;
            }

            NeedRefreshClickThrou = false;

            MainWindow.SetClickThrou();
            EntityStats.SetClickThrou();
            Notifications.SetClickThrou();
            BossGage.SetClickThrou();
        }

        private void RefreshClickThrou()
        {
            if (!NeedRefreshClickThrou) { return; }
            if (BasicTeraData.Instance.WindowData.ClickThrou)
            {
                SetClickThrou();
            }
            else
            {
                UnsetClickThrou();
            }
        }

        public void SaveWindowsPos()
        {
            MainWindow.SaveWindowPos();
            BossGage.SaveWindowPos();
            UploadHistory.SaveWindowPos();
            EntityStats.SaveWindowPos();
            Notifications.SaveWindowPos();
        }

        public void Dispose()
        {
            TopMostOverride = false;

            MainWindow.Dispose();
            DXrender?.Dispose();
            TrayIcon.Dispose();
        }
        internal void StayTopMost()
        {
            App.MainDispatcher.Invoke(() =>
            {
                if (!TopMostOverride /*|| !MainWindow.Topmost*/) //todo: check if second condition is actually needed
                {
                    Debug.WriteLine("Not topmost");
                    return;
                }
                foreach (Window window in Application.Current.Windows)
                {
                    window.Topmost = false;
                    window.Topmost = true;
                }
            });
        }
    }
}