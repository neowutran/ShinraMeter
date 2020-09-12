using System;
using System.Windows.Input;
using DamageMeter.Database.Structures;
using Data;
using Lang;
using Nostrum;
using Tera.Game;
using Tera.Game.Abnormality;

namespace DamageMeter.UI
{
    public interface IPlayerViewModel
    {
        string Name { get; }
        string Level { get; }
        PlayerClass Class { get; }
        double DamageFactor { get; }
        PlayerRole Role { get; }

        bool Increased { get; set; }
        bool Decreased { get; set; }
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

        Metric Metric1 { get; set; }
        Metric Metric2 { get; set; }
        Metric Metric3 { get; set; }
        Metric Metric4 { get; set; }
        Metric Metric5 { get; set; }
    }
    public class PlayerDamageViewModel : TSPropertyChanged, IPlayerViewModel
    {

        /// <summary>
        /// To fire animation in view
        /// </summary>
        public event Action<double> DamageFactorChanged;

        private PlayerDamageDealt _damageData;
        private PlayerHealDealt _healData;
        private PlayerAbnormals _buffs;
        private AbnormalityStorage _allBuffs;
        private Database.Structures.Skills _skills;
        private EntityInformation _entityInfo;
        private Skills _windowSkill;
        private bool _showFirst;

        public string Name => _damageData.Source.Name;
        public string FullName => _damageData.Source.FullName;
        public PlayerClass Class => _damageData.Source.Class;
        public string Level => Class == PlayerClass.Common || _damageData.Source.Level == 70 ? "" : _damageData.Source.Level.ToString();
        public long Amount => _damageData.Amount;
        public double DamageFactor => _entityInfo.TotalDamage == 0 ? 0 : _damageData.Amount / (double)_entityInfo.TotalDamage;
        public bool ShowFirst
        {
            get => _showFirst;
            set
            {
                if (_showFirst == value) return;
                _showFirst = value;
                NotifyPropertyChanged();
            }
        }
        public string FightDuration => $"{LP.Fight_Duration}: {(TimeSpan.FromSeconds(_damageData.Interval / TimeSpan.TicksPerSecond)):mm\\:ss}";
        // for color
        public PlayerRole Role => _damageData.Source.User.Id.Id == PacketProcessor.Instance.PlayerTracker.Me().User.Id.Id
            ? PlayerRole.Self
            : MiscUtils.RoleFromClass(Class);


        public ICommand ShowSkillDetailsCommand { get; }
        public ICommand CloseSkillDetailsCommand { get; }
        public ICommand SwitchCritModeCommand { get; }

        private string _critRate;
        private string _healCritRate;
        private string _damagePerc;
        private string _damageDone;
        private bool _increased;
        private bool _decreased;
        private string _globalDps;
        private string _individualDps;
        private string _damageFromCrits;
        private string _damageTaken;
        private string _dpsTaken;
        private int _deaths;
        private string _floorTime;
        private string _aggroPerc;
        private string _floorTimePerc;
        private double _enduDebuffUptime;
        private string _enduDebuffUptimeStr;
        private Metric _metric1 = Metric.Deaths;
        private Metric _metric2 = Metric.Floortime;
        private Metric _metric3 = Metric.FloortimePerc;
        private Metric _metric4 = Metric.AggroPerc;
        private Metric _metric5 = Metric.EnduDebuffUptime;


