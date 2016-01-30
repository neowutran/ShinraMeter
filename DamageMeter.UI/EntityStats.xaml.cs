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

        private readonly MainWindow _parent;

        public EntityStats(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }


        public void Update(Dictionary<Entity, EntityInfo> stats)
        {
            var entity = NetworkController.Instance.Encounter;
            EnduranceAbnormality.Items.Clear();
            if (entity == null)
            {
                return;
            }
            var statsAbnormalities = stats[entity];
            if (statsAbnormalities.Interval == 0)
            {
                return;
            }


            foreach (var statsAbnormality in statsAbnormalities.AbnormalityTime)
            {

                var abnormality = new Label
                {
                    Content =
                        "" + statsAbnormality.Key.Name + "=> Total uptime: " + statsAbnormality.Value +
                        "s ; Total fight time: " + statsAbnormalities.Interval + "s ; Uptime percentage: " +
                        (statsAbnormality.Value*100)/statsAbnormalities.Interval+"%",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 14,
                    MinHeight = 29,
                    Height = 29,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                EnduranceAbnormality.Items.Add(abnormality);
            }
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
            _parent.CloseEntityStats();
        }
    }
}
