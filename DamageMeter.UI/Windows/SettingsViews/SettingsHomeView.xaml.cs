using DamageMeter.UI.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DamageMeter.UI.Windows.SettingsViews
{
    public partial class SettingsHomeView : INotifyPropertyChanged
    {
        public SettingsHomeView()
        {
            InitializeComponent();
        }

        private bool _gitHubPopupVisible;
        private bool _exportPopupVisible;

        public bool ExportPopupVisible
        {
            get => _exportPopupVisible;
            set
            {
                if (_exportPopupVisible == value) return;
                _exportPopupVisible = value;
                N();
            }
        }
        public bool GitHubPopupVisible
        {
            get => _gitHubPopupVisible;
            set
            {
                if (_gitHubPopupVisible == value) return;
                _gitHubPopupVisible = value;
                N();
            }
        }

        private void OnGitHubPopupMouseLeave(object sender, MouseEventArgs e)
        {
            GitHubPopupVisible = false;
        }

        private void OnGitHubGridMouseEnter(object sender, MouseEventArgs e)
        {
            GitHubPopupVisible = true;
        }

        private void OnExportGridMouseEnter(object sender, MouseEventArgs e)
        {
            ExportPopupVisible = true;
        }

        private void OnExportPopupMouseLeave(object sender, MouseEventArgs e)
        {
            ExportPopupVisible = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void N([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
