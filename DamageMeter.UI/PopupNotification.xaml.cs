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
using System.Collections.Generic;
using System.Linq;

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
            notificationBalloons = new SynchronizedObservableCollection<Balloon>();
            notificationBalloons.CollectionChanged += NotificationBalloons_CollectionChanged;
            content.ItemsSource = notificationBalloons;
            

        }

        private void NotificationBalloons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(notificationBalloons.Count == 0)
            {
                HideWindow();
            }
        }

        public static SynchronizedObservableCollection<Balloon> notificationBalloons;

        public void AddNotification(NotifyFlashMessage flash)
        {
            if (flash == null) { return; }
            if (flash.Balloon != null && flash.Balloon.DisplayTime >= 500)
            {
                //Value(flash.Balloon.TitleText, flash.Balloon.BodyText, flash.Balloon.EventType);
                //Display(flash.Balloon.DisplayTime);

                //notificationBalloons.Insert(0, flash.Balloon);
                var existing = notificationBalloons.FirstOrDefault(x => x.TitleText == flash.Balloon.TitleText && x.Icon == flash.Balloon.Icon);
                if (existing == null)
                {
                    if (!IsBottomHalf)
                    {
                        notificationBalloons.Add(flash.Balloon);
                    }
                    else
                    {
                        notificationBalloons.Insert(0, flash.Balloon);
                    }
                }
                else
                {
                    if(flash.Balloon.EventType == EventType.Whisper)
                    {
                        if (!IsBottomHalf)
                        {
                            notificationBalloons.Add(flash.Balloon);
                        }
                        else
                        {
                            notificationBalloons.Insert(0, flash.Balloon);
                        }
                    }
                    else
                    {
                        existing.UpdateBody(flash.Balloon.BodyText);
                        existing.Refresh();
                    }
           

                }
                //foreach (var notif in notificationBalloons)
                //{
                //    notif.Refresh();
                //}
                Topmost = false;
                Topmost = true;
                if (!IsVisible) ShowWindow();
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
            var an = new DoubleAnimation(0, w, TimeSpan.FromMilliseconds(400)) { EasingFunction = new QuadraticEase() };
            an.Completed += (s, ev) => { root.Width = double.NaN; };
            root.BeginAnimation(WidthProperty, an);
            _stupidNotSafeLock++;
            await Task.Delay(displayTime);
            _stupidNotSafeLock--;
            if (_stupidNotSafeLock == 0) { HideWindow(); }
        }

        internal static void RemoveMe(Balloon context)
        {
            var item = notificationBalloons.FirstOrDefault(x => x.TitleText == context.TitleText && x.Icon == context.Icon && x.BodyText == context.BodyText);
            if(item != null)
            {
                notificationBalloons.Remove(item);
            }
        }

        private void Value(string title, string text, EventType t)
        {
            //TitleLabel.Content = title;
            //TextBlock.Text = text;
            //Color col;
            //switch (t)
            //{
            //    case EventType.MissingAb:
            //        col = Colors.Red;
            //        break;
            //    case EventType.AddRemoveAb:
            //        col = Colors.Orange;
            //        break;
            //    case EventType.Cooldown:
            //        col = Color.FromRgb(0xfd, 0x39, 0x20);
            //        break;
            //    case EventType.AFK:
            //        col = Color.FromRgb(0x2, 0xbb, 0xff);
            //        break;
            //    default:
            //        col = Color.FromRgb(0x2,0xbb,0xff);
            //        break;
            //}
            //rect.Fill = new SolidColorBrush(col);
        }
    }
}