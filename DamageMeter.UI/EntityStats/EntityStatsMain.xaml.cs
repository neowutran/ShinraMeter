using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EntityStats.xaml
    /// </summary>
    public partial class EntityStatsMain
    {
        private readonly MainWindow _parent;

        public EntityStatsMain(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            _header = new EnduranceDebuffHeader(this);
        }

        private readonly EnduranceDebuffHeader _header;
        private readonly List<EnduranceDebuff> _enduranceDebuffsList = new List<EnduranceDebuff>(); 


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

            EnduranceAbnormality.Items.Add(_header);

            for(var i = 0; i < statsAbnormalities.AbnormalityTime.Count; i++)
            {
                EnduranceDebuff abnormality;
                if (_enduranceDebuffsList.Count > i)
                {
                    abnormality = _enduranceDebuffsList[i];
                    abnormality.Update(statsAbnormalities.AbnormalityTime.Keys.ElementAt(i), statsAbnormalities.AbnormalityTime.Values.ElementAt(i), statsAbnormalities);
                }
                else
                {
                    abnormality = new EnduranceDebuff(this);
                    abnormality.Update(statsAbnormalities.AbnormalityTime.Keys.ElementAt(i), statsAbnormalities.AbnormalityTime.Values.ElementAt(i), statsAbnormalities);
                    _enduranceDebuffsList.Add(abnormality);
                }

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