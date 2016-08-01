using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using DamageMeter.Database.Structures;
using Data;
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
        private SkillsDetail _skillDps;
        private SkillsDetail _skillHeal;
        private SkillsDetail _skillMana;
        private SkillsLog _skillDealtLog;
        private SkillsLog _skillReceivedLog;


        public Skills(PlayerStats parent, PlayerDamageDealt playerDamageDealt, EntityInformation entityInformation,
            Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            InitializeComponent();
            _parent = parent;
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            BackgroundColor.Opacity = BasicTeraData.Instance.WindowData.SkillWindowOpacity;
            Update(playerDamageDealt, entityInformation, skills, buffs, timedEncounter);
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

        private Database.Structures.Skills _skills;

        public void Update(PlayerDamageDealt playerDamageDealt, EntityInformation entityInformation,
            Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            var death = buffs.Death;
            if (death == null)
            {
                DeathCounter.Content = 0;
                DeathDuration.Content = "0" + Properties.Resources.Seconds;
            }
            else
            {
                DeathCounter.Content = death.Count(entityInformation.BeginTime, entityInformation.EndTime);
                var duration = death.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                var interval = TimeSpan.FromTicks(duration);
                DeathDuration.Content = interval.ToString(@"mm\:ss");
            }
            var aggro = buffs.Aggro(entityInformation.Entity);
            if (aggro == null)
            {
                AggroCounter.Content = 0;
                AggroDuration.Content = "0" + Properties.Resources.Seconds;
            }
            else
            {
                AggroCounter.Content = aggro.Count(entityInformation.BeginTime, entityInformation.EndTime);
                var duration = aggro.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                var interval = TimeSpan.FromTicks(duration);
                AggroDuration.Content = interval.ToString(@"mm\:ss");
            }

            if (skills != null)
            {
                _skills = skills;
            }
            if (_skills == null)
            {
                return;
            }

            var tabItem = (TabItem) TabControl.SelectedItem;
            if (tabItem == null) return;
            switch (tabItem.Name)
            {
                case "DpsPanel":
                    if (_skillDps == null)
                    {
                        _skillDps =
                            new SkillsDetail(
                                SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter,
                                    Database.Database.Type.Damage), Database.Database.Type.Damage);
                    }
                    DpsPanel.Content = _skillDps;
                    return;
                case "HealPanel":
                    if (_skillHeal == null)
                    {
                        _skillHeal =
                            new SkillsDetail(
                                SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter,
                                    Database.Database.Type.Heal), Database.Database.Type.Heal);
                    }
                    HealPanel.Content = _skillHeal;
                    return;
                case "ManaPanel":
                    if (_skillMana == null)
                    {
                        _skillMana =
                            new SkillsDetail(
                                SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter,
                                    Database.Database.Type.Mana), Database.Database.Type.Mana);
                    }
                    ManaPanel.Content = _skillMana;
                    return;
                case "BuffPanel":
                    if (_buff == null)
                    {
                        _buff = new Buff(playerDamageDealt, buffs, entityInformation);
                    }
                    BuffPanel.Content = _buff;
                    return;
                case "SkillsDealtPanel":
                    if (_skillDealtLog == null)
                    {
                        _skillDealtLog =
                            new SkillsLog(_skills.GetSkillsDealt(playerDamageDealt.Source.User.Id, entityInformation.Entity,
                                timedEncounter), false);
                    }
                    SkillsDealtPanel.Content = _skillDealtLog;
                    return;
                case "SkillsReceivedPanel":
                    if (_skillReceivedLog == null)
                    {
                        _skillReceivedLog =
                            new SkillsLog(_skills.GetSkillsReceived(playerDamageDealt.Source.User.Id, timedEncounter), true);
                    }
                    SkillsReceivedPanel.Content = _skillReceivedLog;
                    return;
            }
        }

     
        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseSkills();
        }
    }
}