using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Tera.Data;
using Tera.Game;
using Tera.Game.Messages;
using Tera.PacketLog;
using Tera.Sniffing;
using Message = Tera.Message;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Text;

namespace Tera.DamageMeter
{
    public partial class DamageMeterForm : Form
    {

        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private TeraSniffer _teraSniffer;
        private static readonly BasicTeraData _basicTeraData = new BasicTeraData();
        private ClassIcons _classIcons;
        private static TeraData _teraData;
        private readonly Dictionary<PlayerInfo, PlayerStatsControl> _controls = new Dictionary<PlayerInfo, PlayerStatsControl>();
        private MessageFactory _messageFactory;
        private EntityTracker _entityRegistry;
        private DamageTracker _damageTracker;
        private Server _server;
        private PlayerTracker _playerTracker;

        public DamageMeterForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _classIcons = new ClassIcons(_basicTeraData.ResourceDirectory + @"class-icons\", 36);
            _teraSniffer = new TeraSniffer(_basicTeraData.Servers);
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
             
            playerStatsSequence = playerStatsSequence.OrderByDescending(playerStats => playerStats.Dealt.Damage + playerStats.Dealt.Heal);
            var totalDamage = playerStatsSequence.Sum(playerStats => playerStats.Dealt.Damage);
            TotalDamageLabel.Text = Helpers.FormatValue(totalDamage);
            TotalDamageLabel.Left = HeaderPanel.Width - TotalDamageLabel.Width;
            int pos = 0;
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
                    playerStatsControl = new PlayerStatsControl();
                    playerStatsControl.PlayerInfo = playerStats;
                    playerStatsControl.Height = 40;
                    _controls.Add(playerStats, playerStatsControl);
                    playerStatsControl.Parent = ListPanel;
                    playerStatsControl.ClassIcons = _classIcons;
                }
                playerStatsControl.Top = pos;
                playerStatsControl.Width = ListPanel.Width;
                pos += playerStatsControl.Height + 2;
                playerStatsControl.TotalDamage = totalDamage;
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
            _teraData = _basicTeraData.DataForRegion(server.Region);
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
            _teraSniffer.Enabled = false;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            if (_server == null)
                return;
            _damageTracker = new DamageTracker(_entityRegistry, _playerTracker, _teraData.SkillDatabase);
            Fetch();
        }

        private void RefershTimer_Tick(object sender, EventArgs e)
        {
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

     
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]

        public static extern IntPtr FindWindow(IntPtr ZeroOnly, string lpWindowName);



        [DllImport("user32.dll")]

        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);



        [DllImport("user32.dll")]

        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool SetForegroundWindow(IntPtr hWnd);

        private void PasteButton_Click(object sender, EventArgs e)
        {
            IntPtr teraHandle = FindWindow(null, "TERA");
            if (teraHandle == IntPtr.Zero)
            {
                MessageBox.Show("TERA is not running.");
                return;
            }
            var text = Clipboard.GetText();

            SetForegroundWindow(teraHandle);

            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(300);
            const int cr = 13;
            const int lf = 10;

            //this char cause trouble with the game
            const int percentage = '%';

            char[] specialChars = { '{', '}', '(', ')', '+', '^'};
            foreach (char c in text.Where(c => (int) c != lf && (int) c != cr && (int) c != percentage))
            {
                if (specialChars.Contains(c))
                {
                    Console.WriteLine("#### SPE ###");
                    Console.WriteLine(c);
                    Console.WriteLine((int)c);
                    SendKeys.SendWait("{" + c + "}");
                }
                else
                {
                    Console.WriteLine("#### NOR ###");
                    Console.WriteLine(c);
                    Console.WriteLine((int)c);
                    SendKeys.SendWait(c + "");
                }
                System.Threading.Thread.Sleep(10);
           
            }
       
        }

        //COPY TO CLIPBOARD FUNCTION
        private void button1_Click(object sender, EventArgs e)
        {
           //stop if nothing to paste
            if (_damageTracker == null) return;

            IEnumerable<PlayerInfo> playerStatsSequence = _damageTracker;
            playerStatsSequence = playerStatsSequence.OrderByDescending(playerStats => playerStats.Dealt.Damage + playerStats.Dealt.Heal);
            var dpsString = "";
            foreach (var playerStats in playerStatsSequence)
            {
                PlayerStatsControl playerStatsControl;
                _controls.TryGetValue(playerStats, out playerStatsControl);
                var damageFraction = (double)playerStats.Dealt.Damage / playerStatsControl.TotalDamage;
                var dpsResult = String.Format("|{0}| {1}{{%}} {{(}}{2}{{)}}; ", playerStats.Name, Math.Round(damageFraction * 100.0, 2), Helpers.FormatValue(playerStats.Dealt.Damage));
                dpsString += dpsResult;
            }
            Console.WriteLine(dpsString);
            Clipboard.SetText(dpsString);

        }

    }
}