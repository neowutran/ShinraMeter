using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DamageMeter.UI.Skill;
using DamageMeter.UI.SkillsHeaders;
using DamageMeter.Database.Structures;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour SkillsDetail.xaml
    /// </summary>
    public partial class SkillsDetail : UserControl
    {
        private Label _currentSortedLabel;
        private Database.Structures.Skills _skills;
        private SortBy _sortBy = SortBy.Amount;
        private SortOrder _sortOrder = SortOrder.Descending;

        private EntityInformation _entityInformation;
        private IEnumerable<SkillAggregate> _skillsId;
        private PlayerDealt _playerDealt;
        private bool _timedEncounter;

        public SkillsDetail(Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, Database.Database.Type type, bool timedEncounter)
        {
            InitializeComponent();
            _entityInformation = entityInformation;
            _playerDealt = playerDealt;
            _timedEncounter = timedEncounter;
            TypeSkill = type;
            switch (TypeSkill)
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
                default:
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
            }


            _skills = skills;
            _skillsId = SkillsAggregate(skills.SkillsId(_playerDealt.Source.User, _entityInformation.Entity.Id, timedEncounter), _skills);
            Repaint();
        }

        private IEnumerable<SkillAggregate> SkillsAggregate(IEnumerable<Tera.Game.Skill> skills, Database.Structures.Skills skillsData)
        {
            Dictionary<string, SkillAggregate> skillsAggregate = new Dictionary<string, SkillAggregate>();
            foreach(Tera.Game.Skill skill  in skills)
            {

                if (_skills.Type(_playerDealt.Source.User.Id, _entityInformation.Entity.Id, skill.Id , _timedEncounter) != TypeSkill)
                {
                    continue;
                }

                if (!skillsAggregate.ContainsKey(skill.Name))
                {
                    skillsAggregate.Add(skill.Name, new SkillAggregate(skill, skillsData, _playerDealt, _entityInformation, _timedEncounter));
                    continue;
                }
                skillsAggregate[skill.Name].Add(skill);
                
            }
            return skillsAggregate.Values.ToList();
        }

        public Database.Database.Type TypeSkill { get; }

        public double ContentWidth { get; private set; }

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


        private void Sort()
        {
            switch (_sortOrder)
            {
               
                case SortOrder.Descending:
                    switch (_sortBy)
                    {
                        case SortBy.Amount:
                            _skillsId = from skill in _skillsId orderby skill.Amount() descending select skill;
                            break;
                        case SortBy.AvgCrit:
                            _skillsId = from skill in _skillsId orderby skill.AvgCrit() descending select skill;
                            break;
                        case SortBy.AvgHit:
                            _skillsId = from skill in _skillsId orderby skill.AvgHit() descending select skill;
                            break;
                        case SortBy.BigCrit:
                            _skillsId = from skill in _skillsId orderby skill.BiggestCrit() descending select skill;
                            break;
                        case SortBy.DamagePercent:
                            _skillsId = from skill in _skillsId orderby skill.DamagePercent() descending select skill;
                            break;
                        case SortBy.Name:
                            _skillsId = from entry in _skillsId orderby entry.Name descending select entry;
                            return;
                        case SortBy.NumberHits:
                            _skillsId = from skill in _skillsId orderby skill.Hits() descending select skill;
                            break;
                        case SortBy.NumberCrits:
                            _skillsId = from skill in _skillsId orderby skill.Crits() descending select skill;
                            break;
                        case SortBy.CritRate:
                            _skillsId = from skill in _skillsId orderby skill.CritRate() descending select skill;
                            break;
                        case SortBy.Avg:
                            _skillsId = from skill in _skillsId orderby skill.Avg() descending select skill;
                            break;
                        case SortBy.BigHit:
                            _skillsId = from skill in _skillsId orderby skill.BiggestHit() descending select skill;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case SortOrder.Ascending:
                    switch (_sortBy)
                    {
                        case SortBy.Amount:
                            _skillsId = from skill in _skillsId orderby skill.Amount() ascending select skill;
                            break;
                        case SortBy.AvgCrit:
                            _skillsId = from skill in _skillsId orderby skill.AvgCrit() ascending select skill;
                            break;
                        case SortBy.AvgHit:
                            _skillsId = from skill in _skillsId orderby skill.AvgHit() ascending select skill;
                            break;
                        case SortBy.BigCrit:
                            _skillsId = from skill in _skillsId orderby skill.BiggestCrit() ascending select skill;
                            break;
                        case SortBy.DamagePercent:
                            _skillsId = from skill in _skillsId orderby skill.DamagePercent() ascending select skill;
                            break;
                        case SortBy.Name:
                            _skillsId = from entry in _skillsId orderby entry.Name ascending select entry;
                            return;
                        case SortBy.NumberHits:
                            _skillsId = from skill in _skillsId orderby skill.Hits() ascending select skill;
                            break;
                        case SortBy.NumberCrits:
                            _skillsId = from skill in _skillsId orderby skill.Crits() ascending select skill;
                            break;
                        case SortBy.CritRate:
                            _skillsId = from skill in _skillsId orderby skill.CritRate() ascending select skill;
                            break;
                        case SortBy.Avg:
                            _skillsId = from skill in _skillsId orderby skill.Avg() ascending select skill;
                            break;
                        case SortBy.BigHit:
                            _skillsId = from skill in _skillsId orderby skill.BiggestHit() ascending select skill;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
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
            foreach (var skill in _skillsId)
            {
                var updated = -1;
                for (var i = 0; i < oldSkills.Count; i++)
                {
                    if (skill.Name != oldSkills[i].SkillNameIdent()) continue;
                    oldSkills[i].Update(skill, _skills, _playerDealt, _entityInformation, _timedEncounter);
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
                    case Database.Database.Type.Damage:
                        SkillsList.Items.Add(new SkillDps(skill, _skills, _playerDealt, _entityInformation, _timedEncounter));
                        break;
                    case Database.Database.Type.Heal:
                        SkillsList.Items.Add(new SkillHeal(skill, _skills, _playerDealt, _entityInformation, _timedEncounter));
                        break;
                    case Database.Database.Type.Mana:
                    default:
                        SkillsList.Items.Add(new SkillMana(skill, _skills, _playerDealt, _entityInformation, _timedEncounter));
                        break;
                }
            }
        }


        public void Update(Database.Structures.Skills skills, EntityInformation entityInformation, PlayerDealt playerDealt, bool timedEncounter)
        {
            _skills = skills;
            _entityInformation = entityInformation;
            _playerDealt = playerDealt;
            _timedEncounter = timedEncounter;
            Repaint();
        }

        private enum SortBy
        {
            Amount,
            Name,
            AvgCrit,
            Avg,
            BigCrit,
            DamagePercent,
            NumberHits,
            NumberCrits,
            AvgHit,
            BigHit,
            CritRate
        }

        private enum SortOrder
        {
            Ascending,
            Descending
        }
    }
}