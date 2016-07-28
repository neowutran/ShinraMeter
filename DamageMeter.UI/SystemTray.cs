using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using DamageMeter.AutoUpdate;
using Data;

namespace DamageMeter.UI
{
    public class SystemTray
    {

        private readonly MainWindow _mainWindow;

        public void UpdatePacketWaiting(int packetWaiting)
        {
            _packetWaitingLabel.Text = packetWaiting + "/3000 before crash";
            _packetWaitingProgressBar.Value = packetWaiting;
            _packetWaitingProgressBar.Text = "Packet waiting to be processed: " + packetWaiting;
        }

        private readonly ToolStripProgressBar _packetWaitingProgressBar;
        private readonly ToolStripLabel _packetWaitingLabel;

        public SystemTray(MainWindow windows)
        {
            _mainWindow = windows;
            TrayIcon = new NotifyIcon
            {
                Icon = BasicTeraData.Instance.ImageDatabase.Tray,
                Visible = true,
                Text = "Shinra Meter V" + UpdateManager.Version + ": No server"
            };
            TrayIcon.Click += TrayIconOnClick;
            TrayIcon.DoubleClick += _trayIcon_DoubleClick;
            _packetWaitingProgressBar = new ToolStripProgressBar
            {
                Minimum = 0,
                Maximum = 3000,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };
            _packetWaitingLabel = new ToolStripLabel();
           
            var excel_current = new ToolStripMenuItem { Text = "Export current to Excel" };
            excel_current.Click += ExcelExportOnClick;
            var reset = new ToolStripMenuItem { Text = "Reset" };
            reset.Click += ResetOnClick;
            var exit = new ToolStripMenuItem { Text = "Close" };
            exit.Click += ExitOnClick;
            var wiki = new ToolStripMenuItem { Text = "Wiki" };
            wiki.Click += WikiOnClick;
            var patch = new ToolStripMenuItem { Text = "Patch note" };
            patch.Click += PatchOnClick;
            var issues = new ToolStripMenuItem { Text = "Report issue" };
            issues.Click += IssuesOnClick;
            var forum = new ToolStripMenuItem { Text = "Forum" };
            forum.Click += ForumOnClick;
            var teradps = new ToolStripMenuItem { Text = "TeraDps.io" };
            teradps.Click += TeraDpsOnClick;
            var excel = new ToolStripMenuItem { Text = "Autoexport to Excel" };
            excel.Click += ExcelOnClick;
            excel.Checked = BasicTeraData.Instance.WindowData.Excel;
            var onlyBoss = new ToolStripMenuItem { Text = "Count only bosses" };
            onlyBoss.Click += onlyBossOnClick;
            onlyBoss.Checked = BasicTeraData.Instance.WindowData.OnlyBoss;
            var detectBosses = new ToolStripMenuItem { Text = "Detect bosses by HP bar (ignore db)" };
            detectBosses.Click += detectBossesOnClick;
            detectBosses.Checked = BasicTeraData.Instance.WindowData.OnlyBoss;
            var siteExport = new ToolStripMenuItem { Text = "Site export" };
            siteExport.Click += SiteOnClick;
            siteExport.Checked = BasicTeraData.Instance.WindowData.SiteExport;
            var party = new ToolStripMenuItem { Text = "Count only party members" };
            party.Click += PartyOnClick;
            party.Checked = BasicTeraData.Instance.WindowData.PartyOnly;
            var stayTop = new ToolStripMenuItem
            {
                Text = "Stay topmost",
                Checked = BasicTeraData.Instance.WindowData.Topmost
            };

            stayTop.Click += SwitchStayTop;

            ClickThrou = new ToolStripMenuItem { Text = "Click throu" };
            ClickThrou.Click += ClickThrouOnClick;
            _switchNoStatsVisibility = new ToolStripMenuItem { Text = "Invisible when no stats" };
            _switchNoStatsVisibility.Click += SwitchNoStatsVisibility;
            _switchNoStatsVisibility.Checked = BasicTeraData.Instance.WindowData.InvisibleUi;
            _alwaysOn = new ToolStripMenuItem { Text = "Show always" };
            _alwaysOn.Click += _trayIcon_DoubleClick;
            _alwaysOn.Checked = BasicTeraData.Instance.WindowData.AlwaysVisible;

            var link = new ToolStripMenuItem { Text = "Links" };
            link.DropDownItems.AddRange(new ToolStripItemCollection(new ToolStrip(), new ToolStripItem[]
            {
                wiki,
                patch,
                issues,
                forum,
                teradps
            } ));

            var config = new ToolStripMenuItem { Text = "Config" };
            config.DropDownItems.AddRange(new ToolStripItemCollection(new ToolStrip(), new ToolStripItem[]
            {
                excel,
                siteExport,
                detectBosses,
                onlyBoss,
                party,
                stayTop,
                ClickThrou,
                _switchNoStatsVisibility,
                _alwaysOn
            }));

            var action = new ToolStripMenuItem { Text = "Action" };
            action.DropDownItems.AddRange(new ToolStripItemCollection(new ToolStrip(), new ToolStripItem[]
            {
                reset,
                excel_current
            }));

            var perf = new ToolStripMenuItem { Text = "Performance information" };
            perf.DropDownItems.AddRange(new ToolStripItemCollection(new ToolStrip(), new ToolStripItem[]
            {
                _packetWaitingLabel,
                _packetWaitingProgressBar
            }));

            var context = new ContextMenuStrip();
            context.Items.Add(perf);
            context.Items.Add(action);
            context.Items.Add(config);
            context.Items.Add(link);
            context.Items.Add(new ToolStripSeparator());
            context.Items.Add(exit);
            TrayIcon.ContextMenuStrip = context;
            
        }

