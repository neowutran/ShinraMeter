using System;
using System.Windows;
using DamageMeter.Database.Structures;
using Data;
using Lang;
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
        private SkillsLog _skillDealtLog;
        private SkillsDetail _skillDps;
        private SkillsDetail _skillHeal;
        private SkillsDetail _skillMana;
        private SkillsDetail _skillCounter;
        private SkillsLog _skillReceivedLog;

        private Database.Structures.Skills _skills;


        public Skills(PlayerStats parent, PlayerDamageDealt playerDamageDealt, EntityInformation entityInformation, Database.Structures.Skills skills,
            PlayerAbnormals buffs, bool timedEncounter)
        {
            Owner = GetWindow(parent);
            InitializeComponent();
            _parent = parent;
            ClassImage.Source = ClassIcons.Instance.GetImage(playerDamageDealt.Source.Class).Source; //BasicTeraData.Instance.ImageDatabase.Close.Source;
            Update(playerDamageDealt, entityInformation, skills, buffs, timedEncounter);
        }

        public void Update(PlayerDamageDealt playerDamageDealt, EntityInformation entityInformation, Database.Structures.Skills skills, PlayerAbnormals buffs,
            bool timedEncounter)
        {
            if (_skills == null || skills != null)
            {
                _skills = skills;
                var death = buffs.Death;
                if (death == null)
                {
                    DeathCounter.Text = "0";
                    DeathDuration.Text = "0" + LP.Seconds;
                }
                else
                {
                    DeathCounter.Text = death.Count(entityInformation.BeginTime, entityInformation.EndTime).ToString();
                    var duration = death.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                    var interval = TimeSpan.FromTicks(duration);
                    DeathDuration.Text = interval.ToString(@"mm\:ss");
                }
                var aggro = buffs.Aggro(entityInformation.Entity);
                if (aggro == null)
                {
                    AggroCounter.Text = "0";
                    AggroDuration.Text = "0" + LP.Seconds;
                }
                else
                {
                    AggroCounter.Text = aggro.Count(entityInformation.BeginTime, entityInformation.EndTime).ToString();
                    var duration = aggro.Duration(entityInformation.BeginTime, entityInformation.EndTime);
                    var interval = TimeSpan.FromTicks(duration);
                    AggroDuration.Text = interval.ToString(@"mm\:ss");
                }

                //return;

                //var tabItem = (TabItem) TabControl.SelectedItem;
                //if (tabItem == null)
                //{
                //    TabControl.SelectedIndex = 0;
                //    tabItem = (TabItem) TabControl.SelectedItem;
                //}
                //switch (tabItem.Name)
                //{
                //    case "DpsPanel":
                //        if (_skillDps == null)
                //        {
                _skillDps = new SkillsDetail(
                    SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Damage),
                    Database.Database.Type.Damage);
                //}
                DpsPanel.Content = _skillDps;
                //    return;
                //case "HealPanel":
                //    if (_skillHeal == null)
                //    {
                _skillHeal = new SkillsDetail(
                    SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Heal),
                    Database.Database.Type.Heal);
                //    }
                HealPanel.Content = _skillHeal;
                //    return;
                //case "ManaPanel":
                //    if (_skillMana == null)
                //    {
                _skillMana = new SkillsDetail(
                    SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Mana),
                    Database.Database.Type.Mana);
                //    }
                ManaPanel.Content = _skillMana;
                //    return;
                //case "BuffPanel":
                //    if (_buff == null)
                //    {
                _buff = new Buff(playerDamageDealt, buffs);
                //    }
                BuffPanel.Content = _buff;
                //    return;

                _skillCounter = new SkillsDetail(
                    SkillAggregate.GetAggregate(playerDamageDealt, entityInformation.Entity, _skills, timedEncounter, Database.Database.Type.Counter),
                    Database.Database.Type.Counter);
                CounterPanel.Content = _skillCounter;

                //case "SkillsDealtPanel":
                //    if (_skillDealtLog == null)
                //    {
                _skillDealtLog = new SkillsLog(_skills?.GetSkillsDealt(playerDamageDealt.Source.User, entityInformation.Entity, timedEncounter), false);
                //    }
                SkillsDealtPanel.Content = _skillDealtLog;
                //    return;
                //case "SkillsReceivedPanel":
                //    if (_skillReceivedLog == null)
                //    {
                _skillReceivedLog = new SkillsLog(_skills?.GetSkillsReceived(playerDamageDealt.Source.User, timedEncounter), true);
                //    }
                SkillsReceivedPanel.Content = _skillReceivedLog;
                //return;
            }
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseSkills();
        }

        private void ClickThrouWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            DeathIcon.Source = BasicTeraData.Instance.ImageDatabase.Skull.Source;
            DeathTimeIcon.Source = BasicTeraData.Instance.ImageDatabase.SkullTime.Source;
            AggroIcon.Source = BasicTeraData.Instance.ImageDatabase.BossGage.Source;
            AggroTimeIcon.Source = BasicTeraData.Instance.ImageDatabase.AggroTime.Source;
            
        }
    }
}