using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Data;
using Lang;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour SkillLog.xaml
    /// </summary>
    public partial class SkillLog
    {
        public SkillLog()
        {
            InitializeComponent();
        }

        private bool _received;
        private long _beginTime;
        public void Update(Database.Structures.Skill skill)
        {
            var skillInfo = SkillResult.GetSkill(skill.Source, skill.Pet, skill.SkillId, skill.HotDot, PacketProcessor.Instance.EntityTracker,
                BasicTeraData.Instance.SkillDatabase, BasicTeraData.Instance.HotDotDatabase, BasicTeraData.Instance.PetSkillDatabase);
            var entity = _received ? skill.Source : skill.Target;
            Brush color = null;
            var fontWeight = FontWeights.Normal;
            if (skill.Critic) { fontWeight = FontWeights.Bold; }
            SkillAmount.FontWeight = fontWeight;
            SkillAmount.ToolTip = skill.Critic ? LP.Critical : LP.White;
            SkillName.Content = skill.SkillId;
            Time.Content = (skill.Time - _beginTime) / TimeSpan.TicksPerSecond + LP.Seconds;
            if (skillInfo != null)
            {
                SkillIcon.ImageSource = BasicTeraData.Instance.Icons.GetImage(skillInfo.IconName);
                SkillName.Content = skillInfo.Name;
            }
            SkillAmount.Content = skill.Amount;
            SkillIconWrapper.ToolTip = skill.SkillId;
            SkillDirection.Content = LP.ResourceManager.GetString(skill.Direction.ToString());
            switch (skill.Direction)
            {
                case HitDirection.Back:
                    SkillDirection.Foreground = ((SolidColorBrush)Application.Current.FindResource("DamageText"));
                    break;
                case HitDirection.Dot:
                    if (skill.Type == Database.Database.Type.Heal) { SkillDirection.Content = LP.Hot; }
                    if (skill.Type == Database.Database.Type.Mana) { SkillDirection.Content = LP.Mot; }
                    break;
                case HitDirection.Front:
                    SkillDirection.Foreground = ((SolidColorBrush)Application.Current.FindResource("ManaText"));
                    break;
                case HitDirection.Side:
                    SkillDirection.Foreground = ((SolidColorBrush)Application.Current.FindResource("BuffText"));
                    break;
            }

            switch (skill.Type)
            {
                case Database.Database.Type.Damage:
                    color = ((SolidColorBrush)Application.Current.FindResource("DamageText"));
                    break;
                case Database.Database.Type.Heal:
                    color = ((SolidColorBrush)Application.Current.FindResource("HealText"));
                    break;
                case Database.Database.Type.Mana:
                    color = ((SolidColorBrush)Application.Current.FindResource("ManaText"));
                    break;
                case Database.Database.Type.Counter:
                    color = ((SolidColorBrush)Application.Current.FindResource("CastText"));
                    SkillDirection.Content = LP.Counter;
                    SkillDirection.Foreground = ((SolidColorBrush)Application.Current.FindResource("CastText"));
                    break;
            }
            SkillAmount.Foreground = color;

            SkillName.ToolTip = skill.Time;
            if (entity is NpcEntity npcEntity)
            {
                SkillTarget.Content = npcEntity.Info.Name + " : " + npcEntity.Info.Area;
            }
            else if (entity is UserEntity) { SkillTarget.Content = ((UserEntity)entity).Name; }
//            SkillPet.Content = skill.Pet == null ? "" : skill.Pet.Name;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }

        private void SkillLog_ContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (_beginTime == 0) return;
            var dc = DataContext as Database.Structures.Skill;
            if (dc==null) return;
            Update(dc);
        }

        private void SkillLog_OnLoaded(object sender, RoutedEventArgs e) {
            var dc = DataContext as Database.Structures.Skill;
            if (dc == null) return;
            var tab = Tag as SkillsLog;
            _received = tab?.Received ?? false;
            _beginTime = tab?.BeginTime ?? 0;
            Update(dc);

        }
    }
}