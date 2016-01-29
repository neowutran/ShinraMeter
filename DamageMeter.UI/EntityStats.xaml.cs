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
using System.Windows.Shapes;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour EntityStats.xaml
    /// </summary>
    public partial class EntityStats : Window
    {

        private MainWindow _parent;

        public EntityStats(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }


        public void Update(Dictionary<Entity, EntityInfo> stats)
        {
            var entity = NetworkController.Instance.Encounter;
            if (entity == null)
            {
                return;
            }
            var statsVolley = stats[entity];
            if (statsVolley.Interval == 0)
            {
                return;
            }
            VolleyOfCurse.Content = "Volley of curse endurance debuff: Total uptime=" + statsVolley.VolleyOfCurse +
                                    "s ; Total fight time=" + statsVolley.Interval + "s ; Uptime percentage:" +
                                    (statsVolley.VolleyOfCurse*100)/statsVolley.Interval+"%";

        }

        private void EntityStats_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }

        private void CloseMeter_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Close");
            _parent.CloseEntityStats();
        }
    }
}
