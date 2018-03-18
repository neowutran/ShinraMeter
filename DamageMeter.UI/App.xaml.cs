using System;
using System.Diagnostics;
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

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App
    {
        private static Mutex _unique;
        private static bool _isNewInstance;
        public static SplashScreen SplashScreen;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;
            BasicTeraData.LogError("##### CRASH #####\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.Source + "\r\n" + ex + "\r\n" + ex.Data + "\r\n" +
                                   ex.InnerException + "\r\n" + ex.TargetSite);
            MessageBox.Show(LP.MainWindow_Fatal_error);
        }
        
        private async void App_OnStartup(object sender, StartupEventArgs e)
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
            bool notUpdating;
            var currentDomain = AppDomain.CurrentDomain;
            // Handler for unhandled exceptions.
            currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            var updating = new Mutex(true, "ShinraMeterUpdating", out notUpdating);
            _unique = new Mutex(true, "ShinraMeter", out _isNewInstance);


            if (_isNewInstance)
            {
                DeleteTmp();
                UpdateManager.ReadDbVersion();
                if (!BasicTeraData.Instance.WindowData.AllowTransparency) { RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly; }
                FormatHelpers.Instance.CultureInfo = LP.Culture;
                if (!BasicTeraData.Instance.WindowData.AutoUpdate) { return; }
                var shutdown = false;
                try {
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
            bool result=App.Current.Dispatcher.Invoke(() =>
            {
                var patchnotes = new UpdatePopup();
                patchnotes.ShowDialog();
                if ((patchnotes.DialogResult??false)!=true) return false;
                return UpdateManager.Update();
            });
            return result;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (_isNewInstance) { _unique.ReleaseMutex(); }
        }
    }
}