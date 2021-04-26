//#define DX_ENABLED

using DamageMeter.AutoUpdate;
using DamageMeter.Sniffing;
using DamageMeter.UI.Windows;
using Data;
using Lang;
using log4net;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace DamageMeter.UI
{
    public partial class App
    {
        private static Mutex _unique;
        private static bool _isNewInstance;
        public static SplashScreen SplashScreen;
        public static HudContainer HudContainer;
        public static Dispatcher MainDispatcher { get; private set; }

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
            MainDispatcher = Dispatcher.CurrentDispatcher;
            bool notUpdating;
            var currentDomain = AppDomain.CurrentDomain;
            // Handler for unhandled exceptions.
            if (!Debugger.IsAttached) currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

            var updating = new Mutex(true, "ShinraMeterUpdating", out notUpdating);
            _unique = new Mutex(true, "ShinraMeter", out _isNewInstance);

            ToolboxMode = Environment.GetCommandLineArgs().Contains("--toolbox");

            if (_isNewInstance)
            {
                var waiting = true;
                var ssThread = new Thread(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                    SplashScreen = new SplashScreen();
                    SplashScreen.SetText("Initializing...");
                    SplashScreen.SetVer(UpdateManager.Version);
                    SplashScreen.Show();
                    waiting = false;
                    Dispatcher.Run();
                })
                {
                    Name = "SplashScreen window thread"
                };
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
            else
            {
                if (ToolboxMode)
                {
                    Environment.Exit(0);
                }
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

        public static bool ToolboxMode { get; private set; }

        private void DeleteTmp()
        {
            try
            {
                var tmpDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\tmp\";
                if (!Directory.Exists(tmpDir)) return;
                Directory.Delete(tmpDir, true);
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
                WindowsServices.SetForegroundWindow(process.MainWindowHandle);
                break;
            }
            Current.Shutdown();
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }

        private static async Task<bool> CheckUpdate()
        {
            SplashScreen.SetText("Checking for updates...");
            return false; //TODO: re-enable
            var isUpToDate = await UpdateManager.IsUpToDate().ConfigureAwait(false);
            if (isUpToDate) { return false; }

            WindowsServices.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            var result = App.Current.Dispatcher.Invoke(() =>
              {
                  var patchnotes = new UpdatePopup();
                  patchnotes.ShowDialog();
                  return (patchnotes.DialogResult ?? false) && UpdateManager.Update();
              });
            return result;
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

            /*TeraSniffer.Instance*/
            PacketProcessor.Instance.Sniffer.Enabled = true;
            /*TeraSniffer.Instance*/
            PacketProcessor.Instance.Sniffer.Warning += (str) => BasicTeraData.LogError(str, false, true);
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            HudContainer = new HudContainer();

            SettingsWindow.Create();

            // ugly way to make sure this is instanced on main thread, since there is no
            // UI control doing it before Excel exporter does it via analysis thread
            _ = ClassIcons.Instance;

        }

        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            HudContainer.SaveWindowsPos();
            Terminate();
        }
        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode != PowerModes.StatusChange)
                /*TeraSniffer.Instance*/
                PacketProcessor.Instance.Sniffer.CleanupForcefully();
        }

        public static void VerifyClose(bool noConfirm)
        {
            if (!noConfirm)
            {
                if (MessageBox.Show(LP.MainWindow_Do_you_want_to_close_the_application, LP.MainWindow_Close_Shinra_Meter_V + UpdateManager.Version,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) { return; }
            }

            HudContainer.SaveWindowsPos();
            HudContainer.MainWindow.Close();
            Terminate();
        }

        private static void Terminate()
        {
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            HudContainer.Dispose();
            PacketProcessor.Instance.Exit();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (_isNewInstance) { _unique.ReleaseMutex(); }
        }

        public static void StartToolboxProcessCheck()
        {
            Task.Run(async () =>
            {
                while (await MiscUtils.IsToolboxRunningAsync())
                {
                    await Task.Delay(2000);
                    Debug.WriteLine("Toolbox running");
                }
                Debug.WriteLine("Toolbox exited, closing meter");
                MainDispatcher.Invoke(() => VerifyClose(true));
            });
        }
    }
}