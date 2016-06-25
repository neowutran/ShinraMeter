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


        public Skills(PlayerStats parent, PlayerDealt playerDealt, EntityInformation entityInformation,
            Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            InitializeComponent();
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

        public void Update(PlayerDealt playerDealt, EntityInformation entityInformation,
            Database.Structures.Skills skills, PlayerAbnormals buffs, bool timedEncounter)
        {
            var death = buffs.Death;
            if (death == null)
            {
                DeathCounter.Content = 0;
                DeathDuration.Content = "0s";
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
                AggroDuration.Content = "0s";
            }
            else
            {
                AggroCounter.Content = aggro.Count(entityInformation.BeginTime, entityInformation.EndTime);
                var duration = aggro.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                var interval = TimeSpan.FromTicks(duration);
                AggroDuration.Content = interval.ToString(@"mm\:ss");
            }

            if (skills == null)
            {
                return;
            }

            if (_skillDps == null)
            {
                _skillDps =
                    new SkillsDetail(
                        SkillsAggregate(playerDealt, entityInformation, skills, timedEncounter,
                            Database.Database.Type.Damage), Database.Database.Type.Damage);
                _skillHeal =
                    new SkillsDetail(
                        SkillsAggregate(playerDealt, entityInformation, skills, timedEncounter,
                            Database.Database.Type.Heal), Database.Database.Type.Heal);
                _skillMana =
                    new SkillsDetail(
                        SkillsAggregate(playerDealt, entityInformation, skills, timedEncounter,
                            Database.Database.Type.Mana), Database.Database.Type.Mana);
                _buff = new Buff(playerDealt, buffs, entityInformation);
            }
            else
            {
                _skillDps.Update(SkillsAggregate(playerDealt, entityInformation, skills, timedEncounter,
                    Database.Database.Type.Damage));
                _skillHeal.Update(SkillsAggregate(playerDealt, entityInformation, skills, timedEncounter,
                    Database.Database.Type.Heal));
                _skillMana.Update(SkillsAggregate(playerDealt, entityInformation, skills, timedEncounter,
                    Database.Database.Type.Mana));
                _buff.Update(playerDealt, buffs, entityInformation);
            }
            HealPanel.Content = _skillHeal;
            DpsPanel.Content = _skillDps;
            ManaPanel.Content = _skillMana;
            BuffPanel.Content = _buff;
        }

        private IEnumerable<SkillAggregate> SkillsAggregate(PlayerDealt playerDealt, EntityInformation entityInformation,
            Database.Structures.Skills skillsData, bool timedEncounter, Database.Database.Type type)
        {
            var skills = skillsData.SkillsId(playerDealt.Source.User, entityInformation.Entity, timedEncounter);
            var skillsAggregate = new Dictionary<string, SkillAggregate>();
            foreach (var skill in skills)
            {
                if (skillsData.Type(playerDealt.Source.User.Id, entityInformation.Entity, skill.Id, timedEncounter) !=
                    type)
                {
                    continue;
                }

                if (!skillsAggregate.ContainsKey(skill.ShortName))
                {
                    skillsAggregate.Add(skill.ShortName,
                        new SkillAggregate(skill, skillsData, playerDealt, entityInformation, timedEncounter, type));
                    continue;
                }
                skillsAggregate[skill.ShortName].Add(skill);
            }
            return skillsAggregate.Values;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseSkills();
        }
    }
}