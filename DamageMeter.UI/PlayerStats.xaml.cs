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

        public void Repaint(PlayerInfo playerInfo, long totalDamage, long firstHit, long lastHit)
        {
            PlayerInfo = playerInfo;
            LabelDps.Content = Dps;

            LabelCritRate.Content = CritRate;
            LabelDamagePart.Content = DamagePart(totalDamage);
            LabelDamageReceived.Content = DamageReceived;
            LabelHitsReceived.Content = HitReceived;
            var intervalTimespan = TimeSpan.FromSeconds(playerInfo.Dealt.Interval);
            Timer.Content = intervalTimespan.ToString(@"mm\:ss");

            var skills = Skills();
            _windowSkill?.Update(skills,
                new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                    PlayerInfo.Dealt.AllSkills), playerInfo);
            //   var damageFraction = PlayerInfo.Dealt.DamageFraction(totalDamage);
            // Console.WriteLine("damageFraction:"+damageFraction+"; total damage:"+ totalDamage+";tt:"+PlayerInfo.Dealt.Damage);
            LabelDamage.Content = Damage;
            DpsIndicator.Width = 450*PlayerInfo.Dealt.DamageFraction(totalDamage)/100;
        }



        private Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> Skills()
        {
            if (NetworkController.Instance.Encounter == null)
            {
                return
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                        PlayerInfo.Dealt.AllSkills);
            }
            if (PlayerInfo.Dealt.ContainsEntity(NetworkController.Instance.Encounter))
            {
                return PlayerInfo.Dealt.GetSkills(NetworkController.Instance.Encounter);
            }

            return new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>();
        }


        private void ShowSkills(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var skills = Skills();
            if (_windowSkill == null)
            {
                _windowSkill = new Skills(skills,
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                        PlayerInfo.Dealt.AllSkills), this, PlayerInfo)
                {
                    Title = PlayerName,
                    CloseMeter = {Content = PlayerInfo.Class + " " + PlayerName + ": CLOSE"}
                };
                _windowSkill.Show();
                return;
            }

            _windowSkill.Update(skills,
                new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                    PlayerInfo.Dealt.AllSkills), PlayerInfo);
            _windowSkill.Show();
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