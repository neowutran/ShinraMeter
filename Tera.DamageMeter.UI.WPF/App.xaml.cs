using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Tera.DamageMeter.UI.WPF
{


    /// <summary>
    ///     Logique d'interaction pour App.xaml
    /// </summary>
    /// 
    public partial class App
    {


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private Mutex _unique;
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var aIsNewInstance = false;
            _unique = new Mutex(true, "ShinraMeter", out aIsNewInstance);
            if (aIsNewInstance) return;
            var current = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(current.ProcessName).Where(process => process.Id != current.Id))
            {
                SetForegroundWindow(process.MainWindowHandle);
                break;
            }
            Current.Shutdown();
        }
    }
}