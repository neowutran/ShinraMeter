using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour SkillsLog.xaml
    /// </summary>
    public partial class SkillsLog
    {

        private IEnumerable<Database.Structures.Skill> _skills;
        private bool _received;
        private bool _initialized;
        public SkillsLog(IEnumerable<Database.Structures.Skill> skills, bool received)
        {
            InitializeComponent();
            //ContentWidth = 900;
            if (skills==null)return;
            if (skills.Count() == 0) return;
            _received = received;
            _skills = skills;
            _initialized = true;
            Display();
        }

        private void Display()
        {
            Database.Database.Type? typeDamage = null;
            Database.Database.Type? typeHeal = null;
            Database.Database.Type? typeMana = null;
            Database.Database.Type? lastType = null;

            if ((bool)Damage.IsChecked)
            {
                typeDamage = Database.Database.Type.Damage;
                lastType = typeDamage;
            }
            if ((bool)Heal.IsChecked)
            {
                typeHeal = Database.Database.Type.Heal;
                lastType = typeHeal;
            }
            if ((bool)Mana.IsChecked)
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

                if (typeDamage == null)
                {
                    typeDamage = lastType;
                }
                if (typeHeal == null)
                {
                    typeHeal = lastType;
                }
                if (typeMana == null)
                {
                    typeMana = lastType;
                }
            }

            Skills.Items.Clear();
            var beginTime = _skills.Min(x => x.Time);
            foreach (var skill in _skills.Where(x => x.Type == typeDamage || x.Type == typeHeal || x.Type == typeMana ).OrderByDescending(x => x.Time))
            {
                var log = new SkillLog();
                log.Update(skill, _received, beginTime);
                Skills.Items.Add(log);
            }
        }

        public double ContentWidth { get; private set; }

        private void ValueChanged(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            Display();
        }

        private void Skills_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
