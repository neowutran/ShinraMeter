using System;
using System.Windows.Input;
using DamageMeter.Database.Structures;
using Data;
using Lang;
using Nostrum;
using Tera.Game;
using Tera.Game.Abnormality;
using Brushes = System.Drawing.Brushes;

namespace DamageMeter.UI
{
    public class PlayerDamageViewModel : TSPropertyChanged
    {
        public enum PlayerRole
        {
            Dps,
            Tank,
            Healer,
            Self,
            None
        }

        public enum CritDisplayMode
        {
            CritRate,
            HealCritRate,
            DamageFromCrits
        }

        /// <summary>
        /// To fire animation in view
        /// </summary>
        public event Action<float> DamageFactorChanged;

        private PlayerDamageDealt _damageData;
        private PlayerHealDealt _healData;
        private PlayerAbnormals _buffs;
        private Database.Structures.Skills _skills;
        private EntityInformation _entityInfo;
        private Skills _windowSkill;

        public string Name => _damageData.Source.Name;
        public string FullName => _damageData.Source.FullName;
        public PlayerClass Class => _damageData.Source.Class;
        public string Level => Class == PlayerClass.Common || _damageData.Source.Level == 70 ? "" : _damageData.Source.Level.ToString();
        public string GlobalDps => FormatHelpers.Instance.FormatValue(_entityInfo.Interval == 0
            ? _damageData.Amount
            : _damageData.Amount * TimeSpan.TicksPerSecond / _entityInfo.Interval) + LP.PerSecond;
        public string IndividualDps => LP.Individual_dps + ": " + FormatHelpers.Instance.FormatValue(_damageData.Interval == 0
            ? _damageData.Amount
            : _damageData.Amount * TimeSpan.TicksPerSecond / _damageData.Interval) + LP.PerSecond;
        public string DamagePerc => Math.Round((double)_damageData.Amount * 100 / _entityInfo.TotalDamage) + "%";
        public string DamageDone => LP.Damage_done + ": " + FormatHelpers.Instance.FormatValue(_damageData.Amount);
        public long Amount => _damageData.Amount;
        public float DamageFactor => _entityInfo.TotalDamage == 0 ? 0 : _damageData.Amount / (float) _entityInfo.TotalDamage;

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
        public CritDisplayMode CritMode =>
            _damageData.Source.IsHealer && BasicTeraData.Instance.WindowData.ShowHealCrit
                ? CritDisplayMode.HealCritRate
                : BasicTeraData.Instance.WindowData.ShowCritDamageRate
                    ? CritDisplayMode.DamageFromCrits
                    : CritDisplayMode.CritRate;

        public string CritRate => CritMode switch
        {
            CritDisplayMode.CritRate => Math.Round(_damageData.CritRate) + "%",
            CritDisplayMode.HealCritRate => Math.Round(_healData?.CritRate ?? 0) + "%",
            CritDisplayMode.DamageFromCrits => Math.Round(_damageData.CritDamageRate) + "%",
            _ => ""
        };

        public string FightDuration => $"{LP.Fight_Duration}: {(TimeSpan.FromSeconds(_damageData.Interval / TimeSpan.TicksPerSecond)):mm\\:ss}";

        // for color
        public PlayerRole Role
        {
            get
            {
                if (_damageData.Source.User.Id.Id == PacketProcessor.Instance.PlayerTracker.Me().User.Id.Id) return PlayerRole.Self;
                return Class switch
                {
                    PlayerClass.Warrior => PlayerRole.Dps,
                    PlayerClass.Slayer => PlayerRole.Dps,
                    PlayerClass.Berserker => PlayerRole.Dps,
                    PlayerClass.Sorcerer => PlayerRole.Dps,
                    PlayerClass.Archer => PlayerRole.Dps,
                    PlayerClass.Reaper => PlayerRole.Dps,
                    PlayerClass.Gunner => PlayerRole.Dps,
                    PlayerClass.Ninja => PlayerRole.Dps,
                    PlayerClass.Valkyrie => PlayerRole.Dps,
                    PlayerClass.Priest => PlayerRole.Healer,
                    PlayerClass.Mystic => PlayerRole.Healer,
                    PlayerClass.Brawler => PlayerRole.Tank,
                    PlayerClass.Lancer => PlayerRole.Tank,
                    _ => PlayerRole.None
                };
            }
        }

        private bool _increasing;
        private bool _decreasing;
        private bool _showFirst;

        public bool Increasing
        {
            get => _increasing;
            set
            {
                if (_increasing == value) return;
                _increasing = value;
                NotifyPropertyChanged();
            }
        }


        public bool Decreasing
        {
            get => _decreasing;
            set
            {
                if (_decreasing == value) return;
                _decreasing = value;
                NotifyPropertyChanged();
            }
        }


        public ICommand ShowSkillDetailsCommand { get; }
        public ICommand CloseSkillDetailsCommand { get; }
        public ICommand SwitchCritModeCommand { get; }

        public PlayerDamageViewModel(PlayerDamageDealt pdd, PlayerHealDealt phd, EntityInformation ei, Database.Structures.Skills sk, PlayerAbnormals b)
        {
            ShowSkillDetailsCommand = new RelayCommand(_ => ShowSkills());
            CloseSkillDetailsCommand = new RelayCommand(_ => CloseSkills());
            SwitchCritModeCommand = new RelayCommand(_ => SwitchCritMode());

            _damageData = pdd;
            _healData = phd;
            _entityInfo = ei;
            _skills = sk;
            _buffs = b;
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
            //todo
            _windowSkill ??= new Skills(this);
            PacketProcessor.Instance.SendFullDetails = true;
            _windowSkill.ShowWindow();
        }

        private void CloseSkills()
        {
            _windowSkill?.Close();
            _windowSkill = null;
        }
        public void Update(PlayerDamageDealt pdd, PlayerHealDealt phd, EntityInformation ei, Database.Structures.Skills sk, PlayerAbnormals b, bool timedEncounter)
        {
            var oldDps = _entityInfo.Interval == 0 ? _damageData.Amount : _damageData.Amount * TimeSpan.TicksPerSecond / _entityInfo.Interval;
            _damageData = pdd;
            _healData = phd;
            _entityInfo = ei;
            _skills = sk;
            _buffs = b;

            var newDps = _entityInfo.Interval == 0 ? _damageData.Amount : _damageData.Amount * TimeSpan.TicksPerSecond / _entityInfo.Interval;

            if (newDps > oldDps)
            {
                Increasing = true;
                Decreasing = false;
            }
            else if (newDps < oldDps)
            {
                Increasing = false;
                Decreasing = true;
            }
            else
            {
                Increasing = false;
                Decreasing = false;
            }

            DamageFactorChanged?.Invoke(DamageFactor);

            if (Role == PlayerRole.Self)
            {
                ShowFirst = BasicTeraData.Instance.WindowData.MeterUserOnTop;
            }

            NotifyPropertyChanged(nameof(GlobalDps));
            NotifyPropertyChanged(nameof(IndividualDps));
            NotifyPropertyChanged(nameof(Amount));
            NotifyPropertyChanged(nameof(CritRate));
            NotifyPropertyChanged(nameof(FightDuration));
            NotifyPropertyChanged(nameof(CritMode));
            NotifyPropertyChanged(nameof(DamagePerc));
            NotifyPropertyChanged(nameof(DamageDone));

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
    }
}