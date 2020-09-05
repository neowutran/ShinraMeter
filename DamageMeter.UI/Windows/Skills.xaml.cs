using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using DamageMeter.Database.Structures;
using Data;
using Lang;
using Tera.Game.Abnormality;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills
    {
        private readonly PlayerStats _parent;
        private Buff _buff;
        private SkillsLog _skillDealtLog;
        private SkillsDetail _skillDps;
        private SkillsDetail _skillHeal;
        private SkillsDetail _skillMana;
        private SkillsDetail _skillCounter;
        private SkillsLog _skillReceivedLog;

        private Database.Structures.Skills _skills;


        public Skills(PlayerDamageViewModel vm)
        {
            DataContext = vm;

            InitializeComponent();

            #region Pasted from VM
            var main = App.Current.MainWindow;

            var screen = Screen.FromHandle(new WindowInteropHelper(main).Handle);
            // Transform screen point to WPF device independent point
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null)
            {
                //if this can't be determined, just use the center screen logic
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                // WindowStartupLocation.CenterScreen sometimes put window out of screen in multi monitor environment
                WindowStartupLocation = WindowStartupLocation.Manual;
                var m = source.CompositionTarget.TransformToDevice;
                var dx = m.M11;
                var dy = m.M22;
                ((UIElement)DpsPanel.Content).Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var maxWidth = (((UIElement)DpsPanel.Content).DesiredSize.Width + 6) * BasicTeraData.Instance.WindowData.Scale;
                Point locationFromScreen;
                if (screen.WorkingArea.X + screen.WorkingArea.Width > (main.Left + main.Width + maxWidth) * dx)
                {
                    locationFromScreen = new Point((main.Left + main.Width) * dx, main.Top * dy);
                }
                else if
                    (screen.WorkingArea.X + maxWidth * dx < main.Left * dx)
                {
                    locationFromScreen = new Point((main.Left - maxWidth) * dx, main.Top * dy);
                }
                else
                {
                    locationFromScreen = new Point(screen.WorkingArea.X + (screen.WorkingArea.Width - maxWidth * dx) / 2,
                        screen.WorkingArea.Y + (screen.WorkingArea.Height - 600 * dy) / 2);
                }
                var targetPoints = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
                Left = targetPoints.X;
                Top = targetPoints.Y;
            }

            #endregion

        }
        public Skills(PlayerStats parent, PlayerDamageDealt playerDamageDealt, EntityInformation entityInformation, Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            Owner = GetWindow(parent);
            InitializeComponent();
            _parent = parent;
            Update(playerDamageDealt, entityInformation, skills, buffs, timedEncounter);
        }

        public void Update(PlayerDamageDealt playerDamageDealt, EntityInformation entityInformation, Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            if (_skills != null && skills == null) { return; }

            _skills = skills;
            var death = buffs.Death;
            if (death == null)
            {
                DeathCounter.Text = "0";
                DeathDuration.Text = "0" + LP.Seconds;
            }
            else
            {
                DeathCounter.Text = death.Count(entityInformation.BeginTime, entityInformation.EndTime).ToString();
                var duration = death.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                var interval = TimeSpan.FromTicks(duration);
                DeathDuration.Text = interval.ToString(@"mm\:ss");
            }
            var aggro = buffs.Aggro(entityInformation.Entity);
            if (aggro == null)
            {
                AggroCounter.Text = "0";
                AggroDuration.Text = "0" + LP.Seconds;
            }
            else
            {
                AggroCounter.Text = aggro.Count(entityInformation.BeginTime, entityInformation.EndTime).ToString();
                var duration = aggro.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                var interval = TimeSpan.FromTicks(duration);
                AggroDuration.Text = interval.ToString(@"mm\:ss");
            }

            _skillDps = new SkillsDetail(SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Damage), Database.Database.Type.Damage);
            DpsPanel.Content = _skillDps;
            _skillHeal = new SkillsDetail(SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Heal), Database.Database.Type.Heal);
            HealPanel.Content = _skillHeal;
            _skillMana = new SkillsDetail(SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Mana), Database.Database.Type.Mana);
            ManaPanel.Content = _skillMana;
            _buff = new Buff(playerDamageDealt, buffs);
            BuffPanel.Content = _buff;

            _skillCounter = new SkillsDetail(SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Counter), Database.Database.Type.Counter);
            CounterPanel.Content = _skillCounter;
            _skillDealtLog = new SkillsLog(_skills?.GetSkillsDealt(playerDamageDealt.Source.User, entityInformation.Entity, timedEncounter), false);
            SkillsDealtPanel.Content = _skillDealtLog;
            _skillReceivedLog = new SkillsLog(_skills?.GetSkillsReceived(playerDamageDealt.Source.User, timedEncounter), true);
            SkillsReceivedPanel.Content = _skillReceivedLog;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseSkills();
        }

        private void ClickThrouWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //todo: use svg for these
            DeathIcon.Source = BasicTeraData.Instance.ImageDatabase.Skull.Source;
            DeathTimeIcon.Source = BasicTeraData.Instance.ImageDatabase.SkullTime.Source;
            AggroIcon.Source = BasicTeraData.Instance.ImageDatabase.BossGage.Source;
            AggroTimeIcon.Source = BasicTeraData.Instance.ImageDatabase.AggroTime.Source;
            
        }
    }
}