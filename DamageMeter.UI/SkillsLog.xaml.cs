using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour SkillsLog.xaml
    /// </summary>
    public partial class SkillsLog
    {
        private readonly bool _initialized;
        private readonly bool _received;

        private readonly IEnumerable<Database.Structures.Skill> _skills;

        public SkillsLog(IEnumerable<Database.Structures.Skill> skills, bool received)
        {
            InitializeComponent();
            //ContentWidth = 900;
            if (skills == null) { return; }
            var enumerable = skills as Database.Structures.Skill[] ?? skills.ToArray();
            if (!enumerable.Any()) { return; }
            _received = received;
            _skills = enumerable;
            _initialized = true;
            Display();
        }

        public double ContentWidth { get; private set; }

        private void Display()
        {
            Database.Database.Type? typeDamage = null;
            Database.Database.Type? typeHeal = null;
            Database.Database.Type? typeMana = null;
            Database.Database.Type? lastType = null;

            if ((bool) Damage.IsChecked)
            {
                typeDamage = Database.Database.Type.Damage;
                lastType = typeDamage;
            }
            if ((bool) Heal.IsChecked)
            {
                typeHeal = Database.Database.Type.Heal;
                lastType = typeHeal;
            }
            if ((bool) Mana.IsChecked)
            {
                typeMana = Database.Database.Type.Mana;
                lastType = typeMana;
            }

            if (lastType == null)
            {
                typeMana = Database.Database.Type.Mana;
                typeHeal = Database.Database.Type.Heal;
                typeDamage = Database.Database.Type.Damage;
            }
            else
            {
                if (typeDamage == null) { typeDamage = lastType; }
                if (typeHeal == null) { typeHeal = lastType; }
                if (typeMana == null) { typeMana = lastType; }
            }

            Skills.Items.Clear();
            var beginTime = _skills.Min(x => x.Time);
            foreach (var skill in _skills.Where(x => x.Type == typeDamage || x.Type == typeHeal || x.Type == typeMana).OrderByDescending(x => x.Time))
            {
                var log = new SkillLog();
                log.Update(skill, _received, beginTime);
                Skills.Items.Add(log);
            }
        }

        private void ValueChanged(object sender, RoutedEventArgs e)
        {
            if (!_initialized) { return; }
            Display();
        }

        private void Skills_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}