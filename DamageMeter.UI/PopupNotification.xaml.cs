using System.Threading.Tasks;
using System.Windows;
using Data;
using Data.Actions.Notify;
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
                var existing = notificationBalloons.FirstOrDefault(x => x.TitleText == flash.Balloon.TitleText && x.Icon == flash.Balloon.Icon);
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
                    if(flash.Balloon.EventType == EventType.Whisper)
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
        }

        public double DHeight
        {
            get => (double)GetValue(DHeightProperty);
            set => SetValue(DHeightProperty, value);
        }

        public static readonly DependencyProperty DHeightProperty = DependencyProperty.Register("DHeight", typeof(double), typeof(PopupNotification));

        internal void RemoveMe(Balloon context)
        {
            var item = notificationBalloons.FirstOrDefault(x => x.TitleText == context.TitleText && x.Icon == context.Icon && x.BodyText == context.BodyText);
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
    }
}