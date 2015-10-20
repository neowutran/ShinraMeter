using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Tera.Data;
using Tera.Protocol.Game;
using Tera.Sniffer;
using Message = Tera.Protocol.Message;

namespace Tera.DamageMeter
{
    public partial class DamageMeterForm : Form
    {
        private TeraSniffer _teraSniffer;
        private static readonly TeraData _teraData = new TeraData();
        private readonly Dictionary<PlayerStats, PlayerStatsControl> _controls = new Dictionary<PlayerStats, PlayerStatsControl>();
        private readonly MessageFactory _messageFactory = new MessageFactory(_teraData.OpCodeNamer);
        private EntityRegistry _entityRegistry;
        private DamageTracker _damageTracker;

        public DamageMeterForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _teraSniffer = new TeraSniffer(_teraData.ServerIps);
            _teraSniffer.MessageReceived += teraSniffer_MessageReceived;
            _teraSniffer.NewConnection += _teraSniffer_NewConnection;

            _teraSniffer.Enabled = true;
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

        public void Fetch(IEnumerable<PlayerStats> playerStatsSequence)
        {
            playerStatsSequence = playerStatsSequence.OrderByDescending(playerStats => playerStats.Dealt.Damage + playerStats.Dealt.Heal);
            var totalDamage = playerStatsSequence.Sum(playerStats => playerStats.Dealt.Damage);
            TotalDamageLabel.Text = Helpers.FormatValue(totalDamage);
            TotalDamageLabel.Left = HeaderPanel.Width - TotalDamageLabel.Width;
            int pos = 0;
            var visiblePlayerStats = new HashSet<PlayerStats>();
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
                    playerStatsControl.Width = ListPanel.Width;
                    _controls.Add(playerStats, playerStatsControl);
                    playerStatsControl.Parent = ListPanel;
                }
                playerStatsControl.Top = pos;
                pos += playerStatsControl.Height;
                playerStatsControl.Fetch(playerStats, totalDamage);
            }

            var invisibleControls = _controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
            foreach (var invisibleControl in invisibleControls)
            {
                invisibleControl.Value.Dispose();
                _controls.Remove(invisibleControl.Key);
            }
        }

        private void _teraSniffer_NewConnection()
        {
            InvokeAction(() =>
                {
                    if (!Text.Contains("connected"))
                        Text += " connected";
                    _entityRegistry = new EntityRegistry();
                    _damageTracker = new DamageTracker(_entityRegistry);
                });
        }

        private void teraSniffer_MessageReceived(Message obj)
        {
            InvokeAction(() =>
                {
                    var message = _messageFactory.Create(obj);
                    _entityRegistry.Update(message);

                    var skillResultMessage = message as EachSkillResultServerMessage;
                    if (skillResultMessage != null)
                    {
                        _damageTracker.Update(skillResultMessage);
                    }
                });
        }

        private void DamageMeterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _teraSniffer.Enabled = false;
        }
        
        private void ResetButton_Click(object sender, EventArgs e)
        {
            _damageTracker = new DamageTracker(_entityRegistry);
            Fetch();
        }

        private void RefershTimer_Tick(object sender, EventArgs e)
        {
            Fetch();
        }
    }
}
