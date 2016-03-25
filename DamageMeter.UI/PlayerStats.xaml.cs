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
            LabelName.ToolTip = playerInfo.Player.FullName;
        }

        public PlayerInfo PlayerInfo { get; set; }

        public string Dps => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Dps(CurrentBoss)) + "/s";

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Damage(CurrentBoss));


        public string DamageReceived => FormatHelpers.Instance.FormatValue(PlayerInfo.Received.Damage(CurrentBoss, _firstHit, _lastHit));

        public string HitReceived => PlayerInfo.Received.Hits(CurrentBoss, _firstHit, _lastHit).ToString();

        public string CritRate => Math.Round(PlayerInfo.Dealt.GetCritRate(CurrentBoss)) + "%";


        public string PlayerName => PlayerInfo.Name;


        public string DamagePart(long totalDamage)
        {
            return Math.Round(PlayerInfo.Dealt.DamageFraction(CurrentBoss, totalDamage)) + "%";
        }

        public Entity CurrentBoss { get; private set; }
        private long _firstHit;
        private long _lastHit;

        public void Repaint(PlayerInfo playerInfo, long totalDamage, long firstHit, long lastHit, Entity currentBoss)
        {
            PlayerInfo = playerInfo;
            CurrentBoss = currentBoss;
            LabelDps.Content = Dps;
            _firstHit = firstHit;
            _lastHit = lastHit;

            LabelCritRate.Content = CritRate;
            LabelDamagePart.Content = DamagePart(totalDamage);
            LabelDamageReceived.Content = DamageReceived;
            LabelHitsReceived.Content = HitReceived;
            var intervalTimespan = TimeSpan.FromSeconds(playerInfo.Dealt.Interval(CurrentBoss));
            Timer.Content = intervalTimespan.ToString(@"mm\:ss");

            var skills = Skills();
            var allskills = AllSkills();
            _windowSkill?.Update(skills,allskills, playerInfo, CurrentBoss);
            LabelDamage.Content = Damage;
            DpsIndicator.Width = 450*PlayerInfo.Dealt.DamageFraction(CurrentBoss,totalDamage)/100;
        }



        private Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> Skills()
        {
            if (CurrentBoss == null)
            {
                return
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                        PlayerInfo.Dealt.AllSkills);
            }

            
            if (PlayerInfo.Dealt.ContainsEntity(CurrentBoss))
            {

                if (NetworkController.Instance.TimedEncounter)
                {
                    return PlayerInfo.Dealt.GetSkillsByTime(CurrentBoss);
                }
                return PlayerInfo.Dealt.GetSkills(CurrentBoss);
            }

            return new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>();
        }



        private Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> AllSkills()
        {
            if (CurrentBoss == null)
            {
                return
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                        PlayerInfo.Dealt.AllSkills);
            }


            if (PlayerInfo.Dealt.ContainsEntity(CurrentBoss))
            {

                if (NetworkController.Instance.TimedEncounter)
                {
                    return PlayerInfo.Dealt.GetSkillsByTime(CurrentBoss);
                }
                return new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(PlayerInfo.Dealt.AllSkills);
            }

            return new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>();
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
            var skills = Skills();
            var allSkills = AllSkills();

            if (_windowSkill == null)
            {
                _windowSkill = new Skills(skills, allSkills, this, PlayerInfo, CurrentBoss)
                {
                    Title = PlayerName,
                    CloseMeter = {Content = PlayerInfo.Class + " " + PlayerName + ": CLOSE"}
                };
                _windowSkill.Show();
                return;
            }

            _windowSkill.Update(skills,allSkills, PlayerInfo, CurrentBoss);
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