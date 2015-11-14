using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tera.DamageMeter.UI.Handler;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour PlayerStats.xaml
    /// </summary>
    public partial class PlayerStats : UserControl
    {
        public ImageSource Image;
        private Skills _windowSkill;

        public PlayerStats(PlayerInfo playerInfo)
        {
            InitializeComponent();
            PlayerData = new PlayerData(playerInfo);
        
            PlayerData.TotalDamageChanged += Repaint;
            _windowSkill = new Skills(PlayerData.PlayerInfo.Dealt.Skills);

            Image = ClassIcons.Instance.GetImage(PlayerData.PlayerInfo.Class).Source;
            Console.WriteLine(PlayerData.PlayerInfo.Class);
            Class.Source = Image;
            LabelName.Content = PlayerName;
        }

        public PlayerData PlayerData { get; set; }

        public string DPS => FormatHelpers.Instance.FormatValue(PlayerData.PlayerInfo.Dps) + "/s";

        public string DamagePart => PlayerData.DamageFraction + "%";

        public string DamageReceived => FormatHelpers.Instance.FormatValue(PlayerData.PlayerInfo.Received.Damage) + "";

        public string CritRate => PlayerData.PlayerInfo.Dealt.CritRate + "%";

        public string PlayerName => PlayerData.PlayerInfo.Name;

        public void Repaint()
        {
            BackgroundCache.Width = ActualWidth;
            DpsIndicator.Width = ActualWidth*(PlayerData.DamageFraction/100);
            LabelDPS.Content = DPS;
            LabelCritRate.Content = CritRate;
            LabelDamagePart.Content = DamagePart;
            LabelDamageReceived.Content = DamageReceived;
            _windowSkill.Update(PlayerData.PlayerInfo.Dealt.Skills);
        }


        private void ShowSkills(object sender, MouseButtonEventArgs e)
        {
            _windowSkill.Show();
        }


        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            w.DragMove();
        }
    }
}