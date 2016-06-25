using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Data;

namespace DamageMeter.UI
{
    public class ClickThrouWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        public ClickThrouWindow()
        {
            AllowsTransparency = BasicTeraData.Instance.WindowData.AllowTransparency;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 200;
            Closing += ClickThrouWindow_Closing;
            WindowStyle = WindowStyle.None;
            Focusable = false;
            BorderThickness = new Thickness(0);
            Topmost = true;
            Icon = BasicTeraData.Instance.ImageDatabase.Icon;
            SizeToContent = SizeToContent.WidthAndHeight;
            MouseLeftButtonDown += Move;
            ShowActivated = false;
            ResizeMode = ResizeMode.NoResize;
        }

        protected void Move(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception Move");
            }
        }

        protected void ClickThrouWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(WindowsServices.ClickNoFocus);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}