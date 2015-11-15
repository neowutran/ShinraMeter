using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Tera.DamageMeter.UI.Handler;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour PlayerStats.xaml
    /// </summary>
    public partial class PlayerStats
    {
        private readonly Skills _windowSkill;
        public ImageSource Image;

        public PlayerStats(PlayerInfo playerInfo)
        {
            InitializeComponent();
            PlayerData = new PlayerData(playerInfo);
            _windowSkill = new Skills(PlayerData.PlayerInfo.Dealt.Skills);
            Image = ClassIcons.Instance.GetImage(PlayerData.PlayerInfo.Class).Source;
            Class.Source = Image;
            LabelName.Content = PlayerName;
        }

        public PlayerData PlayerData { get; set; }

        public string Dps => FormatHelpers.Instance.FormatValue(PlayerData.PlayerInfo.Dps) + "/s";

        public string DamagePart => Math.Round(PlayerData.DamageFraction) + "%";

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerData.PlayerInfo.Dealt.Damage);

        public string DamageReceived => FormatHelpers.Instance.FormatValue(PlayerData.PlayerInfo.Received.Damage);

        public string CritRate => Math.Round(PlayerData.PlayerInfo.Dealt.CritRate) + "%";

        public string PlayerName => PlayerData.PlayerInfo.Name;

        public void Repaint()
        {
            DpsIndicator.Width = ActualWidth*(PlayerData.DamageFraction/100);
            LabelDps.Content = Dps;
            LabelDamage.Content = Damage;
            LabelCritRate.Content = CritRate;
            LabelDamagePart.Content = DamagePart;
            LabelDamageReceived.Content = DamageReceived;
            _windowSkill.Update(PlayerData.PlayerInfo.Dealt.Skills);
        }

        private void ShowSkills(object sender, MouseButtonEventArgs e)
        {
            
                _windowSkill.Show();
           
        }

        public void CloseSkills()
        {
            _windowSkill.Close();
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
                Console.WriteLine("Exception move");
            }
        }
    }
}