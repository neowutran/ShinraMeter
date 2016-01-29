using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DamageMeter.AutoUpdate;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App
    {
        private Mutex _unique;


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            bool aIsNewInstance;
            bool isUpdating;
            var updating = new Mutex(true, "ShinraMeterUpdating", out isUpdating);
            _unique = new Mutex(true, "ShinraMeter", out aIsNewInstance);
            if (aIsNewInstance)
            {
                if (!BasicTeraData.Instance.WindowData.AutoUpdate)
                {
                    return;
                }
                var shutdown = false;
                try
                {
                    shutdown = await CheckUpdate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to contact update server, try again later: " + ex.Message+". Details:\n"+ex.StackTrace);
                }
                if (!shutdown) return;
                Current.Shutdown();
                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            }

            if (!isUpdating)
            {
                SetForeground();
            }
            bool isWaitingUpdateEnd;
            var waitUpdateEnd = new Mutex(true, "ShinraMeterWaitUpdateEnd", out isWaitingUpdateEnd);

            if (!isWaitingUpdateEnd)
            {
                SetForeground();
            }

            while (isUpdating)
            {
                Thread.Sleep(1000);
                updating = new Mutex(true, "ShinraMeterUpdating", out isUpdating);
            }
            updating.Close();
            waitUpdateEnd.Close();
        }

        private static void SetForeground()
        {
            var current = Process.GetCurrentProcess();
            foreach (
                var process in
                    Process.GetProcessesByName(current.ProcessName).Where(process => process.Id != current.Id))
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
            var isUpToDate = await UpdateManager.IsUpToDate().ConfigureAwait(false);
            if (isUpToDate)
            {
                return false;
            }

            if (MessageBox.Show("Do you want to update the application?", "Update Available", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return false;
            return await UpdateManager.Update();
        }
    }
}