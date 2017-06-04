using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Data;
using Data.Actions.Notify;
using System.Windows.Media.Animation;
using System;
using System.Windows.Media;

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
                Value(flash.Balloon.TitleText, flash.Balloon.BodyText, flash.Balloon.EventType);
                Display(flash.Balloon.DisplayTime);
            }
            if (!BasicTeraData.Instance.WindowData.MuteSound && flash.Sound != null) { Task.Run(() => flash.Sound.Play()); }
        }

        private int _stupidNotSafeLock;

        private async void Display (int displayTime)
        {
            Topmost = false;
            Topmost = true;
            ShowWindow();
            var w = root.ActualWidth;
            root.BeginAnimation(WidthProperty, new DoubleAnimation(0, w, TimeSpan.FromMilliseconds(400)) { EasingFunction = new QuadraticEase() });
            _stupidNotSafeLock++;
            await Task.Delay(displayTime);
            _stupidNotSafeLock--;
            if (_stupidNotSafeLock == 0) { HideWindow(); }
        }

        private void Value(string title, string text, EventType t)
        {
            TitleLabel.Content = title;
            TextBlock.Text = text;
            Color col;
            switch (t)
            {
                case EventType.MissingAb:
                    col = Colors.Red;
                    break;
                case EventType.AddRemoveAb:
                    col = Colors.Orange;
                    break;
                case EventType.Cooldown:
                    col = Color.FromRgb(0xfd, 0x39, 0x20);
                    break;
                case EventType.AFK:
                    col = Color.FromRgb(0x2, 0xbb, 0xff);
                    break;
                default:
                    col = Color.FromRgb(0x2,0xbb,0xff);
                    break;
            }

            rect.Fill = new SolidColorBrush(col);
        }
    }
}