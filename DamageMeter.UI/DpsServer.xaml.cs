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

        public void SetData(DpsServerData data)
        {
            _data = data;
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
        }

        private void Enabled_Off(object sender, RoutedEventArgs e)
        {
            _data.Enabled = false;
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
            _data.UploadUrl = new Uri(ServerURLTextbox.Text);
        }

        private void ServerURLTextbox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void AllowedAreaUrlTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            _data.AllowedAreaUrl = new Uri(AllowedAreaUrlTextbox.Text);
        }

        private void AllowedAreaUrlTextbox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void GlyphUploadUrlTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            _data.GlyphUrl = new Uri(GlyphUploadUrlTextbox.Text);

        }

        private void GlyphUploadUrlTextbox_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
