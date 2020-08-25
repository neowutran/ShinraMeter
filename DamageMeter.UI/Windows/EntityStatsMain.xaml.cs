using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DamageMeter.Database.Structures;
using Data;
using Tera.Game.Abnormality;

namespace DamageMeter.UI.EntityStats
{
    public partial class EntityStatsMain
    {
        private readonly List<EnduranceDebuff> _enduranceDebuffsList = new List<EnduranceDebuff>();

        private readonly EnduranceDebuffHeader _header;

        public EntityStatsMain()
        {
            Loaded += OnLoaded;
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            _header = new EnduranceDebuffHeader();
            EnduranceAbnormality.Items.Add(_header);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.LastSnappedPoint = BasicTeraData.Instance.WindowData.DebuffsStatus.Location;
            this.Left = this.LastSnappedPoint?.X ?? 0;
            this.Top = this.LastSnappedPoint?.Y ?? 0;
            this.Show();
            this.Hide();

            if (BasicTeraData.Instance.WindowData.DebuffsStatus.Visible) this.ShowWindow();

        }

        public void Update(EntityInformation entityInformation, AbnormalityStorage abnormals)
        {
            Dispatcher.Invoke(() =>
            {

                if (entityInformation == null) { return; }

                if (entityInformation.Interval == 0) { return; }

                var count = 0;
                foreach (var abnormality in abnormals.Get(entityInformation.Entity))
                {
                    EnduranceDebuff abnormalityUi;
                    if (_enduranceDebuffsList.Count > count) { abnormalityUi = _enduranceDebuffsList[count]; }
                    else
                    {
                        abnormalityUi = new EnduranceDebuff();
                        _enduranceDebuffsList.Add(abnormalityUi);
                    }

                    abnormalityUi.Update(abnormality.Key, abnormality.Value, entityInformation.BeginTime, entityInformation.EndTime);
                    count++;
                    if (EnduranceAbnormality.Items.Count <= count) EnduranceAbnormality.Items.Add(abnormalityUi);
                }

                while (EnduranceAbnormality.Items.Count > count + 1) EnduranceAbnormality.Items.RemoveAt(count + 1);
            });
        }
        protected override bool Empty => EnduranceAbnormality.Items.Count <= 1;

        private void CloseStats_OnClick(object sender, RoutedEventArgs e)
        {
            HideWindow();
        }
    }
}