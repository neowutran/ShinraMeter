using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Data;
using Tera.Game;
using DamageMeter.Database.Structures;

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


        public Skills(PlayerStats parent, PlayerDealt playerDealt, EntityInformation entityInformation, Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            InitializeComponent();
                 
            TabControl.SelectionChanged += TabControlOnSelectionChanged;
            _parent = parent;
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            BackgroundColor.Opacity = BasicTeraData.Instance.WindowData.SkillWindowOpacity;
            Update(playerDealt, entityInformation, skills, buffs, timedEncounter);
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

        public void Update(PlayerDealt playerDealt, EntityInformation entityInformation, Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
     
            var death = buffs.Death;
            if (death == null)
            {
                DeathCounter.Content = 0;
                DeathDuration.Content = "0s";
            }
            else {
                DeathCounter.Content = death.Count(entityInformation.BeginTime, entityInformation.EndTime);
                var duration = death.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                var interval = TimeSpan.FromTicks(duration);
                DeathDuration.Content = interval.ToString(@"mm\:ss");
            }
            var aggro = buffs.Aggro(entityInformation.Entity);
            if (aggro == null)
            {
                AggroCounter.Content = 0;
                AggroDuration.Content = "0s";
            }
            else
            {
                AggroCounter.Content = aggro.Count(entityInformation.BeginTime, entityInformation.EndTime);
                var duration = aggro.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                var interval = TimeSpan.FromTicks(duration);
                AggroDuration.Content = interval.ToString(@"mm\:ss");
            }


            if (_skillDps == null)
            {
                _skillDps = new SkillsDetail(skills, playerDealt, entityInformation, Database.Database.Type.Damage, timedEncounter);
                _skillHeal = new SkillsDetail(skills, playerDealt, entityInformation, Database.Database.Type.Heal, timedEncounter);
                _skillMana = new SkillsDetail(skills, playerDealt, entityInformation, Database.Database.Type.Mana, timedEncounter);
                _buff = new Buff(playerDealt, buffs, entityInformation);

            }
            else {
                _skillDps.Update(skills, entityInformation, playerDealt, timedEncounter);
                _skillHeal.Update(skills, entityInformation, playerDealt, timedEncounter);
                _skillMana.Update(skills, entityInformation, playerDealt, timedEncounter);
                _buff.Update(playerDealt, buffs, entityInformation);
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