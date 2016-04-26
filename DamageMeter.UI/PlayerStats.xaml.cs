using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.Skills.Skill;
using Data;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour PlayerStats.xaml
    /// </summary>
    public partial class PlayerStats
    {
        private Skills _windowSkill;
        public ImageSource Image;

        public PlayerStats(PlayerInfo playerInfo, PlayerAbnormals buffs)
        {
            InitializeComponent();
            PlayerInfo = playerInfo;
            _buffs = buffs;
            Image = ClassIcons.Instance.GetImage(PlayerInfo.Class).Source;
            Class.Source = Image;
            LabelName.Content = PlayerName;
            LabelName.ToolTip = playerInfo.Player.FullName;
        }

        public PlayerInfo PlayerInfo { get; set; }

        public string Dps => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Dps(_currentBoss, _timedEncounter)) + "/s";

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Damage(_currentBoss, _timedEncounter));

        public string GlobalDps => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.GlobalDps(_currentBoss, _timedEncounter, _lastHit - _firstHit)) + "/s";

        public string DamageReceived => FormatHelpers.Instance.FormatValue(PlayerInfo.Received.Damage(_currentBoss, _firstHit, _lastHit, _timedEncounter));

        public string HitReceived => PlayerInfo.Received.Hits(_currentBoss, _firstHit, _lastHit, _timedEncounter).ToString();

        public string CritRate => Math.Round(PlayerInfo.Dealt.CritRate(_currentBoss, _timedEncounter)) + "%";


        public string PlayerName => PlayerInfo.Name;


        public string DamagePart(long totalDamage)
        {
            return Math.Round(PlayerInfo.Dealt.DamageFraction(_currentBoss, totalDamage, _timedEncounter)) + "%";
        }

        private Entity _currentBoss;
        private long _firstHit;
        private long _lastHit;
        private PlayerAbnormals _buffs;

        private bool _timedEncounter;

        public void Repaint(PlayerInfo playerInfo, PlayerAbnormals buffs, long totalDamage, long firstHit, long lastHit, Entity currentBoss, bool timedEncounter)
        {
            PlayerInfo = playerInfo;
            _buffs = buffs;
            _currentBoss = currentBoss;
            _firstHit = firstHit;
            _lastHit = lastHit;
            _timedEncounter = timedEncounter;

            LabelDps.Content = GlobalDps;
            LabelDps.ToolTip = "Individual dps: " +Dps;
            LabelCritRate.Content = CritRate;
            var intervalTimespan = TimeSpan.FromSeconds(playerInfo.Dealt.Interval(_currentBoss));
            LabelCritRate.ToolTip = "Hits received: " + HitReceived+" - Damage received: "+DamageReceived+" - Fight Duration: "+ intervalTimespan.ToString(@"mm\:ss");
            LabelDamagePart.Content = DamagePart(totalDamage);
            LabelDamagePart.ToolTip = "Damage done: " + Damage;
        

            var skills = Skills(_timedEncounter);
            var allskills = AllSkills(_timedEncounter);
            _windowSkill?.Update(skills,allskills, playerInfo, _buffs, _currentBoss, _timedEncounter, firstHit, lastHit);
            DpsIndicator.Width = 265*PlayerInfo.Dealt.DamageFraction(_currentBoss, totalDamage, _timedEncounter) /100;
        }



        private Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> Skills(bool timedEncounter)
        {
            if (_currentBoss == null)
            {
                return
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                        PlayerInfo.Dealt.AllSkills);
            }

            
            if (PlayerInfo.Dealt.ContainsEntity(_currentBoss))
            {

                if (timedEncounter)
                {
                    return PlayerInfo.Dealt.GetSkillsByTime(_currentBoss);
                }
                return PlayerInfo.Dealt.GetSkills(_currentBoss);
            }

            return new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>();
        }



        private Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> AllSkills(bool timedEncounter)
        {
            if (_currentBoss == null)
            {
                return
                    new Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>>(
                        PlayerInfo.Dealt.AllSkills);
            }


            if (PlayerInfo.Dealt.ContainsEntity(_currentBoss))
            {

                if (timedEncounter)
                {
                    return PlayerInfo.Dealt.GetSkillsByTime(_currentBoss);
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
            var skills = Skills(_timedEncounter);
            var allSkills = AllSkills(_timedEncounter);

            if (_windowSkill == null)
            {
                _windowSkill = new Skills(skills, allSkills, this, PlayerInfo, _buffs, _currentBoss, _timedEncounter, _firstHit, _lastHit)
                {
                    Title = PlayerName,
                    CloseMeter = {Content = PlayerInfo.Class + " " + PlayerName + ": CLOSE"}
                };
                _windowSkill.Show();
                return;
            }

            _windowSkill.Update(skills,allSkills, PlayerInfo, _buffs, _currentBoss, _timedEncounter, _firstHit, _lastHit);
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