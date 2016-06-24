using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Data;
using Tera.Game;
using DamageMeter.Database.Structures;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour PlayerStats.xaml
    /// </summary>
    public partial class PlayerStats
    {
        private Skills _windowSkill;
        public ImageSource Image;

        public PlayerStats(PlayerDealt playerDealt, PlayerDealt playerDealtHeal, EntityInformation entityInformation, Database.Structures.Skills skills, PlayerAbnormals buffs)
        {
            InitializeComponent();
            PlayerDealt = playerDealt;
            PlayerDealtHeal = playerDealtHeal;
            EntityInformation = entityInformation;
            Skills = skills;
            _buffs = buffs;
            Image = ClassIcons.Instance.GetImage(PlayerDealt.Source.Class).Source;
            Class.Source = Image;
            LabelName.Content = PlayerName;
            LabelName.ToolTip = PlayerDealt.Source.FullName;
        }

        public PlayerDealt PlayerDealt { get; set; }

        public PlayerDealt PlayerDealtHeal { get; set; }

        public Database.Structures.Skills Skills { get; set; }

        public EntityInformation EntityInformation { get; set; }

        public string Dps => FormatHelpers.Instance.FormatValue( PlayerDealt.Interval == 0 ? PlayerDealt.Amount : (PlayerDealt.Amount * TimeSpan.TicksPerSecond) / PlayerDealt.Interval) + "/s";

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerDealt.Amount);

        public string GlobalDps => FormatHelpers.Instance.FormatValue( EntityInformation.Interval == 0 ? PlayerDealt.Amount : (PlayerDealt.Amount * TimeSpan.TicksPerSecond) / EntityInformation.Interval) + "/s";

        public string CritRate => Math.Round(PlayerDealt.CritRate * 100) + "%";
        public string CritRateHeal => Math.Round(PlayerDealtHeal == null ? 0 : (PlayerDealtHeal.CritRate * 100)) + "%";


        public string PlayerName => PlayerDealt.Source.Name;


        public string DamagePart => Math.Round((double)PlayerDealt.Amount / EntityInformation.TotalDamage) + "%";
       
        private PlayerAbnormals _buffs;

        private bool _timedEncounter;

        public void Repaint(PlayerDealt playerDealt, PlayerDealt playerDealtHeal, EntityInformation entityInformation, Database.Structures.Skills skills,
            PlayerAbnormals buffs, bool timedEncounter)
        {
            PlayerDealtHeal = playerDealtHeal;
            PlayerDealt = playerDealt;
            _buffs = buffs;
            _timedEncounter = timedEncounter;
            Skills = skills;
            LabelDps.Content = GlobalDps;
            LabelDps.ToolTip = "Individual dps: " +Dps;
            LabelCritRate.Content = PlayerDealt.Source.IsHealer && BasicTeraData.Instance.WindowData.ShowHealCrit ? CritRateHeal : CritRate;
            var intervalTimespan = TimeSpan.FromSeconds(PlayerDealt.Interval / TimeSpan.TicksPerSecond);
            LabelCritRate.ToolTip = "Fight Duration: "+ intervalTimespan.ToString(@"mm\:ss");
            LabelCritRate.Foreground = PlayerDealt.Source.IsHealer && BasicTeraData.Instance.WindowData.ShowHealCrit ? Brushes.LawnGreen : Brushes.LightCoral;
            LabelDamagePart.Content = DamagePart;
            LabelDamagePart.ToolTip = "Damage done: " + Damage;
        
            _windowSkill?.Update(PlayerDealt, EntityInformation, Skills, _buffs, _timedEncounter);
            DpsIndicator.Width = EntityInformation.TotalDamage == 0 ? 265 : (265 * PlayerDealt.Amount) / EntityInformation.TotalDamage;
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
                _windowSkill = new Skills(this, PlayerDealt, EntityInformation, Skills, _buffs, _timedEncounter)
                {
                    Title = PlayerName,
                    CloseMeter = {Content = PlayerDealt.Source.Class + " " + PlayerName + ": CLOSE"}
                };
                _windowSkill.Show();
                return;
            }

            _windowSkill.Update(PlayerDealt, EntityInformation , Skills, _buffs, _timedEncounter);
            _windowSkill.Show();
        }

        private void ChangeHeal(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount==2)
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