using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Data;
using Tera.Game;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EntityStats.xaml
    /// </summary>
    public partial class EntityStatsMain
    {
        private readonly List<EnduranceDebuff> _enduranceDebuffsList = new List<EnduranceDebuff>();

        private readonly EnduranceDebuffHeader _header;
        private readonly MainWindow _parent;

        public EntityStatsMain(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            _header = new EnduranceDebuffHeader();

        }

        public void SetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }

        public void UnsetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExVisible(hwnd);
        }

        public void Update(Dictionary<Entity, EntityInfo> stats, AbnormalityStorage abnormals, Entity currentBoss)
        {
            var entity = currentBoss;
            EnduranceAbnormality.Items.Clear();
            if (entity == null)
            {
                return;
            }
            var statsAbnormalities = stats[entity];
            if (statsAbnormalities.Interval == 0 || entity.NpcE==null)
            {
                return;
            }

            EnduranceAbnormality.Items.Add(_header);

            var count = 0;
            foreach (var abnormality in abnormals.Clone(entity.NpcE))
            {
                EnduranceDebuff abnormalityUi;
                if (_enduranceDebuffsList.Count > count)
                {
                    abnormalityUi = _enduranceDebuffsList[count];
                }
                else
                {
                    abnormalityUi = new EnduranceDebuff();
                    _enduranceDebuffsList.Add(abnormalityUi);
                }

                abnormalityUi.Update(abnormality.Key, abnormality.Value,
                    statsAbnormalities.FirstHit/TimeSpan.TicksPerSecond,
                    statsAbnormalities.LastHit/TimeSpan.TicksPerSecond);
                EnduranceAbnormality.Items.Add(abnormalityUi);
                count++;
            }
        }
     
        private void CloseMeter_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseEntityStats();
        }
    }
}