        public bool Increased
        {
            get => _increased;
            set
            {
                if (_increased == value) return;
                _increased = value;
                NotifyPropertyChanged();
            }
        }
        public bool Decreased
        {
            get => _decreased;
            set
            {
                if (_decreased == value) return;
                _decreased = value;
                NotifyPropertyChanged();
            }
        }
        public string GlobalDps
        {
            get => _globalDps;
            set
            {
                if (_globalDps == value) return;
                _globalDps = value;
                NotifyPropertyChanged();
            }
        }
        public string IndividualDps
        {
            get => _individualDps;
            set
            {
                if (_individualDps == value) return;
                _individualDps = value;
                NotifyPropertyChanged();
            }
        }
        public string DamagePerc
        {
            get => _damagePerc;
            set
            {
                if (_damagePerc == value) return;
                _damagePerc = value;
                NotifyPropertyChanged();
            }
        }
        public string DamageDone
        {
            get => _damageDone;
            set
            {
                if (_damageDone == value) return;
                _damageDone = value;
                NotifyPropertyChanged();
            }
        }
        public string CritRate
        {
            get => _critRate;
            set
            {
                if (_critRate == value) return;
                _critRate = value;
                NotifyPropertyChanged();
            }
        }
        public string HealCritRate
        {
            get => _healCritRate;
            set
            {
                if (_healCritRate == value) return;
                _healCritRate = value;
                NotifyPropertyChanged();
            }
        }
        public string DamageFromCrits
        {
            get => _damageFromCrits;
            set
            {
                if (_damageFromCrits == value) return;
                _damageFromCrits = value;
                NotifyPropertyChanged();
            }
        }
        public string DamageTaken
        {
            get => _damageTaken;
            set
            {
                if (_damageTaken == value) return;
                _damageTaken = value;
                NotifyPropertyChanged();
            }
        }
        public string DpsTaken
        {
            get => _dpsTaken;
            set
            {
                if (_dpsTaken == value) return;
                _dpsTaken = value;
                NotifyPropertyChanged();
            }
        }
        public int Deaths
        {
            get => _deaths;
            set
            {
                if (_deaths == value) return;
                _deaths = value;
                NotifyPropertyChanged();
            }
        }
        public string FloorTime
        {
            get => _floorTime;
            set
            {
                if (_floorTime == value) return;
                _floorTime = value;
                NotifyPropertyChanged();
            }
        }
        public string FloorTimePerc
        {
            get => _floorTimePerc;
            set
            {
                if (_floorTimePerc == value) return;
                _floorTimePerc = value;
                NotifyPropertyChanged();
            }
        }
        public string AggroPerc
        {
            get => _aggroPerc;
            set
            {
                if (_aggroPerc == value) return;
                _aggroPerc = value;
                NotifyPropertyChanged();
            }
        }
        public double EnduDebuffUptime
        {
            get => _enduDebuffUptime;
            set
            {
                if (_enduDebuffUptime == value) return;
                _enduDebuffUptime = value;
                NotifyPropertyChanged();
            }
        }
        public string EnduDebuffUptimeStr
        {
            get => _enduDebuffUptimeStr;
            set
            {
                if (_enduDebuffUptimeStr == value) return;
                _enduDebuffUptimeStr = value;
                NotifyPropertyChanged();
            }
        }