        internal NotifyIcon TrayIcon { get; }

        private ToolStripMenuItem _switchNoStatsVisibility;
        private readonly ToolStripMenuItem _alwaysOn;

        internal ToolStripMenuItem ClickThrou { get; }

        private static void ClickThrouOnClick(object sender, EventArgs eventArgs)
        {
            NetworkController.Instance.SwitchClickThrou();
        }

        private void PatchOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki/Patch-note");
        }

        private void TeraDpsOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe", "http://teradps.io");
        }

        private void ExitOnClick(object sender, EventArgs eventArgs)
        {
            _mainWindow.VerifyClose();
        }

        private void ResetOnClick(object sender, EventArgs eventArgs)
        {
            NetworkController.Instance.NeedToReset = true;
        }

        private void ExcelExportOnClick(object sender, EventArgs eventArgs)
        {
            NetworkController.Instance.NeedToExport = true;
        }

        private void ForumOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe",
                "https://discord.gg/0wjLnPs6HoNFxv6O");
        }

        private void IssuesOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/issues");
        }

        private void WikiOnClick(object sender, EventArgs eventArgs)
        {
            Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki");
        }

        private void SwitchNoStatsVisibility(object sender, EventArgs eventArgs)
        {
            var invisibleUi = BasicTeraData.Instance.WindowData.InvisibleUi;
            BasicTeraData.Instance.WindowData.InvisibleUi = !invisibleUi;
            ((ToolStripMenuItem)sender).Checked = BasicTeraData.Instance.WindowData.InvisibleUi;
            if (_mainWindow.ForceWindowVisibilityHidden) return;

            if (invisibleUi)
            {
                _mainWindow.Visibility = Visibility.Visible;
            }
            else
            {
                _mainWindow.Visibility = _mainWindow.Controls.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void TrayIconOnClick(object sender, EventArgs eventArgs)
        {
            /* //no need
            var e = (MouseEventArgs)eventArgs;
            if (e.Button.ToString() == "Right")
            {
                return;
            }
            */
            _mainWindow.StayTopMost();
        }


        private void detectBossesOnClick(object sender, EventArgs eventArgs)
        {
            BasicTeraData.Instance.WindowData.DetectBosses = !BasicTeraData.Instance.WindowData.DetectBosses;
            ((ToolStripMenuItem)sender).Checked = BasicTeraData.Instance.WindowData.DetectBosses;
            if (BasicTeraData.Instance.MonsterDatabase != null) BasicTeraData.Instance.MonsterDatabase.DetectBosses = BasicTeraData.Instance.WindowData.DetectBosses;
        }
        private void onlyBossOnClick(object sender, EventArgs eventArgs)
        {
            BasicTeraData.Instance.WindowData.OnlyBoss = !BasicTeraData.Instance.WindowData.OnlyBoss;
            ((ToolStripMenuItem)sender).Checked = BasicTeraData.Instance.WindowData.OnlyBoss;
        }

        private void SiteOnClick(object sender, EventArgs eventArgs)
        {
            BasicTeraData.Instance.WindowData.SiteExport = !BasicTeraData.Instance.WindowData.SiteExport;
            ((ToolStripMenuItem)sender).Checked = BasicTeraData.Instance.WindowData.SiteExport;
        }

        private void PartyOnClick(object sender, EventArgs eventArgs)
        {
            BasicTeraData.Instance.WindowData.PartyOnly = !BasicTeraData.Instance.WindowData.PartyOnly;
            ((ToolStripMenuItem)sender).Checked = BasicTeraData.Instance.WindowData.PartyOnly;
        }

        private void ExcelOnClick(object sender, EventArgs eventArgs)
        {
            BasicTeraData.Instance.WindowData.Excel = !BasicTeraData.Instance.WindowData.Excel;
            ((ToolStripMenuItem)sender).Checked = BasicTeraData.Instance.WindowData.Excel;
        }

        private void _trayIcon_DoubleClick(object sender, EventArgs e)
        {
            BasicTeraData.Instance.WindowData.AlwaysVisible = !BasicTeraData.Instance.WindowData.AlwaysVisible;
            _alwaysOn.Checked = BasicTeraData.Instance.WindowData.AlwaysVisible;
        }

        private void SwitchStayTop(object sender, EventArgs e)
        {
            BasicTeraData.Instance.WindowData.Topmost = !BasicTeraData.Instance.WindowData.Topmost;
            ((ToolStripMenuItem)sender).Checked = BasicTeraData.Instance.WindowData.Topmost;
            _mainWindow.Topmost = BasicTeraData.Instance.WindowData.Topmost;
        }
    }
}
