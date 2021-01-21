using DamageMeter.Sniffing;
using Data;
using Nostrum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.UI.EventsEditor;
using Lang;
using Tera.Game;
using Tera.RichPresence;
using Action = System.Action;
using CaptureMode = Data.CaptureMode;

namespace DamageMeter.UI.Windows
{
    public class MockPlayerViewModel : TSPropertyChanged, IPlayerViewModel
    {
        public string Name { get; }
        public string Level { get; } = "";
        public PlayerClass Class { get; }
        public double DamageFactor { get; }
        public PlayerRole Role => MiscUtils.RoleFromClass(Class);
        public bool Increased { get; set; }
        public bool Decreased { get; set; }
        public string GlobalDps { get; set; }
        public string IndividualDps { get; set; }
        public string DamagePerc { get; set; }
        public string DamageDone { get; set; }
        public string CritRate { get; set; }
        public string HealCritRate { get; set; }
        public string DamageFromCrits { get; set; }
        public int Deaths { get; set; }
        public string FloorTime { get; set; }
        public string FloorTimePerc { get; set; }
        public string AggroPerc { get; set; }
        public double EnduDebuffUptime { get; set; }
        public string EnduDebuffUptimeStr { get; set; }

        private List<Metric> _metricSource => Role switch
        {
            PlayerRole.Tank => BasicTeraData.Instance.WindowData.TankMetrics,
            PlayerRole.Healer => BasicTeraData.Instance.WindowData.HealerMetrics,
            _ => BasicTeraData.Instance.WindowData.DpsMetrics,
        };

        public List<Metric> AvailableMetrics
        {
            get
            {
                var ret = EnumUtils.ListFromEnum<Metric>();
                ret.Remove(Metric.Hps);
                ret.Remove(Metric.DamageTaken);
                ret.Remove(Metric.DpsTaken);
                ret.Remove(Metric.EnrageCasts);
                ret.Remove(Metric.HealerUptimes);
                if (Role == PlayerRole.Healer) { return ret; }

                ret.Remove(Metric.EnduDebuffUptime);

                return ret;
            }
        }

