using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour SkillsLog.xaml
    /// </summary>
    public partial class SkillsLog
    {
        private readonly bool _initialized;

        public bool Received { get; }
        public ICollectionView View { get; }
        private readonly ObservableCollection<Database.Structures.Skill> _skills;
        public long BeginTime { get; }

        public SkillsLog(IEnumerable<Database.Structures.Skill> skills, bool received)
        {
            InitializeComponent();
            if (skills == null) { return; }
            var enumerable = new ObservableCollection<Database.Structures.Skill>((skills as Database.Structures.Skill[] ?? skills.ToArray()).OrderByDescending(x=>x.Time));
            if (!enumerable.Any()) { return; }
            Received = received;
            _skills = enumerable;
            View = CollectionViewSource.GetDefaultView(_skills);
            BeginTime = enumerable.Min(x => x.Time);
            View.Filter = x => {
                var y = x as Database.Structures.Skill;
                if (y == null) return false;
                if ((bool)Damage.Status && y.Type == Database.Database.Type.Damage) return true;
                if ((bool)Heal.Status && y.Type == Database.Database.Type.Heal) return true;
                if ((bool)Mana.Status && y.Type == Database.Database.Type.Mana) return true;
                if ((bool)Casts.Status && y.Type == Database.Database.Type.Counter) return true;
                return false;
            };
            DataContext = this;
            _initialized = true;
        }

        private void ValueChanged(object sender, RoutedEventArgs e)
        {
            if (!_initialized) { return; }
            (Skills.ItemsSource as ICollectionView)?.Refresh();
        }

        private void Skills_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var s = (ScrollViewer)sender;
            s.ScrollToVerticalOffset(s.VerticalOffset - (e.Delta>0?10:-10));
            e.Handled = true;
        }
    }
}