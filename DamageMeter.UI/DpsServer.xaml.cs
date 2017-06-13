using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lang;
using Xceed.Wpf.AvalonDock.Controls;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour DpsServer.xaml
    /// </summary>
    public partial class DpsServer : UserControl
    {

        private DamageMeter.TeraDpsApi.DpsServer _server;
        private NotifyIcon _icon;
        public DpsServer(DamageMeter.TeraDpsApi.DpsServer server, NotifyIcon parent)
        {
            InitializeComponent();
            _server = server;
            _icon = parent;
        }

        private DpsServerData _data = null;

        private void HideShowSettings(bool b)
        {
            DoubleAnimation an;
            if (b)
            {
                an = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() };
            }
            else
            {
                an = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() };
            }
            settingsGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            rect.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
        }

        public void SetData(DpsServerData data)
        {
            _data = data;
            Enabled.Content = data?.HostName ?? LP.Bad_server_url;
            HideShowSettings(data.Enabled);
            Enabled.Status = data.Enabled;
            AuthTokenTextbox.Text = data.Token;
            UsernameTextbox.Text = data.Username;
            ServerURLTextbox.Text = data.UploadUrl?.ToString();
            AllowedAreaUrlTextbox.Text = data.AllowedAreaUrl?.ToString();
            GlyphUploadUrlTextbox.Text = data.GlyphUrl?.ToString();
        }

        public void RemoveServer()
        {
            BasicTeraData.Instance.WindowData.DpsServers.Remove(_data);
            DataExporter.DpsServers.Remove(_server);
            _icon.DpsServers.Children.Remove(this);
        }

        private void UsernameTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            _data.Username = UsernameTextbox.Text;
        }

        private void UsernameTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            _data.Username = UsernameTextbox.Text;
        }

        private void Enabled_On(object sender, RoutedEventArgs e)
        {
            _data.Enabled = true;
            HideShowSettings(true);
        }

        private void Enabled_Off(object sender, RoutedEventArgs e)
        {
            _data.Enabled = false;
            HideShowSettings(false);
        }

        private void AuthTokenTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            _data.Token = AuthTokenTextbox.Text;
        }

        private void AuthTokenTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            _data.Token = AuthTokenTextbox.Text;
        }

        private void ServerURLTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _data.UploadUrl = new Uri(ServerURLTextbox.Text);
                ServerURLTextbox.Background = new SolidColorBrush(Color.FromArgb(11, 211, 211, 211));
                Enabled.Content = _data.HostName;
            }
            catch
            {
                ServerURLTextbox.Background = new SolidColorBrush(Color.FromArgb(150, 211, 10, 10));
                Enabled.Content = LP.Bad_server_url;
            }
        }

        private void ServerURLTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { ServerURLTextbox_LostFocus(this, new RoutedEventArgs()); }
        }

        private void AllowedAreaUrlTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _data.AllowedAreaUrl = new Uri(AllowedAreaUrlTextbox.Text);
                AllowedAreaUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(11, 211, 211, 211));
            }
            catch { AllowedAreaUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(150, 211, 10, 10)); }
        }

        private void AllowedAreaUrlTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { AllowedAreaUrlTextbox_LostFocus(this, new RoutedEventArgs()); }
        }

        private void GlyphUploadUrlTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _data.GlyphUrl = new Uri(GlyphUploadUrlTextbox.Text);
                GlyphUploadUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(11, 211, 211, 211));
            }
            catch { GlyphUploadUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(150, 211, 10, 10)); }
        }

        private void GlyphUploadUrlTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { GlyphUploadUrlTextbox_LostFocus(this, new RoutedEventArgs()); }
        }

        private void RemoveServerButton_OnClick(object sender, RoutedEventArgs e){

            var an = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            an.Completed += (s,ev) => RemoveServer();
            root.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RemoveServerButtonImage.Source = BasicTeraData.Instance.ImageDatabase.Delete.Source;
        }
    }
}
