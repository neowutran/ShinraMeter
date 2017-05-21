using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

        private async void Display (int displayTime)
        {
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

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try { w?.DragMove(); }
            catch { Debug.WriteLine(@"Exception move"); }
        }
    }
}