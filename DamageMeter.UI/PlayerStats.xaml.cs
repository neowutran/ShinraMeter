using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.Skills.Skill;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour PlayerStats.xaml
    /// </summary>
    public partial class PlayerStats
    {
        private Skills _windowSkill;
        public ImageSource Image;

        public PlayerStats(PlayerInfo playerInfo)
        {
            InitializeComponent();
            PlayerInfo = playerInfo;
            Image = ClassIcons.Instance.GetImage(PlayerInfo.Class).Source;
            Class.Source = Image;
            LabelName.Content = PlayerName;
        }

        public PlayerInfo PlayerInfo { get; set; }

        public string Dps => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Dps) + "/s";

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Damage);


        public string DamageReceived => FormatHelpers.Instance.FormatValue(PlayerInfo.Received.Damage);

        public string HitReceived => PlayerInfo.Received.Hits.ToString();

        public string CritRate => Math.Round(PlayerInfo.Dealt.CritRate) + "%";


        public string PlayerName => PlayerInfo.Name;


        public string DamagePart(long totalDamage)
        {
            return Math.Round(PlayerInfo.Dealt.DamageFraction(totalDamage)) + "%";
        }

        public void Repaint(PlayerInfo playerInfo, long totalDamage, double width)
        {
            PlayerInfo = playerInfo;
            LabelDps.Content = Dps;
            LabelDamage.Content = Damage;
            LabelCritRate.Content = CritRate;
            LabelDamagePart.Content = DamagePart(totalDamage);
            LabelDamageReceived.Content = DamageReceived;
            LabelHitsReceived.Content = HitReceived;
            var intervalTimespan = TimeSpan.FromSeconds(playerInfo.Dealt.Interval);
            Timer.Content = intervalTimespan.ToString(@"mm\:ss");

            _windowSkill?.Update(Skills(),new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(PlayerInfo.Dealt.AllSkills));
            DpsIndicator.Width = width*(PlayerInfo.Dealt.DamageFraction(totalDamage)/100);
        }

        private Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> Skills()
        {
            if (NetworkController.Instance.Encounter == null)
            {
                return new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(PlayerInfo.Dealt.AllSkills);
            }
            if (PlayerInfo.Dealt.ContainsEntity(NetworkController.Instance.Encounter))
            {
                return
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(PlayerInfo.Dealt.GetSkills(NetworkController.Instance.Encounter));
            }

            return new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>();
        }

        private void ShowSkills(object sender, MouseButtonEventArgs e)
        {
            if (_windowSkill == null)
            {
                _windowSkill = new Skills(Skills(),
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(PlayerInfo.Dealt.AllSkills), this)
                {
                    Title = PlayerName,
                    CloseMeter = {Content = PlayerInfo.Class + " " + PlayerName + ": CLOSE"}
                };
            }

            _windowSkill.Show();
            _windowSkill.Update(Skills(),
                new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(PlayerInfo.Dealt.AllSkills));
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