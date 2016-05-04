using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using DamageMeter.Skills.Skill;
using Data;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills
    {
        private Buff _buff;
        private readonly PlayerStats _parent;
        private SkillsDetail _skillDps;
        private SkillsDetail _skillHeal;
        private SkillsDetail _skillMana;


        public Skills(Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedSkills,
            Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedAllSkills, PlayerStats parent,
            PlayerInfo playerInfo, PlayerAbnormals buffs, Entity currentBoss, bool timedEncounter, long firstHit, long lastHit)
        {
            InitializeComponent();
                 
            TabControl.SelectionChanged += TabControlOnSelectionChanged;
            _parent = parent;
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            BackgroundColor.Opacity = BasicTeraData.Instance.WindowData.SkillWindowOpacity;
            Update(timedSkills, timedAllSkills, playerInfo, buffs, currentBoss, timedEncounter, firstHit, lastHit);
        }

        private Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats> NoTimedSkills(
            Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> dictionary)
        {
            var result = new Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>();
            foreach (var timedStats in dictionary)
            {
                foreach (var stats in timedStats.Value)
                {
                    if (result.ContainsKey(stats.Key))
                    {
                        result[stats.Key] += stats.Value;
                        continue;
                    }
                    result.Add(stats.Key, stats.Value);
                }
            }
            return result;
        }

        public void SetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }

        public void UnsetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExVisible(hwnd);
        }

        private void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var tabitem = (TabItem) ((TabControl) selectionChangedEventArgs.Source).SelectedItem;
        }

        public void Update(Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedSkills,
            Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedAllSkills,
            PlayerInfo playerinfo, PlayerAbnormals buffs, Entity currentBoss, bool timedEncounter, long firstHit, long lastHit)
        {
            var lastTick = (lastHit + 1) * TimeSpan.TicksPerSecond - 1;
            var firstTick = firstHit * TimeSpan.TicksPerSecond;
            var death = buffs.Death;
            if (death == null)
            {
                DeathCounter.Content = 0;
                DeathDuration.Content = "0s";
            }
            else {
                DeathCounter.Content = death.Count(firstTick, lastTick);
                var duration = death.Duration(firstTick, lastTick);
                var interval = TimeSpan.FromTicks(duration);
                DeathDuration.Content = interval.ToString(@"mm\:ss");
            }
            var aggro = buffs.Aggro(currentBoss?.NpcE);
            if (aggro == null)
            {
                AggroCounter.Content = 0;
                AggroDuration.Content = "0s";
            }
            else
            {
                AggroCounter.Content = aggro.Count(firstTick, lastTick);
                var duration = aggro.Duration(firstTick, lastTick);
                var interval = TimeSpan.FromTicks(duration);
                AggroDuration.Content = interval.ToString(@"mm\:ss");
            }

            var skills = NoTimedSkills(timedSkills);
            var allSkills = NoTimedSkills(timedAllSkills);

            if (_skillDps == null)
            {
                _skillDps = new SkillsDetail(skills, SkillsDetail.Type.Dps, currentBoss, timedEncounter);
                _skillHeal = new SkillsDetail(allSkills, SkillsDetail.Type.Heal, currentBoss, timedEncounter);
                _skillMana = new SkillsDetail(allSkills, SkillsDetail.Type.Mana, currentBoss, timedEncounter);
                _buff = new Buff(playerinfo, buffs, currentBoss);

            }
            else {
                _skillDps.Update(skills, currentBoss, timedEncounter);
                _skillHeal.Update(new Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>(allSkills), currentBoss, timedEncounter);
                _skillMana.Update(new Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>(allSkills), currentBoss, timedEncounter);
                _buff.Update(playerinfo, buffs, currentBoss);
            }
            HealPanel.Content = _skillHeal;
            DpsPanel.Content = _skillDps;
            ManaPanel.Content = _skillMana;
            BuffPanel.Content = _buff;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseSkills();
        }

    }
}