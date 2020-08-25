using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DamageMeter.AutoUpdate;
using DamageMeter.UI.Windows;
using Data;
using log4net;
using Lang;
using System.Windows.Threading;
using DamageMeter.Sniffing;
using DamageMeter.TeraDpsApi;
using DamageMeter.UI.EntityStats;
using DamageMeter.UI.HUD.Windows;
using Microsoft.Win32;
using Tera.Game;
using Point = System.Windows.Point;

namespace DamageMeter.UI
{
    public partial class App
    {
        private static Mutex _unique;
        private static bool _isNewInstance;
        public static SplashScreen SplashScreen;
        public static WindowsManager WindowManager;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            BasicTeraData.LogError("##### CRASH #####\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" +
                                   ex.InnerException + "\r\n" + ex.TargetSite);
            MessageBox.Show(LP.MainWindow_Fatal_error);
        }
        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            BasicTeraData.LogError("##### FORM EXCEPTION #####\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data +
                                   "\r\n" + ex.InnerException + "\r\n" + ex.TargetSite);
            MessageBox.Show(LP.MainWindow_Fatal_error);
        }

        private static void SetAlignment()
        {
            var isLeft = SystemParameters.MenuDropAlignment;
            if (!isLeft) return;
            typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, false);
        }
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            bool notUpdating;
            var currentDomain = AppDomain.CurrentDomain;
            // Handler for unhandled exceptions.
            if(!Debugger.IsAttached) currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

            var updating = new Mutex(true, "ShinraMeterUpdating", out notUpdating);
            _unique = new Mutex(true, "ShinraMeter", out _isNewInstance);


            if (_isNewInstance)
            {
                var waiting = true;
                var ssThread = new Thread(new ThreadStart(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                    SplashScreen = new SplashScreen();
                    SplashScreen.SetText("Initializing...");
                    SplashScreen.SetVer(UpdateManager.Version);
                    SplashScreen.Show();
                    waiting = false;
                    Dispatcher.Run();
                }));
                ssThread.Name = "SplashScreen window thread";
                ssThread.SetApartmentState(ApartmentState.STA);
                ssThread.Start();
                while (waiting)
                {
                    Thread.Sleep(1);
                }
                DeleteTmp();
                UpdateManager.ReadDbVersion();
                if (!BasicTeraData.Instance.WindowData.AllowTransparency) { RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly; }
                FormatHelpers.Instance.CultureInfo = LP.Culture;
                if (!BasicTeraData.Instance.WindowData.AutoUpdate) { return; }
                var shutdown = false;
                try
                {
                    shutdown = await CheckUpdate();
                }
                catch (Exception ex)
                {
                    var log = LogManager.GetLogger(typeof(Program)); //Log4NET
                    log.Error("##### UPDATE EXCEPTION (version=" + UpdateManager.Version + "): #####\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" +
                              ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" + ex.InnerException + "\r\n" + ex.TargetSite);
                    SplashScreen.SetText(LP.App_Unable_to_contact_update_server);
                    //MessageBox.Show(LP.App_Unable_to_contact_update_server);
                }
                UpdateManager.ClearHash();
                if (!shutdown) { return; }
                SplashScreen.SetText("Shutting down...");
                Current.Shutdown();
                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            }

            if (!notUpdating) { SetForeground(); }
            bool isWaitingUpdateEnd;
            var waitUpdateEnd = new Mutex(true, "ShinraMeterWaitUpdateEnd", out isWaitingUpdateEnd);
            if (!isWaitingUpdateEnd) { SetForeground(); }
            updating.WaitOne();
            DeleteTmp();
            updating.Close();
            waitUpdateEnd.Close();
            try { _unique.WaitOne(); }
            catch { _unique = new Mutex(true, "ShinraMeter", out _isNewInstance); }
        }

        private void DeleteTmp()
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\tmp\"))
                {
                    Directory.Delete(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\tmp\", true);
                }
            }
            catch
            {
                //Ignore
            }
        }

        private static void SetForeground()
        {
            var current = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(current.ProcessName).Where(process => process.Id != current.Id))
            {
                SetForegroundWindow(process.MainWindowHandle);
                break;
            }
            Current.Shutdown();
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }

        private static async Task<bool> CheckUpdate()
        {
            SplashScreen.SetText("Checking for updates...");

            var isUpToDate = await UpdateManager.IsUpToDate().ConfigureAwait(false);
            if (isUpToDate) { return false; }

            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            bool result = App.Current.Dispatcher.Invoke(() =>
              {
                  var patchnotes = new UpdatePopup();
                  patchnotes.ShowDialog();
                  if ((patchnotes.DialogResult ?? false) != true) return false;
                  return UpdateManager.Update();
              });
            return result;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            WindowManager.TrayIcon.Dispose();

            if (_isNewInstance) { _unique.ReleaseMutex(); }
        }

        public static void Setup()
        {
            // Handler for exceptions in threads behind forms.
            System.Windows.Forms.Application.ThreadException += GlobalThreadExceptionHandler;
            // force LeftHandedness to avoid menus/tooltips/popup positions to be messed up on some systems
            SetAlignment();
            Application.Current.Resources["Scale"] = BasicTeraData.Instance.WindowData.Scale; // TODO: ????

            if (BasicTeraData.Instance.WindowData.LowPriority) 
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            TeraSniffer.Instance.Enabled = true;
            TeraSniffer.Instance.Warning += (str) => BasicTeraData.LogError(str, false, true);
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            WindowManager = new WindowsManager();

        }

        public static void Dispose()
        {
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;

        }
        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode != PowerModes.StatusChange)
                TeraSniffer.Instance.CleanupForcefully();
        }


        public static void VerifyClose(bool noConfirm)
        {
            if (!noConfirm)
            {
                if (MessageBox.Show(LP.MainWindow_Do_you_want_to_close_the_application, LP.MainWindow_Close_Shinra_Meter_V + UpdateManager.Version,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) { return; }
            }

            WindowManager.SaveWindowsPos();
            WindowManager.MainWindow.Close();
        }
    }

    public class WindowsManager
    {
        public MainWindow MainWindow; // set from its own ctor

        public readonly EntityStatsMain EntityStats;
        public readonly BossGageWindow BossGage;
        public readonly PopupNotification Notifications;
        public readonly TeradpsHistory UploadHistory;
        public readonly TrayIcon TrayIcon;

        public WindowsManager()
        {
            EntityStats = new EntityStatsMain { Scale = BasicTeraData.Instance.WindowData.DebuffsStatus.Scale, DontClose = true };
            BossGage = new BossGageWindow { Scale = BasicTeraData.Instance.WindowData.BossGageStatus.Scale, DontClose = true };
            Notifications = new PopupNotification { DontClose = true };
            UploadHistory = new TeradpsHistory(new ConcurrentDictionary<UploadData, NpcEntity>()) { Scale = BasicTeraData.Instance.WindowData.HistoryStatus.Scale, DontClose = true };
            
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
            TrayIcon.Icon = icon?.GetIcon() ?? BasicTeraData.Instance.ImageDatabase.Tray;
        }

        private void Update(UiUpdateMessage message)
        {
            EntityStats.Update(message.StatsSummary.EntityInformation, message.Abnormals);
            UploadHistory.Update(message.BossHistory);
            Notifications.AddNotification(message.Flash);

        }

        public void UnsetClickThrou()
        {
            EntityStats.UnsetClickThrou();
            Notifications.UnsetClickThrou();
            BossGage.UnsetClickThrou();
        }

        public void SetClickThrou()
        {
            EntityStats.SetClickThrou();
            Notifications.SetClickThrou();
            BossGage.SetClickThrou();
        }

        public void SaveWindowsPos()
        {
            BasicTeraData.Instance.WindowData.BossGageStatus = new WindowStatus(BossGage.LastSnappedPoint ?? new Point(BossGage.Left, BossGage.Top), BossGage.Visible, BossGage.Scale);
            BasicTeraData.Instance.WindowData.HistoryStatus = new WindowStatus(UploadHistory.LastSnappedPoint ?? new Point(UploadHistory.Left, UploadHistory.Top), UploadHistory.Visible, UploadHistory.Scale);
            BasicTeraData.Instance.WindowData.DebuffsStatus = new WindowStatus(EntityStats.LastSnappedPoint ?? new Point(EntityStats.Left, EntityStats.Top), EntityStats.Visible, EntityStats.Scale);
            BasicTeraData.Instance.WindowData.PopupNotificationLocation = Notifications.LastSnappedPoint ?? new Point(Notifications.Left, Notifications.Top);
        }

    }
}