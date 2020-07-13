using DamageMeter.UI.Windows;
using Data;
using Lang;
using Nostrum;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace DamageMeter.UI
{

    public class DpsServerViewModel : TSPropertyChanged
    {
        public TeraDpsApi.DpsServer Server { get; }

        private bool _isUploadUrlValid;
        private bool _isWhitelistUrlValid;
        private bool _isGlyphUrlValid;

        public string HostName => Server.Data.HostName == null || !IsUploadUrlValid ? LP.Bad_server_url : Server.Data.HostName;
        public string HomeUrl => Server.HomeUrl.ToString();

        public bool Enabled
        {
            get => Server.Data.Enabled;
            set
            {
                if (Server.Data.Enabled == value) return;
                Server.Data.Enabled = value;
                NotifyPropertyChanged();
            }
        }
        public string Token
        {
            get => Server.Data.Token;
            set
            {
                if (Server.Data.Token == value) return;
                Server.Data.Token = value;
                NotifyPropertyChanged();
            }
        }
        public string UserName
        {
            get => Server.Data.Username;
            set
            {
                if (Server.Data.Username == value) return;
                Server.Data.Username = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsUploadUrlValid
        {
            get => _isUploadUrlValid;
            set
            {
                if (_isUploadUrlValid == value) return;
                _isUploadUrlValid = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsWhitelistUrlValid
        {
            get => _isWhitelistUrlValid;
            set
            {
                if (_isWhitelistUrlValid == value) return;
                _isWhitelistUrlValid = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsGlyphUrlValid
        {
            get => _isGlyphUrlValid;
            set
            {
                if (_isGlyphUrlValid == value) return;
                _isGlyphUrlValid = value;
                NotifyPropertyChanged();
            }
        }

        public string UploadUrl
        {
            get => Server.Data.UploadUrl?.ToString();
            set
            {
                if (Server.Data.UploadUrl?.ToString() == value)
                {
                    IsUploadUrlValid = true;
                    return;
                }
                try
                {
                    Server.Data.UploadUrl = new Uri(value);
                    NotifyPropertyChanged();
                    IsUploadUrlValid = true;
                }
                catch
                {
                    IsUploadUrlValid = false;
                }

                NotifyPropertyChanged(nameof(HomeUrl));
                NotifyPropertyChanged(nameof(HostName));
            }
        }
        public string WhitelistUrl
        {
            get => Server.Data.AllowedAreaUrl?.ToString();
            set
            {
                if (Server.Data.AllowedAreaUrl?.ToString() == value)
                {
                    IsWhitelistUrlValid = true;
                    return;
                }
                try
                {
                    Server.Data.AllowedAreaUrl = new Uri(value);
                    NotifyPropertyChanged();
                    IsWhitelistUrlValid = true;
                }
                catch
                {
                    IsWhitelistUrlValid = false;
                }
            }
        }
        public string GlyphUrl
        {
            get => Server.Data.GlyphUrl?.ToString();
            set
            {
                if (Server.Data.GlyphUrl?.ToString() == value)
                {
                    IsGlyphUrlValid = true;
                    return;
                }
                try
                {
                    Server.Data.GlyphUrl = new Uri(value);
                    NotifyPropertyChanged();
                    IsGlyphUrlValid = true;
                }
                catch
                {
                    IsGlyphUrlValid = false;
                }

            }
        }

        public ICommand BrowseUrlCommand { get; }
        public ICommand RemoveServerCommand { get; }

        public DpsServerViewModel(TeraDpsApi.DpsServer server)
        {
            Server = server;
            NotifyPropertyChanged(nameof(HostName));
            NotifyPropertyChanged(nameof(HomeUrl));

            IsUploadUrlValid = server.UploadUrl != null;
            IsGlyphUrlValid = server.GlyphUrl!= null;
            IsWhitelistUrlValid= server.AllowedAreaUrl!= null;

            BrowseUrlCommand = new RelayCommand(_ =>
            {
                if (Server.HomeUrl == null) return;
                Process.Start("explorer.exe", HomeUrl);
            });

            RemoveServerCommand = new RelayCommand(_ =>
            {
                BasicTeraData.Instance.WindowData.DpsServers.Remove(Server.Data);
                DataExporter.DpsServers.Remove(Server);
                SettingsWindowViewModel.NotifyServerRemoved(this);
            });
        }
    }

    public partial class DpsServer : UserControl
    {

        //private TeraDpsApi.DpsServer _server;
        //private DpsServerData _data = null;
        //private NotifyIcon _icon;

        public DpsServer()
        {
            InitializeComponent();
        }

        public DpsServer(TeraDpsApi.DpsServer server) : this()
        {
            //_server = server;
            //_icon = parent;
            //SetData(_server.Data);
        }

        //private void HideShowSettings(bool b)
        //{
        //    DoubleAnimation an;
        //    an = b 
        //        ? new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() } 
        //        : new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() };
        //    settingsGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
        //}

        //public void SetData(DpsServerData data)
        //{
        //    //_data = data;
        //    //ServerLabel.Text = data.HostName ?? LP.Bad_server_url;
        //    HideShowSettings(data.Enabled);
        //    //Enabled.Status = data.Enabled;
        //    //AuthTokenTextbox.Text = data.Token;
        //    //UsernameTextbox.Text = data.Username;
        //    //ServerURLTextbox.Text = data.UploadUrl?.ToString();
        //    //AllowedAreaUrlTextbox.Text = data.AllowedAreaUrl?.ToString();
        //    //GlyphUploadUrlTextbox.Text = data.GlyphUrl?.ToString();
        //    LinkIcon.ToolTip = _server.HomeUrl;
        //}

        //public void RemoveServer()
        //{
        //    BasicTeraData.Instance.WindowData.DpsServers.Remove(_data);
        //    DataExporter.DpsServers.Remove(_server);
        //    //TODO: _icon.DpsServers.Children.Remove(this);
        //}

        //private void UsernameTextbox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    _data.Username = UsernameTextbox.Text;
        //}

        //private void UsernameTextbox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    _data.Username = UsernameTextbox.Text;
        //}

        //private void Enabled_On(object sender, RoutedEventArgs e)
        //{
        //    _data.Enabled = true;
        //    HideShowSettings(true);
        //}

        //private void Enabled_Off(object sender, RoutedEventArgs e)
        //{
        //    _data.Enabled = false;
        //    HideShowSettings(false);
        //}

        //private void AuthTokenTextbox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    _data.Token = AuthTokenTextbox.Text;
        //}

        //private void AuthTokenTextbox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    _data.Token = AuthTokenTextbox.Text;
        //}

        //private void ServerURLTextbox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        _data.UploadUrl = new Uri(ServerURLTextbox.Text);
        //        ServerURLTextbox.Background = new SolidColorBrush(Color.FromArgb(11, 211, 211, 211));
        //        ServerLabel.Text = _data.HostName;
        //        LinkIcon.ToolTip = _server.HomeUrl;
        //    }
        //    catch
        //    {
        //        ServerURLTextbox.Background = new SolidColorBrush(Color.FromArgb(150, 211, 10, 10));
        //        ServerLabel.Text = LP.Bad_server_url;
        //    }
        //}

        //private void ServerURLTextbox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter) { ServerURLTextbox_LostFocus(this, new RoutedEventArgs()); }
        //}

        //private void AllowedAreaUrlTextbox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        _data.AllowedAreaUrl = new Uri(AllowedAreaUrlTextbox.Text);
        //        AllowedAreaUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(11, 211, 211, 211));
        //    }
        //    catch { AllowedAreaUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(150, 211, 10, 10)); }
        //}

        //private void AllowedAreaUrlTextbox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter) { AllowedAreaUrlTextbox_LostFocus(this, new RoutedEventArgs()); }
        //}

        //private void GlyphUploadUrlTextbox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        _data.GlyphUrl = new Uri(GlyphUploadUrlTextbox.Text);
        //        GlyphUploadUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(11, 211, 211, 211));
        //    }
        //    catch { GlyphUploadUrlTextbox.Background = new SolidColorBrush(Color.FromArgb(150, 211, 10, 10)); }
        //}

        //private void GlyphUploadUrlTextbox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter) { GlyphUploadUrlTextbox_LostFocus(this, new RoutedEventArgs()); }
        //}

        //private void RemoveServerButton_OnClick(object sender, RoutedEventArgs e)
        //{

        //    var an = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
        //    an.Completed += (s, ev) => RemoveServer();
        //    root.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, an);
        //}

        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    RemoveServerButtonImage.Source = BasicTeraData.Instance.ImageDatabase.Delete.Source;
        //    LinkIcon.Source = BasicTeraData.Instance.ImageDatabase.Links.Source;
        //}

        //private void LinkIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //if (_server.HomeUrl != null) Process.Start("explorer.exe", _server.HomeUrl.ToString());
        //}
        private void OnTbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Keyboard.ClearFocus();
        }
    }
}
