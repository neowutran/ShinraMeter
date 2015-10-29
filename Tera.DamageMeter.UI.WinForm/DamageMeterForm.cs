using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tera.DamageMeter.UI.Handler;
using Tera.Data;
using Tera.Game;
using Tera.Game.Messages;
using Tera.PacketLog;
using Tera.Sniffing;

namespace Tera.DamageMeter
{
    public partial class DamageMeterForm : Form
    {
        private static readonly BasicTeraData BasicTeraData = new BasicTeraData();
        private static TeraData _teraData;

        private readonly Dictionary<PlayerInfo, PlayerStatsControl> _controls =
            new Dictionary<PlayerInfo, PlayerStatsControl>();

        private ClassIcons _classIcons;
        private DamageTracker _damageTracker;
        private EntityTracker _entityRegistry;
        private MessageFactory _messageFactory;
        private PlayerTracker _playerTracker;
        private Server _server;
        private TeraSniffer _teraSniffer;

        private readonly KeyboardHook _hook = new KeyboardHook();

        public DamageMeterForm()
        {
            InitializeComponent();
            RegisterKeyboardHook();
        }

        private void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Home:
                    CopyPaste.Paste();
                    break;
                case Keys.End:
                    CopyPaste.Copy(PlayerData());
                    break;
                case Keys.Delete:
                    Reset();
                    break;
            }
        }

        private void RegisterKeyboardHook()
        {
            // register the event that is fired after the key press.
            _hook.KeyPressed +=
                hook_KeyPressed;
            // register the control + alt + F12 combination as hot key.
            try
            {
                _hook.RegisterHotKey(UI.Handler.ModifierKeys.None, Keys.Home);
            }
            catch
            {
                MessageBox.Show("Cannot bind Paste function on key Home");
            }
            try
            {
                _hook.RegisterHotKey(UI.Handler.ModifierKeys.None, Keys.End);
            }
            catch
            {
                MessageBox.Show("Cannot bind Copy function on key End");
            }
            try
            {
                _hook.RegisterHotKey(UI.Handler.ModifierKeys.None, Keys.Delete);
            }
            catch
            {
                MessageBox.Show("Cannot bind Reset function on key Delete");
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        public List<PlayerData> PlayerData()
        {
            return _controls.Select(keyValue => keyValue.Value.PlayerData).ToList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _classIcons = new ClassIcons(BasicTeraData.ResourceDirectory + @"class-icons\", 36);
            _teraSniffer = new TeraSniffer(BasicTeraData.Servers);
            _teraSniffer.MessageReceived += message => InvokeAction(() => HandleMessageReceived(message));
            _teraSniffer.NewConnection += server => InvokeAction(() => HandleNewConnection(server));

            _teraSniffer.Enabled = true;
            UpdateSettingsUi();
        }

        private void InvokeAction(Action action)
        {
            if (IsDisposed)
                return;
            if (!InvokeRequired)
                throw new InvalidOperationException("Expected InvokeRequired");
            Invoke(action);
        }


        public void Fetch()
        {
            if (_damageTracker != null)
            {
                Fetch(_damageTracker);
            }
        }

        public void Fetch(IEnumerable<PlayerInfo> playerStatsSequence)
        {
            playerStatsSequence =
                playerStatsSequence.OrderByDescending(playerStats => playerStats.Dealt.Damage + playerStats.Dealt.Heal);
            var totalDamage = playerStatsSequence.Sum(playerStats => playerStats.Dealt.Damage);
            TotalDamageLabel.Text = Helpers.FormatValue(totalDamage);
            TotalDamageLabel.Left = HeaderPanel.Width - TotalDamageLabel.Width;
            var pos = 0;
            var visiblePlayerStats = new HashSet<PlayerInfo>();
            foreach (var playerStats in playerStatsSequence)
            {
                if (pos > ListPanel.Height)
                    break;

                visiblePlayerStats.Add(playerStats);
                PlayerStatsControl playerStatsControl;
                _controls.TryGetValue(playerStats, out playerStatsControl);
                if (playerStatsControl == null)
                {
                    playerStatsControl = new PlayerStatsControl(playerStats);
                    Console.WriteLine(playerStats);
                    playerStatsControl.Height = 40;
                    _controls.Add(playerStats, playerStatsControl);
                    playerStatsControl.Parent = ListPanel;
                    playerStatsControl.ClassIcons = _classIcons;
                }
                playerStatsControl.Top = pos;
                playerStatsControl.Width = ListPanel.Width;
                pos += playerStatsControl.Height + 2;
                playerStatsControl.PlayerData.TotalDamage = totalDamage;
                playerStatsControl.Invalidate();
            }

            var invisibleControls = _controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
            foreach (var invisibleControl in invisibleControls)
            {
                invisibleControl.Value.Dispose();
                _controls.Remove(invisibleControl.Key);
            }
        }

        public void UpdateSettingsUi()
        {
            alwaysOnTopToolStripMenuItem.Checked = TopMost;
            CaptureMenuItem.Checked = _teraSniffer.Enabled;
        }

        private void HandleNewConnection(Server server)
        {
            Text = $"Damage Meter connected to {server.Name}";
            _server = server;
            _teraData = BasicTeraData.DataForRegion(server.Region);
            _entityRegistry = new EntityTracker();
            _playerTracker = new PlayerTracker(_entityRegistry);
            _damageTracker = new DamageTracker(_entityRegistry, _playerTracker, _teraData.SkillDatabase);
            _messageFactory = new MessageFactory(_teraData.OpCodeNamer);
        }

        private void HandleMessageReceived(Message obj)
        {
            var message = _messageFactory.Create(obj);
            _entityRegistry.Update(message);

            var skillResultMessage = message as EachSkillResultServerMessage;
            if (skillResultMessage != null)
            {
                _damageTracker.Update(skillResultMessage);
            }
        }

        private void DamageMeterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //_teraSniffer.Enabled = false;
            Application.Exit();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            if (_server == null)
                return;
            _damageTracker = new DamageTracker(_entityRegistry, _playerTracker, _teraData.SkillDatabase);
            Fetch();
        }

        private void RefershTimer_Tick(object sender, EventArgs e)
        {
            var hWnd = FindWindow(null, "TERA");
            Rect rect;
            GetWindowRect(hWnd, out rect);
            WindowState = rect.X == -32000 ? FormWindowState.Minimized : FormWindowState.Normal;
            Fetch();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            MainMenu.Show(MenuButton, 0, MenuButton.Height);
        }

        private void OpenPacketLogMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenPacketLogFileDialog.ShowDialog() != DialogResult.OK)
                return;

            _teraSniffer.Enabled = false;
            var server = new Server("Packet Log", "EU", null);
            HandleNewConnection(server);
            foreach (var message in PacketLogReader.ReadMessages(OpenPacketLogFileDialog.FileName))
            {
                HandleMessageReceived(message);
            }
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            UpdateSettingsUi();
        }

        private void CaptureMenuItem_Click(object sender, EventArgs e)
        {
            _teraSniffer.Enabled = !_teraSniffer.Enabled;
            UpdateSettingsUi();
        }


        //https://msdn.microsoft.com/en-us/library/ms171548(v=vs.110).aspx
        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void PasteButton_Click(object sender, EventArgs e)
        {
            var teraHandle = FindWindow(null, "TERA");
            if (teraHandle == IntPtr.Zero)
            {
                MessageBox.Show("TERA is not running.");
                return;
            }
            SetForegroundWindow(teraHandle);
            CopyPaste.Paste();
        }

        //COPY TO CLIPBOARD FUNCTION
        private void button1_Click(object sender, EventArgs e)
        {
            CopyPaste.Copy(PlayerData());
        }


        private void ListPanel_Paint(object sender, PaintEventArgs e)
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }
    }
}