using Data;
using Data.Actions.Notify;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Xceed.Wpf.AvalonDock.Controls;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logica di interazione per NotificationControl.xaml
    /// </summary>
    public partial class NotificationControl : UserControl
    {
        public NotificationControl()
        {
            InitializeComponent();
            //Width = 0;
        }
        Balloon _context;
        Timer _close;
        public double DHeight;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _context = (Balloon)DataContext;
            _context.PropChanged += _context_RefreshEvent;
            _close = new Timer(_context.DisplayTime);
            _close.Elapsed += _close_Elapsed;
            Color col;
            switch (_context.EventType)
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
                case EventType.Whisper:
                    col = Color.FromRgb(0xff, 0x87, 0xb3);
                    img.Source = BasicTeraData.Instance.ImageDatabase.Whisper.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;
                    break;
                case EventType.MatchingSuccess:
                    col = Color.FromRgb(0x2d, 0xff, 0x73);
                    img.Source = BasicTeraData.Instance.ImageDatabase.DoneCircle.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;
                    break;
                case EventType.ReadyCheck:
                    col = Color.FromRgb(0x2d, 0xff, 0x73);
                    img.Source = BasicTeraData.Instance.ImageDatabase.DoneCircle.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;
                    break;
                case EventType.OtherUserApply:
                    col = Color.FromRgb(0x2d, 0xff, 0x73);
                    img.Source = BasicTeraData.Instance.ImageDatabase.GroupAdd.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;

                    break;
                case EventType.Broker:
                    col = Color.FromRgb(0xbf, 0x54, 0x26);
                    img.Source = BasicTeraData.Instance.ImageDatabase.Money.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;

                    break;
                case EventType.PartyInvite:
                    col = Color.FromRgb(0x2d, 0xff, 0x73);
                    img.Source = BasicTeraData.Instance.ImageDatabase.GroupAdd.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;

                    break;
                case EventType.Trade:
                    col = Color.FromRgb(0xbf, 0x54, 0x26);
                    img.Source = BasicTeraData.Instance.ImageDatabase.Money.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;

                    break;
                case EventType.GenericContract:
                    col = Color.FromRgb(0x2, 0xbb, 0xff);
                    img.Source = BasicTeraData.Instance.ImageDatabase.Info.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;

                    break;
                case EventType.VanguardCredits:
                    col = Color.FromRgb(0xde, 0x48, 0xf2);
                    ellImgBrush.ImageSource = BasicTeraData.Instance.ImageDatabase.Credits.Source;

                    break;
                case EventType.WakeUp:
                    col = Color.FromRgb(0x48, 0xf2, 0x94);
                    img.Source = BasicTeraData.Instance.ImageDatabase.Info.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;

                    break;
                case EventType.Mention:
                    col = Color.FromRgb(0x48, 0xf2, 0x94);
                    img.Source = BasicTeraData.Instance.ImageDatabase.Info.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;

                    break;
                default:
                    col = Color.FromRgb(0x2, 0xbb, 0xff);
                    img.Source = BasicTeraData.Instance.ImageDatabase.Info.Source;
                    ell.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;
                    break;
            }

            //rect.Stroke = new SolidColorBrush(col);
            //(rect.Effect as DropShadowEffect).Color = col;
            DHeight = ActualHeight;
            ((UIElement)this).FindLogicalAncestor<PopupNotification>().DHeight += DHeight;
            var an = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            this.BeginAnimation(OpacityProperty, an);
            _close.Start();
        }

        private void _context_RefreshEvent(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Refresh")
            {
                _close?.Stop();
                _close?.Start();
            }
            else if (e.PropertyName == "BodyText")
            {
                TextBlock.Text = _context.BodyText;
                var par = ((UIElement) this).FindLogicalAncestor<PopupNotification>();
                par.DHeight -= DHeight;
                DHeight = ActualHeight;
                par.DHeight += DHeight;
            }
        }

        private void _close_Elapsed(object sender, ElapsedEventArgs e)
        {
            _close.Stop();
            Dispatcher.InvokeIfRequired(() =>
            {
                var h = this.ActualHeight;
                var an = new DoubleAnimation(h, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuadraticEase() };
                an.Completed += (s, ev) =>
                {
                    var parent = ((UIElement)this).FindLogicalAncestor<PopupNotification>();
                    parent?.RemoveMe(_context);
                    if (parent != null) parent.DHeight -= DHeight;
                    _close.Dispose();
                };
                //this.BeginAnimation(WidthProperty, an);
                this.BeginAnimation(HeightProperty, an);
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }
    }
}
