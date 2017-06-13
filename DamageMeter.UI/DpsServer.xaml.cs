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

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour DpsServer.xaml
    /// </summary>
    public partial class DpsServer : UserControl
    {

        private Guid _guid;
        public DpsServer(Guid guid)
        {
            InitializeComponent();
            _guid = guid;
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
            usernameGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            tokenGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            dpsUploadGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            glyphUploadGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
            allowedAreasGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);

        }
        public void SetData(DpsServerData data)
        {
            _data = data;
            Enabled.Content = data.HostName;
            HideShowSettings(data.Enabled);
            Enabled.Status = data.Enabled;
            AuthTokenTextbox.Text = data.Token;
            UsernameTextbox.Text = data.Username;
            ServerURLTextbox.Text = data.UploadUrl.ToString();
            AllowedAreaUrlTextbox.Text = data.AllowedAreaUrl?.ToString();
            GlyphUploadUrlTextbox.Text = data.GlyphUrl?.ToString();
            
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
            }
            catch { ServerURLTextbox.Background = new SolidColorBrush(Color.FromArgb(150, 211, 10, 10)); }
            Enabled.Content = _data.HostName;
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
    }
}
