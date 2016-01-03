using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DamageMeter.Skills.Skill;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour SkillsDetail.xaml
    /// </summary>
    public partial class SkillsDetail : UserControl
    {
        public enum Type
        {
            Heal,
            Dps,
            Mana
        }

        private Label _currentSortedLabel;
        private IEnumerable<KeyValuePair<Skill, SkillStats>> _skills;
        private SortBy _sortBy = SortBy.Damage;
        private SortOrder _sortOrder = SortOrder.Descending;

        public SkillsDetail(Dictionary<Skill, SkillStats> skills, Type type)
        {
            InitializeComponent();
            TypeSkill = type;
            switch (TypeSkill)
            {
                case Type.Dps:
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
                case Type.Heal:
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
                default:
                case Type.Mana:

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
            }


            _skills = skills;
            Repaint();
        }

        public Type TypeSkill { get; }

        public double ContentWidth { get; private set; }

        private void LabelBiggestHealHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.HBigHit, (Label) sender, SkillsHeaderHeal.BiggestHit);
        }

        private void LabelBiggestHealCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.HBigCrit, (Label) sender, SkillsHeaderHeal.BiggestCrit);
        }

        private void LabelAverageHealHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.HAvgHit, (Label) sender, SkillsHeaderHeal.AverageHit);
        }

        private void LabelAverageHealCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.HAvgCrit, (Label) sender, SkillsHeaderHeal.AverageCrit);
        }

        private void LabelAverageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.HAvg, (Label) sender, SkillsHeaderHeal.AverageTotal);
        }

        private void LabelAverageTotalOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DAvg, (Label) sender, SkillsHeaderDps.AverageTotal);
        }


        private void LabelTotalHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Heal, (Label) sender, SkillsHeaderHeal.Heal);
        }

        private void LabelNumberCritHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberCritsHeal, (Label) sender, SkillsHeaderHeal.CritsHeal);
        }

        private void LabelNumberCritDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberCritsDmg, (Label) sender, SkillsHeaderDps.CritsDmg);
        }

        private void LabelNumberHitManaOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHitsMana, (Label) sender, SkillsHeaderMana.HitsMana);
        }

        private void LabelNumberHitHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHitsHeal, (Label) sender, SkillsHeaderHeal.HitsHeal);
        }

        private void LabelNumberHitDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.NumberHitsDmg, (Label) sender, SkillsHeaderDps.HitsDmg);
        }

        private void LabelCritRateHealOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.CritRateHeal, (Label) sender, SkillsHeaderHeal.CritRateHeal);
        }

        private void LabelCritRateDmgOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.CritRateDmg, (Label) sender, SkillsHeaderDps.CritRateDmg);
        }

        private void LabelTotalManaOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Mana, (Label) sender, SkillsHeaderMana.Mana);
        }


        private IEnumerable<KeyValuePair<Skill, SkillStats>> Sort()
        {
            switch (_sortOrder)
            {
                case SortOrder.Descending:
                    switch (_sortBy)
                    {
                        case SortBy.Damage:
                            return from entry in _skills orderby entry.Value.Damage descending select entry;
                        case SortBy.DAvgCrit:
                            return from entry in _skills orderby entry.Value.DmgAverageCrit descending select entry;
                        case SortBy.DAvgHit:
                            return from entry in _skills orderby entry.Value.DmgAverageHit descending select entry;
                        case SortBy.DBigCrit:
                            return from entry in _skills orderby entry.Value.DmgBiggestCrit descending select entry;
                        case SortBy.DamagePercent:
                            return from entry in _skills orderby entry.Value.DamagePercentage descending select entry;
                        case SortBy.Name:
                            return from entry in _skills orderby entry.Key.SkillName descending select entry;
                        case SortBy.Mana:
                            return from entry in _skills orderby entry.Value.Mana descending select entry;
                        case SortBy.Heal:
                            return from entry in _skills orderby entry.Value.Heal descending select entry;
                        case SortBy.NumberHitsDmg:
                            return from entry in _skills orderby entry.Value.HitsDmg descending select entry;
                        case SortBy.NumberHitsHeal:
                            return from entry in _skills orderby entry.Value.HitsHeal descending select entry;
                        case SortBy.NumberHitsMana:
                            return from entry in _skills orderby entry.Value.HitsMana descending select entry;
                        case SortBy.NumberCritsDmg:
                            return from entry in _skills orderby entry.Value.CritsDmg descending select entry;
                        case SortBy.NumberCritsHeal:
                            return from entry in _skills orderby entry.Value.CritsHeal descending select entry;
                        case SortBy.CritRateDmg:
                            return from entry in _skills orderby entry.Value.CritRateDmg descending select entry;
                        case SortBy.CritRateHeal:
                            return from entry in _skills orderby entry.Value.CritRateHeal descending select entry;
                        case SortBy.DAvg:
                            return from entry in _skills orderby entry.Value.DmgAverageTotal descending select entry;
                        case SortBy.HAvg:
                            return from entry in _skills orderby entry.Value.HealAverageTotal descending select entry;

                        case SortBy.HAvgCrit:
                            return from entry in _skills orderby entry.Value.HealAverageCrit descending select entry;

                        case SortBy.HAvgHit:
                            return from entry in _skills orderby entry.Value.HealAverageHit descending select entry;

                        case SortBy.HBigCrit:
                            return from entry in _skills orderby entry.Value.HealBiggestCrit descending select entry;

                        case SortBy.HBigHit:
                            return from entry in _skills orderby entry.Value.HealBiggestHit descending select entry;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case SortOrder.Ascending:
                    switch (_sortBy)
                    {
                        case SortBy.Damage:
                            return from entry in _skills orderby entry.Value.Damage ascending select entry;
                        case SortBy.DAvgCrit:
                            return from entry in _skills orderby entry.Value.DmgAverageCrit ascending select entry;
                        case SortBy.DAvgHit:
                            return from entry in _skills orderby entry.Value.DmgAverageHit ascending select entry;
                        case SortBy.DBigCrit:
                            return from entry in _skills orderby entry.Value.DmgBiggestCrit ascending select entry;
                        case SortBy.DamagePercent:
                            return from entry in _skills orderby entry.Value.DamagePercentage ascending select entry;
                        case SortBy.Name:
                            return from entry in _skills orderby entry.Key.SkillName ascending select entry;
                        case SortBy.Mana:
                            return from entry in _skills orderby entry.Value.Mana ascending select entry;
                        case SortBy.Heal:
                            return from entry in _skills orderby entry.Value.Heal ascending select entry;
                        case SortBy.NumberHitsDmg:
                            return from entry in _skills orderby entry.Value.HitsDmg ascending select entry;
                        case SortBy.NumberHitsHeal:
                            return from entry in _skills orderby entry.Value.HitsHeal ascending select entry;
                        case SortBy.NumberHitsMana:
                            return from entry in _skills orderby entry.Value.HitsMana ascending select entry;
                        case SortBy.NumberCritsDmg:
                            return from entry in _skills orderby entry.Value.CritsDmg ascending select entry;
                        case SortBy.NumberCritsHeal:
                            return from entry in _skills orderby entry.Value.CritsHeal ascending select entry;
                        case SortBy.CritRateDmg:
                            return from entry in _skills orderby entry.Value.CritRateDmg ascending select entry;
                        case SortBy.CritRateHeal:
                            return from entry in _skills orderby entry.Value.CritRateHeal ascending select entry;
                        case SortBy.DAvg:
                            return from entry in _skills orderby entry.Value.DmgAverageTotal ascending select entry;
                        case SortBy.HAvg:
                            return from entry in _skills orderby entry.Value.HealAverageTotal ascending select entry;

                        case SortBy.HAvgCrit:
                            return from entry in _skills orderby entry.Value.HealAverageCrit ascending select entry;

                        case SortBy.HAvgHit:
                            return from entry in _skills orderby entry.Value.HealAverageHit ascending select entry;

                        case SortBy.HBigCrit:
                            return from entry in _skills orderby entry.Value.HealBiggestCrit ascending select entry;

                        case SortBy.HBigHit:
                            return from entry in _skills orderby entry.Value.HealBiggestHit ascending select entry;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeSort(SortBy sortby, Label sender, string labelstring)
        {
            if (_sortBy == sortby)
            {
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _currentSortedLabel.Content =
                    ((string) _currentSortedLabel.Content).Remove(((string) _currentSortedLabel.Content).Length - 1);
                _sortBy = sortby;
                _sortOrder = SortOrder.Descending;
                _currentSortedLabel = sender;
            }

            if (_sortOrder == SortOrder.Ascending)
            {
                sender.Content = labelstring + "↑";
            }
            else
            {
                sender.Content = labelstring + "↓";
            }
            Repaint();
        }

        private void LabelTotalDamageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Damage, (Label) sender, SkillsHeaderDps.TotalDamage);
        }

        private void LabelDamagePercentageOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DamagePercent, (Label) sender, SkillsHeaderDps.DamagePercentage);
        }

        private void LabelBiggestCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DBigCrit, (Label) sender, SkillsHeaderDps.BiggestCrit);
        }

        private void LabelAverageHitOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DAvgHit, (Label) sender, SkillsHeaderDps.AverageHit);
        }

        private void LabelAverageCritOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.DAvgCrit, (Label) sender, SkillsHeaderDps.AverageCrit);
        }

        private void LabelNameOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ChangeSort(SortBy.Name, (Label) sender, SkillsHeaderDps.SkillName);
        }

        private void Skills_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private List<ISkill> Clear()
        {
            var skills = new List<ISkill>();
            var header = SkillsList.Items[0];
            for (var i = 1; i < SkillsList.Items.Count; i++)
            {
                skills.Add((ISkill) SkillsList.Items[i]);
            }
            SkillsList.Items.Clear();
            SkillsList.Items.Add(header);
            return skills;
        }

        public void Repaint()
        {
            var oldSkills = Clear();
            var sortedDict = Sort();
            foreach (var skill in sortedDict)
            {
                var updated = -1;
                for (var i = 0; i < oldSkills.Count; i++)
                {
                    if (!skill.Key.SkillName.Equals(oldSkills[i].SkillNameIdent())) continue;
                    oldSkills[i].Update(skill.Key, skill.Value);
                    SkillsList.Items.Add(oldSkills[i]);
                    updated = i;
                    break;
                }

                if (updated != -1)
                {
                    oldSkills.RemoveAt(updated);
                    continue;
                }
                switch (TypeSkill)
                {
                    case Type.Dps:
                        if (skill.Value.Damage == 0) continue;
                        SkillsList.Items.Add(new SkillDps(skill.Key, skill.Value));
                        break;
                    case Type.Heal:
                        if (skill.Value.Heal == 0) continue;
                        SkillsList.Items.Add(new SkillHeal(skill.Key, skill.Value));
                        break;
                    case Type.Mana:
                    default:
                        if (skill.Value.Mana == 0) continue;
                        SkillsList.Items.Add(new SkillMana(skill.Key, skill.Value));
                        break;
                }
            }
        }


        public void Update(Dictionary<Skill, SkillStats> skills)
        {
            _skills = skills;
            Repaint();
        }

        private enum SortBy
        {
            Damage,
            Name,
            Heal,
            Mana,
            DAvgCrit,
            DAvg,
            DBigCrit,
            HAvgCrit,
            HAvg,
            HBigCrit,
            DamagePercent,
            NumberHitsDmg,
            NumberHitsHeal,
            NumberHitsMana,
            NumberCritsDmg,
            NumberCritsHeal,
            DAvgHit,
            HAvgHit,
            HBigHit,
            CritRateDmg,
            CritRateHeal
        };

        private enum SortOrder
        {
            Ascending,
            Descending
        };
    }
}