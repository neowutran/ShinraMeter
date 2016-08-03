using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DamageMeter.Database.Structures;
using Data;
using Tera.Game.Abnormality;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour PlayerStats.xaml
    /// </summary>
    public partial class PlayerStats
    {
        private PlayerAbnormals _buffs;

        private bool _timedEncounter;
        private Skills _windowSkill;
        public ImageSource Image;

        public PlayerStats(PlayerDamageDealt playerDamageDealt, PlayerHealDealt playeHealDealt, EntityInformation entityInformation,
            Database.Structures.Skills skills, PlayerAbnormals buffs)
        {
            InitializeComponent();
            PlayerDamageDealt = playerDamageDealt;
            PlayerHealDealt = playeHealDealt;
            EntityInformation = entityInformation;
            Skills = skills;
            _buffs = buffs;
            Image = ClassIcons.Instance.GetImage(PlayerDamageDealt.Source.Class).Source;
            Class.Source = Image;
            LabelName.Content = PlayerName;
            LabelName.ToolTip = PlayerDamageDealt.Source.FullName;
        }

        public PlayerDamageDealt PlayerDamageDealt { get; set; }

        public PlayerHealDealt PlayerHealDealt { get; set; }

        public Database.Structures.Skills Skills { get; set; }

        public EntityInformation EntityInformation { get; set; }

        public string Dps
            =>
                FormatHelpers.Instance.FormatValue(PlayerDamageDealt.Interval == 0
                    ? PlayerDamageDealt.Amount
                    : PlayerDamageDealt.Amount*TimeSpan.TicksPerSecond/PlayerDamageDealt.Interval) + LangPack.PerSecond;

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerDamageDealt.Amount);

        public string GlobalDps
            =>
                FormatHelpers.Instance.FormatValue(EntityInformation.Interval == 0
                    ? PlayerDamageDealt.Amount
                    : PlayerDamageDealt.Amount*TimeSpan.TicksPerSecond/EntityInformation.Interval) + LangPack.PerSecond;

        public string CritRate => Math.Round(PlayerDamageDealt.CritRate) + "%";
        public string CritRateHeal => Math.Round(PlayerHealDealt?.CritRate ?? 0) + "%";


        public string PlayerName => PlayerDamageDealt.Source.Name;


        public string DamagePart => Math.Round((double) PlayerDamageDealt.Amount*100/EntityInformation.TotalDamage) + "%";

        public void Repaint(PlayerDamageDealt playerDamageDealt, PlayerHealDealt playerHealDealt, EntityInformation entityInformation,
            Database.Structures.Skills skills,
            PlayerAbnormals buffs, bool timedEncounter)
        {
            PlayerHealDealt = playerHealDealt;
            EntityInformation = entityInformation;
            PlayerDamageDealt = playerDamageDealt;
            _buffs = buffs;
            _timedEncounter = timedEncounter;
            Skills = skills;
            LabelDps.Content = GlobalDps;
            LabelDps.ToolTip = $"{LangPack.Individual_dps}: {Dps}";
            LabelCritRate.Content = PlayerDamageDealt.Source.IsHealer && BasicTeraData.Instance.WindowData.ShowHealCrit
                ? CritRateHeal
                : CritRate;
            var intervalTimespan = TimeSpan.FromSeconds(PlayerDamageDealt.Interval/TimeSpan.TicksPerSecond);
            LabelCritRate.ToolTip = $"{LangPack.Fight_Duration}: {intervalTimespan.ToString(@"mm\:ss")}";
            LabelCritRate.Foreground = PlayerDamageDealt.Source.IsHealer && BasicTeraData.Instance.WindowData.ShowHealCrit
                ? Brushes.LawnGreen
                : Brushes.LightCoral;
            LabelDamagePart.Content = DamagePart;
            LabelDamagePart.ToolTip = $"{LangPack.Damage_done}: {Damage}";

            _windowSkill?.Update(PlayerDamageDealt, EntityInformation, Skills, _buffs, _timedEncounter);
            DpsIndicator.Width = EntityInformation.TotalDamage == 0
                ? 265
                : 265*PlayerDamageDealt.Amount/EntityInformation.TotalDamage;
        }

        public void SetClickThrou()
        {
            _windowSkill?.SetClickThrou();
        }

        public void UnsetClickThrou()
        {
            _windowSkill?.UnsetClickThrou();
        }


        private void ShowSkills(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_windowSkill == null)
            {
                _windowSkill = new Skills(this, PlayerDamageDealt, EntityInformation, Skills, _buffs, _timedEncounter)
                {
                    Title = PlayerName,
                    CloseMeter = {Content = PlayerDamageDealt.Source.Class + " " + PlayerName + ": "+LangPack.Close}
                };
                Screen screen = Screen.FromHandle(new WindowInteropHelper(Window.GetWindow(this)).Handle);
                Window main = Window.GetWindow(this);
                // Transform screen point to WPF device independent point
                PresentationSource source = PresentationSource.FromVisual(this);

                if (source?.CompositionTarget == null)
                {   //if this can't be determined, just use the center screen logic
                    _windowSkill.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                else
                {
                    // WindowStartupLocation.CenterScreen sometimes put window out of screen in multi monitor environment
                    _windowSkill.WindowStartupLocation = WindowStartupLocation.Manual;
                    Matrix m = source.CompositionTarget.TransformToDevice;
                    double dx = m.M11;
                    double dy = m.M22;
                    var maxWidth = 856 * BasicTeraData.Instance.WindowData.Scale;
                    Point locationFromScreen;
                    if (screen.WorkingArea.X+screen.WorkingArea.Width > (main.Left + main.Width + maxWidth) * dx)
                        locationFromScreen = new Point(
                            (main.Left + main.Width) * dx,
                            main.Top * dy);
                    else if (screen.WorkingArea.X + maxWidth * dx < main.Left * dx)
                        locationFromScreen = new Point(
                            (main.Left - maxWidth) * dx,
                            main.Top * dy);
                    else
                        locationFromScreen = new Point(
                            screen.WorkingArea.X + (screen.WorkingArea.Width - maxWidth * dx) / 2,
                            screen.WorkingArea.Y + (screen.WorkingArea.Height - 600 * dy) / 2);
                    Point targetPoints = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
                    _windowSkill.Left = targetPoints.X;
                    _windowSkill.Top = targetPoints.Y;
                }

                _windowSkill.Show();
                NetworkController.Instance.SendFullDetails = true;
                return;
            }

            _windowSkill.Update(PlayerDamageDealt, EntityInformation, Skills, _buffs, _timedEncounter);
            _windowSkill.Show();
        }

        private void ChangeHeal(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                BasicTeraData.Instance.WindowData.ShowHealCrit = !BasicTeraData.Instance.WindowData.ShowHealCrit;
        }

        public void CloseSkills()
        {
            _windowSkill?.Close();
            _windowSkill = null;
        }


        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try
            {
                w?.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}