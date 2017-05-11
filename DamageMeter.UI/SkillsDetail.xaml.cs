using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DamageMeter.UI.Skill;
using DamageMeter.UI.SkillsHeaders;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour SkillsDetail.xaml
    /// </summary>
    public partial class SkillsDetail
    {
        private readonly Database.Database.Type _type;
        private Label _currentSortedLabel;

        private IEnumerable<SkillAggregate> _skills;
        private SortBy _sortBy = SortBy.Amount;
        private SortOrder _sortOrder = SortOrder.Descending;

        public SkillsDetail(IEnumerable<SkillAggregate> skillAggregate, Database.Database.Type type)
        {
            InitializeComponent();
            _skills = skillAggregate;
            _type = type;

            switch (_type)
            {
                case Database.Database.Type.Damage:
                {
                    var header = new SkillsHeaderDps();
                    ContentWidth = header.Width;
                    header.LabelName.MouseRightButtonUp += LabelNameOnMouseRightButtonUp;
                    header.LabelAverageCrit.MouseRightButtonUp += LabelAverageCritOnMouseRightButtonUp;
                    header.LabelAverageHit.MouseRightButtonUp += LabelAverageHitOnMouseRightButtonUp;
                    header.LabelBiggestCrit.MouseRightButtonUp += LabelBiggestCritOnMouseRightButtonUp;
                    header.LabelCritRateDmg.MouseRightButtonUp += LabelCritRateDmgOnMouseRightButtonUp;
                    header.LabelDamagePercentage.MouseRightButtonUp += LabelDamagePercentageOnMouseRightButtonUp;
                    header.LabelNumberHitDmg.MouseRightButtonUp += LabelNumberHitDmgOnMouseRightButtonUp;
                    header.LabelTotalDamage.MouseRightButtonUp += LabelTotalDamageOnMouseRightButtonUp;
                    header.LabelNumberCritDmg.MouseRightButtonUp += LabelNumberCritDmgOnMouseRightButtonUp;
                    header.LabelAverageTotal.MouseRightButtonUp += LabelAverageTotalOnMouseRightButtonUp;
                    _currentSortedLabel = header.LabelTotalDamage;
                    SkillsList.Items.Add(header);
                }
                    break;
                case Database.Database.Type.Heal:
                {
                    var header = new SkillsHeaderHeal();
                    ContentWidth = header.Width;

                    header.LabelName.MouseRightButtonUp += LabelNameOnMouseRightButtonUp;
                    header.LabelCritRateHeal.MouseRightButtonUp += LabelCritRateHealOnMouseRightButtonUp;
                    header.LabelNumberHitHeal.MouseRightButtonUp += LabelNumberHitHealOnMouseRightButtonUp;
                    header.LabelNumberCritHeal.MouseRightButtonUp += LabelNumberCritHealOnMouseRightButtonUp;
                    header.LabelTotalHeal.MouseRightButtonUp += LabelTotalHealOnMouseRightButtonUp;
                    header.LabelAverage.MouseRightButtonUp += LabelAverageOnMouseRightButtonUp;
                    header.LabelAverageCrit.MouseRightButtonUp += LabelAverageHealCritOnMouseRightButtonUp;
                    header.LabelAverageHit.MouseRightButtonUp += LabelAverageHealHitOnMouseRightButtonUp;
                    header.LabelBiggestCrit.MouseRightButtonUp += LabelBiggestHealCritOnMouseRightButtonUp;
                    header.LabelBiggestHit.MouseRightButtonUp += LabelBiggestHealHitOnMouseRightButtonUp;
                    _currentSortedLabel = header.LabelTotalHeal;
                    SkillsList.Items.Add(header);
                }
                    break;
                case Database.Database.Type.Mana:
                {
                    var header = new SkillsHeaderMana();
                    ContentWidth = header.Width;

                    header.LabelName.MouseRightButtonUp += LabelNameOnMouseRightButtonUp;
                    header.LabelNumberHitMana.MouseRightButtonUp += LabelNumberHitManaOnMouseRightButtonUp;
                    header.LabelTotalMana.MouseRightButtonUp += LabelTotalManaOnMouseRightButtonUp;
                    _currentSortedLabel = header.LabelTotalMana;
                    SkillsList.Items.Add(header);
                }
                    break;
                case Database.Database.Type.Counter:
                {
                    var header = new SkillsHeaderCounter();
                    ContentWidth = header.Width;

                    header.LabelName.MouseRightButtonUp += LabelNameOnMouseRightButtonUp;
                    header.LabelNumberHit.MouseRightButtonUp += LabelNumberHitCounterOnMouseRightButtonUp;
                    _currentSortedLabel = header.LabelNumberHit;
                    SkillsList.Items.Add(header);
                }
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            Repaint();
        }

        public double ContentWidth { get; }

        private void LabelBiggestHealHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.BigHit, (Label) sender, SkillsHeaderHeal.BiggestHit);
        }

        private void LabelBiggestHealCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.BigCrit, (Label) sender, SkillsHeaderHeal.BiggestCrit);
        }

        private void LabelAverageHealHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgHit, (Label) sender, SkillsHeaderHeal.AverageHit);
        }

        private void LabelAverageHealCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgCrit, (Label) sender, SkillsHeaderHeal.AverageCrit);
        }

        private void LabelAverageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Avg, (Label) sender, SkillsHeaderHeal.AverageTotal);
        }

        private void LabelAverageTotalOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Avg, (Label) sender, SkillsHeaderDps.AverageTotal);
        }


        private void LabelTotalHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Amount, (Label) sender, SkillsHeaderHeal.Heal);
        }

        private void LabelNumberCritHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberCrits, (Label) sender, SkillsHeaderHeal.CritsHeal);
        }

        private void LabelNumberCritDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberCrits, (Label) sender, SkillsHeaderDps.CritsDmg);
        }

        private void LabelNumberHitManaOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHits, (Label) sender, SkillsHeaderMana.HitsMana);
        }

        private void LabelNumberHitHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHits, (Label) sender, SkillsHeaderHeal.HitsHeal);
        }

        private void LabelNumberHitDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHits, (Label) sender, SkillsHeaderDps.HitsDmg);
        }

        private void LabelCritRateHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.CritRate, (Label) sender, SkillsHeaderHeal.CritRateHeal);
        }

        private void LabelCritRateDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.CritRate, (Label) sender, SkillsHeaderDps.CritRateDmg);
        }

        private void LabelTotalManaOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Amount, (Label) sender, SkillsHeaderMana.Mana);
        }

        private void LabelNumberHitCounterOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHits, (Label)sender, SkillsHeaderCounter.Hits);
        }

        private void Sort()
        {
            switch (_sortOrder)
            {
                case SortOrder.Descending:
                    switch (_sortBy)
                    {
                        case SortBy.Amount:
                            _skills = from skill in _skills orderby skill.Amount() descending select skill;
                            break;
                        case SortBy.AvgCrit:
                            _skills = from skill in _skills orderby skill.AvgCrit() descending select skill;
                            break;
                        case SortBy.AvgHit:
                            _skills = from skill in _skills orderby skill.AvgWhite() descending select skill;
                            break;
                        case SortBy.BigCrit:
                            _skills = from skill in _skills orderby skill.BiggestCrit() descending select skill;
                            break;
                        case SortBy.DamagePercent:
                            _skills = from skill in _skills orderby skill.DamagePercent() descending select skill;
                            break;
                        case SortBy.Name:
                            _skills = from entry in _skills orderby entry.Name descending select entry;
                            return;
                        case SortBy.NumberHits:
                            _skills = from skill in _skills orderby skill.Hits() descending select skill;
                            break;
                        case SortBy.NumberCrits:
                            _skills = from skill in _skills orderby skill.Crits() descending select skill;
                            break;
                        case SortBy.CritRate:
                            _skills = from skill in _skills orderby skill.CritRate() descending select skill;
                            break;
                        case SortBy.Avg:
                            _skills = from skill in _skills orderby skill.Avg() descending select skill;
                            break;
                        case SortBy.BigHit:
                            _skills = from skill in _skills orderby skill.BiggestHit() descending select skill;
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                    break;
                case SortOrder.Ascending:
                    switch (_sortBy)
                    {
                        case SortBy.Amount:
                            _skills = from skill in _skills orderby skill.Amount() select skill;
                            break;
                        case SortBy.AvgCrit:
                            _skills = from skill in _skills orderby skill.AvgCrit() select skill;
                            break;
                        case SortBy.AvgHit:
                            _skills = from skill in _skills orderby skill.AvgWhite() select skill;
                            break;
                        case SortBy.BigCrit:
                            _skills = from skill in _skills orderby skill.BiggestCrit() select skill;
                            break;
                        case SortBy.DamagePercent:
                            _skills = from skill in _skills orderby skill.DamagePercent() select skill;
                            break;
                        case SortBy.Name:
                            _skills = from entry in _skills orderby entry.Name select entry;
                            return;
                        case SortBy.NumberHits:
                            _skills = from skill in _skills orderby skill.Hits() select skill;
                            break;
                        case SortBy.NumberCrits:
                            _skills = from skill in _skills orderby skill.Crits() select skill;
                            break;
                        case SortBy.CritRate:
                            _skills = from skill in _skills orderby skill.CritRate() select skill;
                            break;
                        case SortBy.Avg:
                            _skills = from skill in _skills orderby skill.Avg() select skill;
                            break;
                        case SortBy.BigHit:
                            _skills = from skill in _skills orderby skill.BiggestHit() select skill;
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeSort(SortBy sortby, Label sender, string labelstring)
        {
            if (_sortBy == sortby) { _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending; }
            else
            {
                _currentSortedLabel.Content = ((string) _currentSortedLabel.Content).Remove(((string) _currentSortedLabel.Content).Length - 1);
                _sortBy = sortby;
                _sortOrder = SortOrder.Descending;
                _currentSortedLabel = sender;
            }

            if (_sortOrder == SortOrder.Ascending) { sender.Content = labelstring + "↑"; }
            else { sender.Content = labelstring + "↓"; }
            Repaint();
        }

        private void LabelTotalDamageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Amount, (Label) sender, SkillsHeaderDps.TotalDamage);
        }

        private void LabelDamagePercentageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DamagePercent, (Label) sender, SkillsHeaderDps.DamagePercentage);
        }

        private void LabelBiggestCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.BigCrit, (Label) sender, SkillsHeaderDps.BiggestCrit);
        }

        private void LabelAverageHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgHit, (Label) sender, SkillsHeaderDps.AverageHit);
        }

        private void LabelAverageCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.AvgCrit, (Label) sender, SkillsHeaderDps.AverageCrit);
        }

        private void LabelNameOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Name, (Label) sender, SkillsHeaderDps.SkillName);
        }

        private void Skills_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try { w?.DragMove(); }
            catch { Console.WriteLine(@"Exception move"); }
        }

        private List<ISkill> Clear()
        {
            var skills = new List<ISkill>();
            var header = SkillsList.Items[0];
            for (var i = 1; i < SkillsList.Items.Count; i++) { skills.Add((ISkill) SkillsList.Items[i]); }
            SkillsList.Items.Clear();
            SkillsList.Items.Add(header);
            return skills;
        }

        public void Repaint()
        {
            var oldSkills = Clear();
            Sort();
            foreach (var skill in _skills)
            {
                var updated = -1;
                for (var i = 0; i < oldSkills.Count; i++)
                {
                    if (skill.Name != oldSkills[i].SkillNameIdent()) { continue; }
                    oldSkills[i].Update(skill);
                    SkillsList.Items.Add(oldSkills[i]);
                    updated = i;
                    break;
                }

                if (updated != -1)
                {
                    oldSkills.RemoveAt(updated);
                    continue;
                }
                switch (_type)
                {
                    case Database.Database.Type.Damage:
                        SkillsList.Items.Add(new SkillDps(skill));
                        break;
                    case Database.Database.Type.Heal:
                        SkillsList.Items.Add(new SkillHeal(skill));
                        break;
                    case Database.Database.Type.Mana:
                        SkillsList.Items.Add(new SkillMana(skill));
                        break;
                    case Database.Database.Type.Counter:
                        SkillsList.Items.Add(new SkillCounter(skill));
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SkillsList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }


        private enum SortBy
        {
            Amount = 1,
            Name = 2,
            AvgCrit = 3,
            Avg = 4,
            BigCrit = 5,
            DamagePercent = 6,
            NumberHits = 7,
            NumberCrits = 8,
            AvgHit = 9,
            BigHit = 10,
            CritRate = 11
        }

        private enum SortOrder
        {
            Ascending = 1,
            Descending = 2
        }
    }
}