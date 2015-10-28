using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Tera.Data;
using Tera.Game;
using Tera.Game.Messages;
using Tera.PacketLog;
using Tera.Sniffing;

namespace Tera.DamageMeter
{
    public partial class DamageMeterForm : Form
    {
        private static readonly BasicTeraData _basicTeraData = new BasicTeraData();
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

        public DamageMeterForm()
        {
            InitializeComponent();
        }

        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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
            playerStatsSequence =
                playerStatsSequence.OrderByDescending(playerStats => playerStats.Dealt.Damage + playerStats.Dealt.Heal);
            var totalDamage = playerStatsSequence.Sum(playerStats => playerStats.Dealt.Damage);
            TotalDamageLabel.Text = Helpers.FormatValue(totalDamage);
            TotalDamageLabel.Left = HeaderPanel.Width - TotalDamageLabel.Width;
            var pos = 0;
            var visiblePlayerStats = new HashSet<PlayerInfo>();
            foreach (var playerStats in playerStatsSequence)
            {
                Console.WriteLine("has data");
                Console.WriteLine("totalDamage" + totalDamage);
                if (pos > ListPanel.Height)
                    break;

                visiblePlayerStats.Add(playerStats);
                PlayerStatsControl playerStatsControl;
                _controls.TryGetValue(playerStats, out playerStatsControl);
                if (playerStatsControl == null)
                {
                    playerStatsControl = new PlayerStatsControl();
                    playerStatsControl.PlayerInfo = playerStats;
                    Console.WriteLine(playerStats);
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
            Console.WriteLine("connected");

        }

        private void HandleMessageReceived(Message obj)
        {
            Console.WriteLine("has message");
            Console.WriteLine(obj);
            var message = _messageFactory.Create(obj);
            _entityRegistry.Update(message);

            var skillResultMessage = message as EachSkillResultServerMessage;
            if (skillResultMessage != null)
            {
                _damageTracker.Update(skillResultMessage);
                Console.WriteLine("has infos");
            }
            else
            {
                Console.WriteLine("Has no infos");
            }
            Console.WriteLine(skillResultMessage);
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
            var text = Clipboard.GetText();

            SetForegroundWindow(teraHandle);

            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(300);
            const int cr = 13;
            const int lf = 10;

    
            char[] specialChars = {'{', '}', '(', ')', '+', '^', '%', '~', '[',']'};
            foreach (char c in text.Where(c => (int) c != lf && (int) c != cr))
            {
                if (specialChars.Contains(c))
                {
         
                        Console.WriteLine("#### SPE ###");
                        Console.WriteLine(c);
                        Console.WriteLine((int) c);
                        SendKeys.SendWait("{" + c + "}");
                    
                }
                else
                {
                    Console.WriteLine("#### NOR ###");
                    Console.WriteLine(c);
                    Console.WriteLine((int) c);
                    SendKeys.SendWait(c + "");
                }
                SendKeys.SendWait("{%}");
                SendKeys.Send("{%}");
                SendKeys.Flush();
                Thread.Sleep(10);
            }
        }

        //COPY TO CLIPBOARD FUNCTION
        private void button1_Click(object sender, EventArgs e)
        {
            //stop if nothing to paste
            if (_damageTracker == null) return;

            IEnumerable<PlayerInfo> playerStatsSequence = _damageTracker;
            playerStatsSequence =
                playerStatsSequence.OrderByDescending(playerStats => playerStats.Dealt.Damage + playerStats.Dealt.Heal);
            var dpsString = "";
            foreach (var playerStats in playerStatsSequence)
            {
                PlayerStatsControl playerStatsControl;
                _controls.TryGetValue(playerStats, out playerStatsControl);
                var damageFraction = (double) playerStats.Dealt.Damage/playerStatsControl.TotalDamage;
                var dpsResult = string.Format("|{0}| {1}{{%}} {{(}}{2}{{)}}; ", playerStats.Name,
                    Math.Round(damageFraction*100.0, 2), Helpers.FormatValue(playerStats.Dealt.Damage));
                dpsString += dpsResult;
            }
            Console.WriteLine(dpsString);
            Clipboard.SetText(dpsString);
        }
    }
}