        public Metric Metric1
        {
            get => _metric1;
            set
            {
                if (_metric1 == value) return;
                _metric1 = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric2
        {
            get => _metric2;
            set
            {
                if (_metric2 == value) return;
                _metric2 = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric3
        {
            get => _metric3;
            set
            {
                if (_metric3 == value) return;
                _metric3 = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric4
        {
            get => _metric4;
            set
            {
                if (_metric4 == value) return;
                _metric4 = value;
                NotifyPropertyChanged();
            }
        }
        public Metric Metric5
        {
            get => _metric5;
            set
            {
                if (_metric5 == value) return;
                _metric5 = value;
                NotifyPropertyChanged();
            }
        }


        public PlayerDamageViewModel(PlayerDamageDealt pdd, PlayerHealDealt phd, EntityInformation ei, Database.Structures.Skills sk, AbnormalityStorage b, bool timedEncounter)
        {
            ShowSkillDetailsCommand = new RelayCommand(_ => ShowSkills());
            CloseSkillDetailsCommand = new RelayCommand(_ => CloseSkills());
            SwitchCritModeCommand = new RelayCommand(_ => SwitchCritMode());

            // todo: set stats type from settings

            _damageData = pdd;
            _healData = phd;
            _entityInfo = ei;
            _skills = sk;
            _buffs = b.Get(_damageData.Source);
            _allBuffs = b;

            Update(pdd, phd, ei, sk, b, timedEncounter);
        }
        public void Update(PlayerDamageDealt pdd, PlayerHealDealt phd, EntityInformation ei, Database.Structures.Skills sk, AbnormalityStorage b, bool timedEncounter)
        {
            var metricSource = MiscUtils.RoleFromClass(Class) switch
            {
                PlayerRole.Tank => BasicTeraData.Instance.WindowData.TankMetrics,
                PlayerRole.Healer => BasicTeraData.Instance.WindowData.HealerMetrics,
                _ => BasicTeraData.Instance.WindowData.DpsMetrics
            };

            Metric1 = metricSource[0];
            Metric2 = metricSource[1];
            Metric3 = metricSource[2];
            Metric4 = metricSource[3];
            Metric5 = metricSource[4];

            var oldDps = _entityInfo?.Interval == 0 ? _damageData?.Amount : _damageData?.Amount * TimeSpan.TicksPerSecond / _entityInfo?.Interval;
            _damageData = pdd;
            _healData = phd;
            _entityInfo = ei;
            _skills = sk;
            _buffs = b.Get(_damageData.Source);
            _allBuffs = b;

            var newDps = _entityInfo.Interval == 0 ? _damageData.Amount : _damageData.Amount * TimeSpan.TicksPerSecond / _entityInfo.Interval;
            if (newDps > oldDps)
            {
                Increased = true;
                Decreased = false;
            }
            else if (newDps < oldDps)
            {
                Increased = false;
                Decreased = true;
            }
            else
            {
                Increased = false;
                Decreased = false;
            }

            // --
            DamageDone = FormatHelpers.Instance.FormatValue(_damageData?.Amount);
            DamagePerc = Math.Round(_damageData.Amount * 100D / _entityInfo.TotalDamage) + "%";
            GlobalDps = FormatHelpers.Instance.FormatValue(_entityInfo.Interval == 0
                ? _damageData.Amount
                : _damageData.Amount * TimeSpan.TicksPerSecond / _entityInfo.Interval) + LP.PerSecond;
            IndividualDps = FormatHelpers.Instance.FormatValue(_damageData.Interval == 0
                ? _damageData.Amount
                : _damageData.Amount * TimeSpan.TicksPerSecond / _damageData.Interval) + LP.PerSecond;

            // --
            CritRate = Math.Round(_damageData.CritRate) + "%";
            HealCritRate = Math.Round(_healData?.CritRate ?? 0) + "%";
            DamageFromCrits = Math.Round(_damageData.CritDamageRate) + "%";

            // --
            DamageTaken = FormatHelpers.Instance.FormatValue(_skills?.DamageReceived(_damageData.Source.User, _entityInfo.Entity, timedEncounter) ?? 0);
            DpsTaken = FormatHelpers.Instance.FormatValue(_entityInfo.Interval == 0
                ? (_skills?.DamageReceived(_damageData.Source.User, _entityInfo.Entity, timedEncounter) ?? 0L)
                : (_skills?.DamageReceived(_damageData.Source.User, _entityInfo.Entity, timedEncounter) ?? 0L) * TimeSpan.TicksPerSecond / _entityInfo.Interval) + LP.PerSecond;

            // --
            var death = _buffs.Death;
            Deaths = death?.Count(_entityInfo.BeginTime, _entityInfo.EndTime) ?? 0;
            var deathTicks = death?.Duration(_entityInfo.BeginTime, _entityInfo.EndTime) ?? 0L;
            var deathDuration = death != null ? TimeSpan.FromTicks(deathTicks) : TimeSpan.Zero;
            FloorTime = deathDuration.ToString(@"mm\:ss");
            FloorTimePerc = Math.Round(_entityInfo.Interval == 0
                ? deathTicks
                : (double)deathTicks * 100 / (double)_entityInfo.Interval) + "%";

            // --
            var aggro = _buffs.Aggro(_entityInfo.Entity);
            var aggroTicks = aggro?.Duration(_entityInfo.BeginTime, _entityInfo.EndTime) ?? 0L;
            AggroPerc = Math.Round(_entityInfo.Interval == 0
               ? aggroTicks
               : (double)aggroTicks * 100 / (double)_entityInfo.Interval) + "%";


            // --
            // TN: 28090 - VOC: 27160
            switch (Class)
            {
                case PlayerClass.Priest:
                {
                    _allBuffs.Get(_entityInfo.Entity).TryGetValue(BasicTeraData.Instance.HotDotDatabase.Get(28090), out var tn);
                    EnduDebuffUptime = _entityInfo.Interval == 0
                        ? ((tn?.Duration(_entityInfo.BeginTime, _entityInfo.EndTime) ?? 0L))
                        : ((tn?.Duration(_entityInfo.BeginTime, _entityInfo.EndTime) ?? 0L) / (double)_entityInfo.Interval);
                    EnduDebuffUptimeStr = Math.Round(EnduDebuffUptime * 100) + "%";
                    break;
                }
                case PlayerClass.Mystic:
                {
                    _allBuffs.Get(_entityInfo.Entity).TryGetValue(BasicTeraData.Instance.HotDotDatabase.Get(27160), out var voc);
                    EnduDebuffUptime = _entityInfo.Interval == 0
                        ? ((voc?.Duration(_entityInfo.BeginTime, _entityInfo.EndTime) ?? 0L))
                        : ((voc?.Duration(_entityInfo.BeginTime, _entityInfo.EndTime) ?? 0L) / (double)_entityInfo.Interval);
                    EnduDebuffUptimeStr = Math.Round(EnduDebuffUptime * 100) + "%";
                    break;
                }
                default:
                    EnduDebuffUptime = 0;
                    EnduDebuffUptimeStr = Math.Round(EnduDebuffUptime * 100) + "%";
                    break;
            }



            DamageFactorChanged?.Invoke(DamageFactor);

            if (Role == PlayerRole.Self)
            {
                ShowFirst = BasicTeraData.Instance.WindowData.MeterUserOnTop;
            }

            NotifyPropertyChanged(nameof(Amount));
            NotifyPropertyChanged(nameof(FightDuration));

            _windowSkill?.Update(_damageData, _entityInfo, _skills, _buffs, timedEncounter);
#if false
            //todo

            GridStats.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var SGrid = ((MainWindow)((FrameworkElement)((FrameworkElement)((FrameworkElement)Parent).Parent).Parent).Parent).SGrid;
            var EGrid = ((MainWindow)((FrameworkElement)((FrameworkElement)((FrameworkElement)Parent).Parent).Parent).Parent).EGrid;
            SGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var mainWidth = SGrid.DesiredSize.Width;
            Spacer.Width = mainWidth > GridStats.DesiredSize.Width ? mainWidth - GridStats.DesiredSize.Width : 0;
            EGrid.MaxWidth = Math.Max(mainWidth, GridStats.DesiredSize.Width);
#endif

        }

        private void SwitchCritMode()
        {
            if (Class == PlayerClass.Priest || Class == PlayerClass.Mystic)
            {
                BasicTeraData.Instance.WindowData.ShowHealCrit = !BasicTeraData.Instance.WindowData.ShowHealCrit;
            }
            else
            {
                BasicTeraData.Instance.WindowData.ShowCritDamageRate = !BasicTeraData.Instance.WindowData.ShowCritDamageRate;
            }
        }

        //todo: add clickthru to new window

        private void ShowSkills()
        {
            _windowSkill ??= new Skills(this);
            PacketProcessor.Instance.SendFullDetails = true;
            _windowSkill.ShowWindow();
        }

        private void CloseSkills()
        {
            _windowSkill?.Close();
            _windowSkill = null;
        }
    }
}