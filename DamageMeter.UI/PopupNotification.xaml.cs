using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Data;
using Data.Actions.Notify;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour PopupNotification.xaml
    /// </summary>
    public partial class PopupNotification
    {
        public PopupNotification()
        {
            InitializeComponent();
            this.Loaded += (s, a) => SnapToScreen();
        }

        public void AddNotification(NotifyFlashMessage flash)
        {
            if (flash == null) { return; }
            if (flash.Balloon != null && flash.Balloon.DisplayTime >= 500)
            {
                Value(flash.Balloon.TitleText, flash.Balloon.BodyText);
                Display(flash.Balloon.DisplayTime);
            }
            if (!BasicTeraData.Instance.WindowData.MuteSound && flash.Sound != null) { Task.Run(() => flash.Sound.Play()); }
        }

        private int _stupidNotSafeLock;
        public Point? LastSnappedPoint=null;
        private bool dragged = true;
        private bool dragging = false;

        private void SnapToScreen()
        {
            if (dragging) return;
            var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            // Transform screen point to WPF device independent point
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null) return;
            var m = source.CompositionTarget.TransformToDevice;
            var dx = m.M11;
            var dy = m.M22;
            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);
            TitleLabel.Measure(size);
            TextBlock.Measure(size);
            var width = TitleLabel.DesiredSize.Width + TextBlock.DesiredSize.Width;
            var height = TitleLabel.DesiredSize.Height + TextBlock.DesiredSize.Height + 4 * dx;
            var newLeft = (dragged ? Left : LastSnappedPoint?.X ?? Left) * dx;
            var newTop = (dragged ? Top : LastSnappedPoint?.Y ?? Top) * dy;
            var snapLeft = newLeft;
            var snapTop = newTop;
            if (screen.WorkingArea.X + screen.WorkingArea.Width < newLeft + width + 30 * dx)
            {
                newLeft = screen.WorkingArea.X + screen.WorkingArea.Width - width;
                snapLeft = screen.WorkingArea.X + screen.WorkingArea.Width - 100 * dx;
            }
            else if (screen.WorkingArea.X > newLeft - 30 * dx)
            {
                newLeft = screen.WorkingArea.X;
                snapLeft = screen.WorkingArea.X;
            }
            if (screen.WorkingArea.Y + screen.WorkingArea.Height < newTop + height + 30 * dy)
            {
                newTop = screen.WorkingArea.Y + screen.WorkingArea.Height - height;
                snapTop = screen.WorkingArea.Y + screen.WorkingArea.Height - 70 * dy;
            }
            else if (screen.WorkingArea.Y > newTop - 30 * dy)
            {
                newTop = screen.WorkingArea.Y;
                snapTop = screen.WorkingArea.Y;
            }
            var locationFromScreen=new Point(newLeft, newTop);
            var targetPoint = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
            Left = targetPoint.X;
            Top = targetPoint.Y;
            locationFromScreen = new Point(snapLeft, snapTop);
            if (dragged) LastSnappedPoint = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
            dragged = false;
        }

        private async void Display (int displayTime)
        {
            SnapToScreen();
            ShowWindow();
            _stupidNotSafeLock++;
            await Task.Delay(displayTime);
            _stupidNotSafeLock--;
            if (_stupidNotSafeLock == 0) { HideWindow(); }
        }



        private void Value(string title, string text)
        {
            TitleLabel.Content = title;
            TextBlock.Text = text;
        }

        internal override void Move(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try
            {
                dragging = true;
                w?.DragMove();
                dragged = true;
                dragging = false;
                SnapToScreen();
            }
            catch
            {
                Debug.WriteLine(@"Exception move");
            }
        }
    }
}