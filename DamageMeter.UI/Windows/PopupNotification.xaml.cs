using System.Threading.Tasks;
using System.Windows;
using Data;
using Data.Actions.Notify;
using System.Linq;
using System.Collections.Generic;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour PopupNotification.xaml
    /// </summary>
    public partial class PopupNotification
    {
        public PopupNotification()
        {
            Loaded += OnLoaded;
            InitializeComponent();
            notificationBalloons = new SynchronizedObservableCollection<Balloon>();
            notificationBalloons.CollectionChanged += NotificationBalloons_CollectionChanged;
            content.ItemsSource = notificationBalloons;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.LastSnappedPoint = BasicTeraData.Instance.WindowData.PopupNotificationLocation;
            this.Left = this.LastSnappedPoint?.X ?? 0;
            this.Top = this.LastSnappedPoint?.Y ?? 0;
            this.Show();
            this.Hide();
        }

        private static PopupNotification _instance = null;
        public static PopupNotification Instance => _instance ?? (_instance = new PopupNotification());


        private void NotificationBalloons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(notificationBalloons.Count == 0)
            {
                HideWindow();
            }
        }

        public SynchronizedObservableCollection<Balloon> notificationBalloons;

        public void AddNotification(List<NotifyFlashMessage> flashList)
        {
            foreach(var flash in flashList.OrderByDescending(x => x.Priority))
            {
                AddNotification(flash);
            }
        }


        public void AddNotification(NotifyFlashMessage flash)
        {
            Dispatcher.Invoke(() =>
            {

                if (flash == null) { return; }

                if (flash.Balloon != null && flash.Balloon.DisplayTime >= 500)
                {
                    var existing = notificationBalloons.ToSyncArray().FirstOrDefault(x => x.TitleText == flash.Balloon.TitleText && x.Icon == flash.Balloon.Icon);
                    if (existing == null)
                    {
                        if (!SnappedToBottom)
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
                        if (flash.Balloon.EventType == EventType.Whisper)
                        {
                            if (!SnappedToBottom)
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

                    Topmost = false;
                    Topmost = true;
                    if (!IsVisible) ShowWindow();
                }

                if (!BasicTeraData.Instance.WindowData.MuteSound && flash.Sound != null) { Task.Run(() => flash.Sound.Play()); }
            });
        }

        public double DHeight
        {
            get => (double)GetValue(DHeightProperty);
            set => SetValue(DHeightProperty, value);
        }

        public static readonly DependencyProperty DHeightProperty = DependencyProperty.Register("DHeight", typeof(double), typeof(PopupNotification));

        internal void RemoveMe(Balloon context)
        {
            var item = notificationBalloons.ToSyncArray().FirstOrDefault(x => x.TitleText == context.TitleText && x.Icon == context.Icon && x.BodyText == context.BodyText);
            if (item != null) notificationBalloons.Remove(item);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);
            var totalHeight = content.DesiredSize.Height;
            if (DHeight > content.DesiredSize.Height)
            {
                if (SnappedToBottom) {spacer.Height = DHeight - totalHeight; bottomspacer.Height = 0;}
                else {bottomspacer.Height = DHeight - totalHeight; spacer.Height=0;}
            }
            else { spacer.Height = 0; bottomspacer.Height = 0;}
            return base.MeasureOverride(availableSize);
        }

        public override void SaveWindowPos()
        {
            BasicTeraData.Instance.WindowData.PopupNotificationLocation = LastSnappedPoint ?? new Point(Left, Top);
        }
    }
}