using System;
using System.Diagnostics;
using System.Windows.Input;
using DamageMeter.UI.Windows;
using Data;
using Lang;
using Nostrum;

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
}