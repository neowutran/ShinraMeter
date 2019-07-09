using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DamageMeter.Database.Structures;
using Data;
using Lang;
using Tera.Game.Abnormality;
using System.Windows.Media.Animation;
using Tera.Game;

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
            if(playerDamageDealt.Source.Class != PlayerClass.Common) LabelLevel.Content = playerDamageDealt.Source.Level;

            var isMe = PlayerDamageDealt.Source.User.Id.Id == PacketProcessor.Instance.PlayerTracker.Me().User.Id.Id;

            var meCol100 = BasicTeraData.Instance.WindowData.PlayerColor; 
            var meCol30 = Color.FromArgb((byte) (meCol100.A == 0 ? 0 : 60), meCol100.R, meCol100.G, meCol100.B);
            var meCol0 = Color.FromArgb(0, meCol100.R, meCol100.G, meCol100.B);

            var dpsCol100 = BasicTeraData.Instance.WindowData.DpsColor;
            var dpsCol30 = Color.FromArgb((byte)(dpsCol100.A == 0 ? 0 : 60), dpsCol100.R, dpsCol100.G, dpsCol100.B);
            var dpsCol0 = Color.FromArgb(0, dpsCol100.R, dpsCol100.G, dpsCol100.B);

            var tankCol100 = BasicTeraData.Instance.WindowData.TankColor;
            var tankCol30 = Color.FromArgb((byte)(tankCol100.A == 0 ? 0 : 60), tankCol100.R, tankCol100.G, tankCol100.B);
            var tankCol0 = Color.FromArgb(0, tankCol100.R, tankCol100.G, tankCol100.B);

            var healCol100 = BasicTeraData.Instance.WindowData.HealerColor;
            var healCol30 = Color.FromArgb((byte)(healCol100.A == 0 ? 0 : 60), healCol100.R, healCol100.G, healCol100.B);
            var healCol0 = Color.FromArgb(0, healCol100.R, healCol100.G, healCol100.B);

            var unkCol100 = Color.FromArgb(255, 200, 200, 200);
            var unkCol30 = Color.FromArgb(60, 200, 200, 200);
            var unkCol0 = Color.FromArgb(0, 200, 200, 200);

            if (playerDamageDealt != null)
            {
                switch (playerDamageDealt.Source.Class)
                {
                    case Tera.Game.PlayerClass.Warrior:
                    case Tera.Game.PlayerClass.Slayer:
                    case Tera.Game.PlayerClass.Berserker:
                    case Tera.Game.PlayerClass.Sorcerer:
                    case Tera.Game.PlayerClass.Archer:
                    case Tera.Game.PlayerClass.Reaper:
                    case Tera.Game.PlayerClass.Gunner:
                    case Tera.Game.PlayerClass.Ninja:
                    case Tera.Game.PlayerClass.Valkyrie:
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[0] = new GradientStop(isMe?  meCol30 : dpsCol30, 1);
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[1] = new GradientStop(isMe?  meCol0 : dpsCol0, 0);
                        DpsIndicator.BorderBrush = new SolidColorBrush(isMe ? meCol100 : dpsCol100);
                        break;
                    case Tera.Game.PlayerClass.Priest:
                    case Tera.Game.PlayerClass.Mystic:
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[0] = new GradientStop(isMe?  meCol30 :healCol30, 1);
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[1] = new GradientStop(isMe ? meCol0 : healCol0, 0);
                        DpsIndicator.BorderBrush = new SolidColorBrush(isMe ? meCol100 : healCol100);
                        break;
                    case Tera.Game.PlayerClass.Brawler:
                    case Tera.Game.PlayerClass.Lancer:
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[0] = new GradientStop(isMe?  meCol30 :tankCol30, 1);
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[1] = new GradientStop(isMe ? meCol0 : tankCol0, 0);
                        DpsIndicator.BorderBrush = new SolidColorBrush(isMe ? meCol100 : tankCol100);
                        break;
                    case Tera.Game.PlayerClass.Common:
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[0] = new GradientStop(unkCol30, 1);
                        (DpsIndicator.Background as LinearGradientBrush).GradientStops[1] = new GradientStop(unkCol0, 0);
                        DpsIndicator.BorderBrush = new SolidColorBrush(unkCol100);
                        break;
                }
            }
        }

        public PlayerDamageDealt PlayerDamageDealt { get; set; }

        public PlayerHealDealt PlayerHealDealt { get; set; }

        public Database.Structures.Skills Skills { get; set; }

        public EntityInformation EntityInformation { get; set; }

        public string Dps => FormatHelpers.Instance.FormatValue(PlayerDamageDealt.Interval == 0
                                 ? PlayerDamageDealt.Amount
                                 : PlayerDamageDealt.Amount * TimeSpan.TicksPerSecond / PlayerDamageDealt.Interval) + LP.PerSecond;

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerDamageDealt.Amount);

        public string GlobalDps => FormatHelpers.Instance.FormatValue(EntityInformation.Interval == 0
                                       ? PlayerDamageDealt.Amount
                                       : PlayerDamageDealt.Amount * TimeSpan.TicksPerSecond / EntityInformation.Interval) + LP.PerSecond;

        public string CritRate => Math.Round(PlayerDamageDealt.CritRate) + "%";
        public string CritDamageRate => Math.Round(PlayerDamageDealt.CritDamageRate) + "%";
        public string CritRateHeal => Math.Round(PlayerHealDealt?.CritRate ?? 0) + "%";


        public string PlayerName => PlayerDamageDealt.Source.Name;


        public string DamagePart => Math.Round((double)PlayerDamageDealt.Amount * 100 / EntityInformation.TotalDamage) + "%";

        public void Repaint(PlayerDamageDealt playerDamageDealt, PlayerHealDealt playerHealDealt, EntityInformation entityInformation,
            Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            PlayerHealDealt = playerHealDealt;
            EntityInformation = entityInformation;
            PlayerDamageDealt = playerDamageDealt;
            _buffs = buffs;
            _timedEncounter = timedEncounter;
            Skills = skills;
            LabelDps.Content = GlobalDps;
            LabelDps.ToolTip = $"{LP.Individual_dps}: {Dps}";
            LabelCritRate.Content = PlayerDamageDealt.Source.IsHealer && BasicTeraData.Instance.WindowData.ShowHealCrit
                ? CritRateHeal
                : BasicTeraData.Instance.WindowData.ShowCritDamageRate
                    ? CritDamageRate
                    : CritRate;
            var intervalTimespan = TimeSpan.FromSeconds(PlayerDamageDealt.Interval / TimeSpan.TicksPerSecond);
            LabelCritRate.ToolTip = $"{LP.Fight_Duration}: {intervalTimespan.ToString(@"mm\:ss")}";
            LabelCritRate.Foreground = PlayerDamageDealt.Source.IsHealer && BasicTeraData.Instance.WindowData.ShowHealCrit
                ? Brushes.LawnGreen
                : BasicTeraData.Instance.WindowData.ShowCritDamageRate
                    ? Brushes.Orange
                    : Brushes.LightCoral;
            LabelDamagePart.Content = DamagePart;
            LabelDamagePart.ToolTip = $"{LP.Damage_done}: {Damage}";

            _windowSkill?.Update(PlayerDamageDealt, EntityInformation, Skills, _buffs, _timedEncounter);
            Spacer.Width = 0;
            GridStats.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var SGrid = ((MainWindow)((FrameworkElement)((FrameworkElement)((FrameworkElement)Parent).Parent).Parent).Parent).SGrid;
            var EGrid = ((MainWindow)((FrameworkElement)((FrameworkElement)((FrameworkElement)Parent).Parent).Parent).Parent).EGrid;
            SGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var mainWidth = SGrid.DesiredSize.Width;
            Spacer.Width = mainWidth > GridStats.DesiredSize.Width ? mainWidth - GridStats.DesiredSize.Width : 0;
            var an = new DoubleAnimation(DpsIndicator.ActualWidth, EntityInformation.TotalDamage == 0 ? mainWidth : mainWidth * PlayerDamageDealt.Amount / EntityInformation.TotalDamage, TimeSpan.FromMilliseconds(400)) { EasingFunction = new QuadraticEase() };
            DpsIndicator.BeginAnimation(WidthProperty, an);
            EGrid.MaxWidth = Math.Max(mainWidth, GridStats.DesiredSize.Width);
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
                    PlayerNameTB = { Text = PlayerName }
                    //CloseMeter = {Content = LP.ResourceManager.GetString(PlayerDamageDealt.Source.Class.ToString(), LP.Culture) + " " + PlayerName + ": " + LP.Close}
                };
                var main = Window.GetWindow(this);
                if (main == null) { _windowSkill = null; return;}//switched bosses while loading details window
                var screen = Screen.FromHandle(new WindowInteropHelper(main).Handle);
                // Transform screen point to WPF device independent point
                var source = PresentationSource.FromVisual(this);

                if (source?.CompositionTarget == null)
                {
                    //if this can't be determined, just use the center screen logic
                    _windowSkill.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                else
                {
                    // WindowStartupLocation.CenterScreen sometimes put window out of screen in multi monitor environment
                    _windowSkill.WindowStartupLocation = WindowStartupLocation.Manual;
                    var m = source.CompositionTarget.TransformToDevice;
                    var dx = m.M11;
                    var dy = m.M22;
                    ((UIElement)_windowSkill.DpsPanel.Content).Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    var maxWidth = (((UIElement)_windowSkill.DpsPanel.Content).DesiredSize.Width + 6) * BasicTeraData.Instance.WindowData.Scale;
                    Point locationFromScreen;
                    if (screen.WorkingArea.X + screen.WorkingArea.Width > (main.Left + main.Width + maxWidth) * dx)
                    {
                        locationFromScreen = new Point((main.Left + main.Width) * dx, main.Top * dy);
                    }
                    else if (screen.WorkingArea.X + maxWidth * dx < main.Left * dx) { locationFromScreen = new Point((main.Left - maxWidth) * dx, main.Top * dy); }
                    else
                    {
                        locationFromScreen = new Point(screen.WorkingArea.X + (screen.WorkingArea.Width - maxWidth * dx) / 2,
                            screen.WorkingArea.Y + (screen.WorkingArea.Height - 600 * dy) / 2);
                    }
                    var targetPoints = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
                    _windowSkill.Left = targetPoints.X;
                    _windowSkill.Top = targetPoints.Y;
                }
            }
            PacketProcessor.Instance.SendFullDetails = true;
            _windowSkill.ShowWindow();
        }

        private void ChangeHeal(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) { return; }
            if (PlayerDamageDealt.Source.IsHealer) { BasicTeraData.Instance.WindowData.ShowHealCrit = !BasicTeraData.Instance.WindowData.ShowHealCrit; }
            else { BasicTeraData.Instance.WindowData.ShowCritDamageRate = !BasicTeraData.Instance.WindowData.ShowCritDamageRate; }
        }

        public void CloseSkills()
        {
            _windowSkill?.Close();
            _windowSkill = null;
        }


        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }

        private void GridStats_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GridStats.Background = new SolidColorBrush(Color.FromArgb(0x10, 0xb0, 0xb0, 0xb0));
        }

        private void GridStats_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GridStats.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}