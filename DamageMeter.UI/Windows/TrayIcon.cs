using System.Drawing;
using DamageMeter.AutoUpdate;
using DamageMeter.UI.Windows;
using Data;

namespace DamageMeter.UI
{
    public class TrayIcon
    {
        private readonly System.Windows.Forms.NotifyIcon _trayIcon;
        public string Text
        {
            get => _trayIcon.Text;
            set => _trayIcon.Text = value;
        }
        public Icon Icon
        {
            get => _trayIcon.Icon;
            set => _trayIcon.Icon = value;
        }

        public TrayIcon()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Icon = BasicTeraData.Instance.ImageDatabase.Tray,
                Text = "Shinra Meter v" + UpdateManager.Version

            };

            _trayIcon.MouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            SettingsWindow.ShowWindow();
        }

        public void Dispose()
        {
            _trayIcon.Dispose();
        }

    }
}