        public Metric Metric1
        {
            get => _metricSource[0];
            set
            {
                if (_metricSource[0] == value) return;
                _metricSource[0] = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric2
        {
            get => _metricSource[1];
            set
            {
                if (_metricSource[1] == value) return;
                _metricSource[1] = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric3
        {
            get => _metricSource[2];
            set
            {
                if (_metricSource[2] == value) return;
                _metricSource[2] = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric4
        {
            get => _metricSource[3];
            set
            {
                if (_metricSource[3] == value) return;
                _metricSource[3] = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric5
        {
            get => _metricSource[4];
            set
            {
                if (_metricSource[4] == value) return;
                _metricSource[4] = value;
                NotifyPropertyChanged();
            }
        }

        public MockPlayerViewModel(string name, PlayerClass c)
        {
            var rng = new Random();

            Name = name;
            Class = c;
            DamageFactor = rng.NextDouble();
            GlobalDps = FormatHelpers.Instance.FormatValue(rng.Next(100_000, 40_000_000)) + LP.PerSecond;
            DamagePerc = Math.Round(rng.NextDouble() * 100) + "%";
            DamageDone = FormatHelpers.Instance.FormatValue(rng.Next(100_000_000, 2_000_000_000));
            CritRate = Math.Round(rng.NextDouble() * 100) + "%";
            HealCritRate = Math.Round(rng.NextDouble() * 100) + "%";
            DamageFromCrits = Math.Round(rng.NextDouble() * 100) + "%";
            Deaths = rng.Next(0, 20);
            FloorTime = TimeSpan.FromSeconds(rng.Next(0, 500)).ToString(@"mm\:ss");
            FloorTimePerc = Math.Round(rng.NextDouble() * 100) + "%";
            AggroPerc = Math.Round(rng.NextDouble() * 100) + "%";
            EnduDebuffUptime = rng.NextDouble() * 100;
            EnduDebuffUptimeStr = Math.Round(EnduDebuffUptime) + "%";
            Increased = rng.NextDouble() >= 0.5;
            Decreased = !Increased;
        }
    }


    public class SettingsWindowViewModel : TSPropertyChanged
    {

        private static WindowData Data => BasicTeraData.Instance.WindowData;
        private static HotkeysData Hotkeys => BasicTeraData.Instance.HotkeysData;

        public static IEnumerable<GraphMode> GraphModes = EnumUtils.ListFromEnum<GraphMode>();
        public static IEnumerable<CaptureMode> CaptureModes
        {
            get
            {
                var ret = EnumUtils.ListFromEnum<CaptureMode>();
                if (!Environment.GetCommandLineArgs().Contains("--toolbox")) ret.Remove(CaptureMode.Toolbox);
                return ret;
            }
        }

        public bool CanChangeCaptureMode => !Environment.GetCommandLineArgs().Contains("--toolbox");

        // detection
        //public bool UseNpcap
        //{
        //    get => Data.Winpcap;
        //    set
        //    {
        //        if (Data.Winpcap == value) return;
        //        Data.Winpcap = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        public CaptureMode CaptureMode
        {
            get => Environment.GetCommandLineArgs().Contains("--toolbox") ? CaptureMode.Toolbox : Data.CaptureMode;
            set
            {
                if (Data.CaptureMode == value) return;
                Data.CaptureMode = value;
                NotifyPropertyChanged();
            }
        }
        public bool CountOnlyBosses
        {
            get => Data.OnlyBoss;
            set
            {
                if (Data.OnlyBoss == value) return;
                Data.OnlyBoss = value;
                NotifyPropertyChanged();
            }
        }
        public bool DetectBossesByHpBar
        {
            get => Data.DetectBosses;
            set
            {
                if (Data.DetectBosses == value) return;
                Data.DetectBosses = value;
                if (BasicTeraData.Instance.MonsterDatabase != null) BasicTeraData.Instance.MonsterDatabase.DetectBosses = value;

                NotifyPropertyChanged();
            }
        }
        public bool CountOnlyPartyMembers
        {
            get => Data.PartyOnly;
            set
            {
                if (Data.PartyOnly == value) return;
                Data.PartyOnly = value;
                NotifyPropertyChanged();
            }
        }
        public int ResetTimeout
        {
            get => Data.IdleResetTimeout;
            set
            {
                if (Data.IdleResetTimeout == value) return;
                Data.IdleResetTimeout = value;
                NotifyPropertyChanged();
            }
        }
        public bool IgnorePacketsThreshold
        {
            get => Data.IgnorePacketsThreshold;
            set
            {
                if (Data.IgnorePacketsThreshold == value) return;
                Data.IgnorePacketsThreshold = value;
                NotifyPropertyChanged();
            }
        }
        public bool LowPriority
        {
            get => Data.LowPriority;
            set
            {
                if (Data.LowPriority == value) return;
                Data.LowPriority = value;
                NotifyPropertyChanged();

                Process.GetCurrentProcess().PriorityClass = value
                    ? ProcessPriorityClass.Idle
                    : ProcessPriorityClass.Normal;


            }
        }


        // ui
        public bool AggroBasedTimer
        {
            get => Data.DisplayTimerBasedOnAggro;
            set
            {
                if (Data.DisplayTimerBasedOnAggro == value) return;
                Data.DisplayTimerBasedOnAggro = value;
                NotifyPropertyChanged();
            }
        }
        public bool DisplayOnlyBossHitByMeterUser
        {
            get => Data.DisplayOnlyBossHitByMeterUser;
            set
            {
                if (Data.DisplayOnlyBossHitByMeterUser == value) return;
                Data.DisplayOnlyBossHitByMeterUser = value;
                NotifyPropertyChanged();
            }
        }
        public bool FullscreenOverlay
        {
            get => Data.EnableOverlay;
            set
            {
                if (Data.EnableOverlay == value) return;
                Data.EnableOverlay = value;
                NotifyPropertyChanged();
                // TODO: when dx is fixed
                //if (value)
                //{
                //    if (_mainWindow.DXrender != null) return;
                //    //_mainWindow.DXrender = new Renderer(); ///*** fixme
                //}
                //else
                //{
                //    var render = _mainWindow.DXrender;
                //    _mainWindow.DXrender = null;
                //    render?.Dispose();
                //}
            }
        }
        public bool ClickThru
        {
            get => Data.ClickThrou;
            set
            {
                if (Data.ClickThrou == value) return;
                Data.ClickThrou = value;
                PacketProcessor.Instance.SwitchClickThrou(value);

                NotifyPropertyChanged();
            }
        }
        public bool InvisibleWhenNoStats
        {
            get => Data.InvisibleUi;
            set
            {
                if (Data.InvisibleUi == value) return;
                Data.InvisibleUi = value;
                NotifyPropertyChanged();
                if (App.HudContainer.MainWindow.ForceHidden) return;
                if (value || App.HudContainer.MainWindow.DC.Players.Count > 0)
                {
                    App.HudContainer.MainWindow.ShowWindow();
                }
                else
                {
                    App.HudContainer.MainWindow.HideWindow();
                }
                //App.HudContainer.MainWindow.Visibility = value
                //    ? Visibility.Visible
                //    : App.HudContainer.MainWindow.Controls.Count > 0
                //        ? Visibility.Visible
                //        : Visibility.Hidden;
            }
        }
        public bool ShowAlways
        {
            get => Data.AlwaysVisible;
            set
            {
                if (Data.AlwaysVisible == value) return;
                Data.AlwaysVisible = value;
                NotifyPropertyChanged();
            }
        }
        public bool StayTopmost
        {
            get => Data.Topmost;
            set
            {
                if (Data.Topmost == value) return;
                Data.Topmost = value;
                NotifyPropertyChanged();
                UpdateTopMost();

            }
        }
        public bool ShowSelfOnTop
        {
            get => Data.MeterUserOnTop;
            set
            {
                if (Data.MeterUserOnTop == value) return;
                Data.MeterUserOnTop = value;
                NotifyPropertyChanged();
            }
        }
        public int NumberOfPlayersDisplayed
        {
            get => Data.NumberOfPlayersDisplayed;
            set
            {
                if (Data.NumberOfPlayersDisplayed == value) return;
                Data.NumberOfPlayersDisplayed = value;
                NotifyPropertyChanged();
                NumberOfPlayersDisplayedChanged?.Invoke(value);
            }
        }

        // for Gokoro
        public bool SnapToBorders
        {
            get => Data.SnapToBorders;
            set
            {
                if (Data.SnapToBorders == value) return;
                Data.SnapToBorders = value;
                NotifyPropertyChanged();
            }
        }

        //todo: ability to reset to defaults
        public Color SelfColor
        {
            get => Data.PlayerColor;
            set
            {
                if (Data.PlayerColor == value) return;
                Data.PlayerColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color DpsColor
        {
            get => Data.DpsColor;
            set
            {
                if (Data.DpsColor == value) return;
                Data.DpsColor = value;
                NotifyPropertyChanged();
                MockParty[0].NotifyPropertyChangedEx(nameof(IPlayerViewModel.Role));
            }
        }
        public Color HealerColor
        {
            get => Data.HealerColor;
            set
            {
                if (Data.HealerColor == value) return;
                Data.HealerColor = value;
                NotifyPropertyChanged();
                MockParty[2].NotifyPropertyChangedEx(nameof(IPlayerViewModel.Role));

            }
        }
        public Color TankColor
        {
            get => Data.TankColor;
            set
            {
                if (Data.TankColor == value) return;
                Data.TankColor = value;
                NotifyPropertyChanged();
                MockParty[1].NotifyPropertyChangedEx(nameof(IPlayerViewModel.Role));

            }
        }
        public Color BackgroundColor
        {
            get => Data.BackgroundColor;
            set
            {
                if (Data.BackgroundColor == value) return;
                Data.BackgroundColor = value;
                NotifyPropertyChanged();
                WindowColorsChanged?.Invoke();
            }
        }
        public Color BorderColor
        {
            get => Data.BorderColor;
            set
            {
                if (Data.BorderColor == value) return;
                Data.BorderColor = value;
                NotifyPropertyChanged();
                WindowColorsChanged?.Invoke();
            }
        }

        public double MainWindowOpacity
        {
            get => Data.MainWindowOpacity;
            set
            {
                if (Data.MainWindowOpacity == value) return;
                Data.MainWindowOpacity = value;
                //MainWindowOpacityChanged?.Invoke(value);
                NotifyPropertyChanged();
                MainViewModel.Instance.NotifyPropertyChangedEx(nameof(MainViewModel.WindowOpacity));
            }
        }
        public double OtherWindowsOpacity
        {
            get => Data.OtherWindowOpacity;
            set
            {
                if (Data.OtherWindowOpacity == value) return;
                Data.OtherWindowOpacity = value;
                OtherWindowsOpacityChanged?.Invoke(value);
                NotifyPropertyChanged();
            }
        }
        public double Scale
        {
            get => Data.Scale;
            set
            {
                if (Data.Scale == value) return;
                Data.Scale = value;
                WindowScaleChanged?.Invoke(value);
                NotifyPropertyChanged();
            }
        }

        // layout

        // tts
        public int TtsMaxSize
        {
            get => Data.MaxTTSSize;
            set
            {
                if (Data.MaxTTSSize == value) return;
                Data.MaxTTSSize = value;
                NotifyPropertyChanged();
            }
        }
        public bool TtsMessagesTruncate
        {
            get => Data.TTSSizeExceededTruncate;
            set
            {
                if (Data.TTSSizeExceededTruncate == value) return;
                Data.TTSSizeExceededTruncate = value;
                NotifyPropertyChanged();
            }
        }

        // hp bar
        public bool BossGageVisible
        {
            get => Data.BossGageStatus.Visible;
            set
            {
                var dataBossGageStatus = Data.BossGageStatus;
                if (dataBossGageStatus.Visible == value) return;
                dataBossGageStatus.Visible = value;
                Data.BossGageStatus = dataBossGageStatus; // todo: check this
                if (dataBossGageStatus.Visible)
                    App.HudContainer.BossGage.ShowWindow();
                else
                    App.HudContainer.BossGage.HideWindow();
                //todo: manually show/hide the window
                NotifyPropertyChanged();
            }
        }
        public bool BossGageAbnormalsDisabled
        {
            get => Data.NoAbnormalsInHUD;
            set
            {
                if (Data.NoAbnormalsInHUD == value) return;
                Data.NoAbnormalsInHUD = value;
                NotifyPropertyChanged();
                if (!value) return;
                foreach (var boss in DamageMeter.HudManager.Instance.CurrentBosses.ToSyncArray())
                {
                    boss.Buffs.DisposeAll();
                }

            }
        }

        // graph
        public bool RealtimeGraphEnabled
        {
            get => Data.RealtimeGraphEnabled;
            set
            {
                if (Data.RealtimeGraphEnabled == value) return;
                Data.RealtimeGraphEnabled = value;
                NotifyPropertyChanged();
            }
        }
        public bool RealtimeGraphAnimated
        {
            get => Data.RealtimeGraphAnimated;
            set
            {
                if (Data.RealtimeGraphAnimated == value) return;
                Data.RealtimeGraphAnimated = value;
                NotifyPropertyChanged();
            }
        }
        public int RealtimeGraphDisplayedInterval
        {
            get => Data.RealtimeGraphDisplayedInterval;
            set
            {
                if (Data.RealtimeGraphDisplayedInterval == value) return;
                Data.RealtimeGraphDisplayedInterval = value;
                NotifyPropertyChanged();
            }
        }
        public int RealtimeGraphCMAseconds
        {
            get => Data.RealtimeGraphCMAseconds;
            set
            {
                if (Data.RealtimeGraphCMAseconds == value) return;
                Data.RealtimeGraphCMAseconds = value;
                NotifyPropertyChanged();
            }
        }
        public GraphMode GraphMode
        {
            get => Data.GraphMode;
            set
            {
                if (Data.GraphMode == value) return;
                Data.GraphMode = value;
                NotifyPropertyChanged();
            }
        }


        // input
        public bool RemoveAltEnter
        {
            get => Data.RemoveTeraAltEnterHotkey;
            set
            {
                if (Data.RemoveTeraAltEnterHotkey == value) return;
                Data.RemoveTeraAltEnterHotkey = value;
                NotifyPropertyChanged();
                KeyboardHook.Instance.Update();
            }
        }
        public bool FormatPasteString
        {
            get => Data.FormatPasteString;
            set
            {
                if (Data.FormatPasteString == value) return;
                Data.FormatPasteString = value;
                NotifyPropertyChanged();
            }
        }
        public bool DisableIngameChatPaste
        {
            get => Data.NoPaste;
            set
            {
                if (Data.NoPaste == value) return;
                Data.NoPaste = value;
                NotifyPropertyChanged();
            }
        }
        public int PasteDelay
        {
            get => Data.LFDelay;
            set
            {
                if (Data.LFDelay == value) return;
                Data.LFDelay = value;
                NotifyPropertyChanged();
            }
        }


        // dps servers
        public ObservableCollection<DpsServerViewModel> DpsServers { get; } = new ObservableCollection<DpsServerViewModel>();


        // misc
        public bool ExportPacketLogs
        {
            get => Data.PacketsCollect;
            set
            {
                if (Data.PacketsCollect == value) return;
                Data.PacketsCollect = value;
                NotifyPropertyChanged();
                /*TeraSniffer.Instance*/
                PacketProcessor.Instance.Sniffer.EnableMessageStorage = value;

            }
        }
        public bool ExcelAutoExport
        {
            get => Data.Excel;
            set
            {
                if (Data.Excel == value) return;
                Data.Excel = value;
                NotifyPropertyChanged();
            }
        }
        public bool JsonAutoExport
        {
            get => Data.Json;
            set
            {
                if (Data.Json == value) return;
                Data.Json = value;
                NotifyPropertyChanged();
            }
        }
        public int ExcelCmaSeconds
        {
            get => Data.ExcelCMADPSSeconds;
            set
            {
                if (Data.ExcelCMADPSSeconds == value) return;
                Data.ExcelCMADPSSeconds = value;
                NotifyPropertyChanged();
            }
        }

        // event
        public bool EnableChatAndNotif
        {
            get => Data.EnableChat;
            set
            {
                if (Data.EnableChat == value) return;
                Data.EnableChat = value;
                NotifyPropertyChanged();
                RichPresence.Instance.Update();

            }
        }
        public bool AutoDisableChatWhenOverloaded
        {
            get => Data.AutoDisableChatWhenOverloaded;
            set
            {
                if (Data.AutoDisableChatWhenOverloaded == value) return;
                Data.AutoDisableChatWhenOverloaded = value;
                NotifyPropertyChanged();

            }
        }
        public bool MuteSound
        {
            get => Data.MuteSound;
            set
            {
                if (Data.MuteSound == value) return;
                Data.MuteSound = value;
                NotifyPropertyChanged();
            }
        }
        public bool CopyInspectOnApply
        {
            get => Data.CopyInspect;
            set
            {
                if (Data.CopyInspect == value) return;
                Data.CopyInspect = value;
                NotifyPropertyChanged();
            }
        }
        public bool DisablePartyEvents
        {
            get => Data.DisablePartyEvent;
            set
            {
                if (Data.DisablePartyEvent == value) return;
                Data.DisablePartyEvent = value;
                NotifyPropertyChanged();
            }
        }
        public bool ShowAfkEventsIngame
        {
            get => Data.ShowAfkEventsIngame;
            set
            {
                if (Data.ShowAfkEventsIngame == value) return;
                Data.ShowAfkEventsIngame = value;
                NotifyPropertyChanged();
            }
        }

        // rich presence
        public bool RichPresenceEnable
        {
            get => Data.EnableRichPresence;
            set
            {
                if (Data.EnableRichPresence == value) return;
                Data.EnableRichPresence = value;
                NotifyPropertyChanged();
                if (value)
                {
                    RichPresence.Instance.Update();
                }
                else
                {
                    RichPresence.Instance.Deinitialize();
                }
            }
        }
        public bool RichPresenceShowLocation
        {
            get => Data.RichPresenceShowLocation;
            set
            {
                if (Data.RichPresenceShowLocation == value) return;
                Data.RichPresenceShowLocation = value;
                NotifyPropertyChanged();
                RichPresence.Instance.Update();

            }
        }
        public bool RichPresenceShowCharacter
        {
            get => Data.RichPresenceShowCharacter;
            set
            {
                if (Data.RichPresenceShowCharacter == value) return;
                Data.RichPresenceShowCharacter = value;
                NotifyPropertyChanged();
                RichPresence.Instance.Update();

            }
        }
        public bool RichPresenceShowStatus
        {
            get => Data.RichPresenceShowStatus;
            set
            {
                if (Data.RichPresenceShowStatus == value) return;
                Data.RichPresenceShowStatus = value;
                NotifyPropertyChanged();
                RichPresence.Instance.Update();

            }
        }
        public bool RichPresenceShowParty
        {
            get => Data.RichPresenceShowParty;
            set
            {
                if (Data.RichPresenceShowParty == value) return;
                Data.RichPresenceShowParty = value;
                NotifyPropertyChanged();
                RichPresence.Instance.Update();
            }
        }

        // chat colors
        public Color ChatWhisper
        {
            get => Data.WhisperColor;
            set
            {
                if (Data.WhisperColor == value) return;
                Data.WhisperColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatAlliance
        {
            get => Data.AllianceColor;
            set
            {
                if (Data.AllianceColor == value) return;
                Data.AllianceColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatArea
        {
            get => Data.AreaColor;
            set
            {
                if (Data.AreaColor == value) return;
                Data.AreaColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatGeneral
        {
            get => Data.GeneralColor;
            set
            {
                if (Data.GeneralColor == value) return;
                Data.GeneralColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatGroup
        {
            get => Data.GroupColor;
            set
            {
                if (Data.GroupColor == value) return;
                Data.GroupColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatGuild
        {
            get => Data.GuildColor;
            set
            {
                if (Data.GuildColor == value) return;
                Data.GuildColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatRaid
        {
            get => Data.RaidColor;
            set
            {
                if (Data.RaidColor == value) return;
                Data.RaidColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatSay
        {
            get => Data.SayColor;
            set
            {
                if (Data.SayColor == value) return;
                Data.SayColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatTrade
        {
            get => Data.TradingColor;
            set
            {
                if (Data.TradingColor == value) return;
                Data.TradingColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatEmotes
        {
            get => Data.EmotesColor;
            set
            {
                if (Data.EmotesColor == value) return;
                Data.EmotesColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color ChatPrivate
        {
            get => Data.PrivateChannelColor;
            set
            {
                if (Data.PrivateChannelColor == value) return;
                Data.PrivateChannelColor = value;
                NotifyPropertyChanged();
            }
        }

        public static event Action<DpsServerViewModel> ServerRemoved;
        public static event Action<double> WindowScaleChanged;
        public static event Action<int> NumberOfPlayersDisplayedChanged;
        public static event Action WindowColorsChanged;
        public static event Action<double> OtherWindowsOpacityChanged;
        public static event Action PauseChanged;

        public ICommand AddServerCommand { get; }
        public ICommand SetViewCommand { get; }
        public ICommand UploadGlyphBuildCommand { get; }
        public ICommand OpenChatBoxCommand { get; }
        public ICommand BrowseLinkCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand CloseMeterCommand => MainViewModel.Instance.VerifyCloseCommand;
        public ICommand ExportCurrentCommand { get; }
        public ICommand TogglePauseCommand => MainViewModel.Instance.TogglePauseCommand;
        public ICommand OpenUploadHistoryCommand { get; }
        public ICommand ResetColorCommand { get; }
        public ICommand OpenEventEditorCommand { get; }
        public List<MockPlayerViewModel> MockParty { get; }

        private int _selectedIndex;
        private long _lastSend;
        private bool _isShiftDown;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                NotifyPropertyChanged();
            }
        }
        public bool Paused => Data.UserPaused;

        public ToastViewModel ToastData { get; }


        public SettingsWindowViewModel()
        {
            ToastData = new ToastViewModel();

            DataExporter.GlpyhExportStatusUpdated += OnGlyphExportStatusUpdated;

            DataExporter.DpsServers.Where(x => !x.AnonymousUpload).ToList().ForEach(x => DpsServers.Add(new DpsServerViewModel(x)));
            ServerRemoved += OnServerRemoved;
            PauseChanged += OnPauseChanged;

            AddServerCommand = new RelayCommand(_ =>
            {
                var server = new TeraDpsApi.DpsServer(new DpsServerData(null, null, null, null, null, true), false);
                BasicTeraData.Instance.WindowData.DpsServers.Add(server.Data);
                DataExporter.DpsServers.Add(server);
                DpsServers.Add(new DpsServerViewModel(server));
            });
            SetViewCommand = new RelayCommand(arg =>
            {
                SelectedIndex = int.Parse(arg?.ToString() ?? "0");
            });
            UploadGlyphBuildCommand = new RelayCommand(_ =>
            {
                DataExporter.ExportGlyph();
            });
            OpenChatBoxCommand = new RelayCommand(_ =>
            {
                new Chatbox().ShowWindow(); // force only one?
            });
            BrowseLinkCommand = new RelayCommand(type =>
            {
                var url = "https://github.com/neowutran/ShinraMeter";
                switch (type.ToString())
                {
                    case "wiki": url += "/wiki"; break;
                    case "patch": url += "/wiki/Patch-note"; break;
                    case "issues": url += "/issues"; break;
                    case "discord": url = "https://discord.gg/anUXQTp"; break;
                }

                Process.Start("explorer.exe", url);
            });
            ResetCommand = new RelayCommand(_ =>
            {
                PacketProcessor.Instance.NeedToReset = true;
            });

            ExportCurrentCommand = new RelayCommand(target =>
            {
                switch (target.ToString())
                {
                    case "site":
                        if (_lastSend + TimeSpan.TicksPerSecond * 60 >= DateTime.Now.Ticks) return;
                        PacketProcessor.Instance.NeedToExport = DataExporter.Dest.Site;
                        _lastSend = DateTime.Now.Ticks;
                        break;
                    case "excel":
                        PacketProcessor.Instance.NeedToExport = DataExporter.Dest.Excel | DataExporter.Dest.Manual;
                        break;
                    case "json":
                        PacketProcessor.Instance.NeedToExport = DataExporter.Dest.Json | DataExporter.Dest.Manual;
                        break;
                }
            });
            OpenUploadHistoryCommand = new RelayCommand(_ => App.HudContainer.UploadHistory.ShowWindow());
            ResetColorCommand = new RelayCommand(type =>
            {
                switch (type?.ToString())
                {
                    case "self": SelfColor = WindowData.DefaultPlayerColor; break;
                    case "dps": DpsColor = WindowData.DefaultDpsColor; break;
                    case "healer": HealerColor = WindowData.DefaultHealerColor; break;
                    case "tank": TankColor = WindowData.DefaultTankColor; break;
                    case "background": BackgroundColor = WindowData.DefaultBackgroundColor; break;
                    case "border": BorderColor = WindowData.DefaultBorderColor; break;
                }
            });
            OpenEventEditorCommand = new RelayCommand(_ => new EventsEditorWindow().ShowDialog());

            var count = 0;
            Hotkeys.Copy.ForEach(h => CopyKeys.Add(new CopyKeyVM($"DPS paste {++count}", h)));
            MockParty = new List<MockPlayerViewModel>
            {
                new MockPlayerViewModel("Deep.Yes", PlayerClass.Warrior),
                new MockPlayerViewModel("No.Broler", PlayerClass.Lancer),
                new MockPlayerViewModel("Priest.Kek", PlayerClass.Mystic)
            };
        }


        private void OnGlyphExportStatusUpdated(DataExporter.GlyphExportStatus status, string txt)
        {
            var lvl = status switch
            {
                DataExporter.GlyphExportStatus.Unknown => ToastInfo.Severity.Error,
                DataExporter.GlyphExportStatus.TooFast => ToastInfo.Severity.Error,
                DataExporter.GlyphExportStatus.NotLoggedIn => ToastInfo.Severity.Error,
                DataExporter.GlyphExportStatus.LevelTooLow => ToastInfo.Severity.Error,
                DataExporter.GlyphExportStatus.InvalidUrl => ToastInfo.Severity.Error,
                DataExporter.GlyphExportStatus.Rejected => ToastInfo.Severity.Error,
                DataExporter.GlyphExportStatus.Success => ToastInfo.Severity.Success,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
            ToastData.Show(txt, lvl);

        }

        private void OnPauseChanged()
        {
            NotifyPropertyChanged(nameof(Paused));
        }

        private void OnServerRemoved(DpsServerViewModel dpsServerViewModel)
        {
            DpsServers.Remove(dpsServerViewModel);
        }

        public static void NotifyServerRemoved(DpsServerViewModel dpsServerViewModel)
        {
            ServerRemoved?.Invoke(dpsServerViewModel);
        }

        private void UpdateTopMost()
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.Topmost = BasicTeraData.Instance.WindowData.Topmost;
                window.ShowInTaskbar = !BasicTeraData.Instance.WindowData.Topmost;
            }
        }
        public static void NotifyPausedChanged()
        {
            PauseChanged?.Invoke();
        }


        public HotKey TopmostHotkey
        {
            get => Hotkeys.Topmost;
            set
            {
                if (Hotkeys.Topmost.Equals(value) || BasicTeraData.Instance.HotkeysData.IsAssigned(value)) return;
                Hotkeys.Topmost = value;
                NotifyPropertyChanged();

            }
        }

        public HotKey PasteHotkey
        {
            get => Hotkeys.Paste;
            set
            {
                if (Hotkeys.Paste.Equals(value) || BasicTeraData.Instance.HotkeysData.IsAssigned(value)) return;
                Hotkeys.Paste = value;
                NotifyPropertyChanged();
            }
        }
        public HotKey ClickThrouHotkey
        {
            get => Hotkeys.ClickThrou;
            set
            {
                if (Hotkeys.ClickThrou.Equals(value) || BasicTeraData.Instance.HotkeysData.IsAssigned(value)) return;
                Hotkeys.ClickThrou = value;
                NotifyPropertyChanged();
            }
        }
        public HotKey ExcelSaveHotkey
        {
            get => Hotkeys.ExcelSave;
            set
            {
                if (Hotkeys.ExcelSave.Equals(value) || BasicTeraData.Instance.HotkeysData.IsAssigned(value)) return;
                Hotkeys.ExcelSave = value;
                NotifyPropertyChanged();
            }
        }

        public HotKey ResetHotkey
        {
            get => Hotkeys.Reset;
            set
            {
                if (Hotkeys.Reset.Equals(value) || BasicTeraData.Instance.HotkeysData.IsAssigned(value)) return;
                Hotkeys.Reset = value;
                NotifyPropertyChanged();
            }
        }
        public HotKey ResetCurrentHotkey
        {
            get => Hotkeys.ResetCurrent;
            set
            {
                if (Hotkeys.ResetCurrent.Equals(value) || BasicTeraData.Instance.HotkeysData.IsAssigned(value)) return;
                Hotkeys.ResetCurrent = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<CopyKeyVM> CopyKeys { get; } = new ObservableCollection<CopyKeyVM>();

        public bool IsShiftDown
        {
            get => _isShiftDown;
            set
            {
                if (_isShiftDown == value) return;
                _isShiftDown = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class CopyKeyVM : TSPropertyChanged
    {
        private readonly CopyKey _ck;

        public HotKey HotKey
        {
            get => _ck.Hotkey;
            set
            {
                if (_ck.Hotkey.Equals(value) || BasicTeraData.Instance.HotkeysData.IsAssigned(value)) return;
                _ck.Hotkey = value;
                NotifyPropertyChanged();
            }
        }

        public string Name { get; }
        public string Preview => _ck.Content;

        public CopyKeyVM(string name, CopyKey ck)
        {
            _ck = ck;
            Name = name;
        }
    }
}