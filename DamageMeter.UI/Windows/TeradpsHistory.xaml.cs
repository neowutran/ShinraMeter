using System.Collections.Concurrent;
using System.Windows;
using DamageMeter.TeraDpsApi;
using Data;
using Tera.Game;

namespace DamageMeter.UI
{
    public partial class TeradpsHistory
    {
        protected override bool Empty => !TeraDpsHistory.HasItems;

        public TeradpsHistory(ConcurrentDictionary<UploadData, NpcEntity> bossHistory)
        {
            Loaded += OnLoaded;
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            Update(bossHistory);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.LastSnappedPoint = BasicTeraData.Instance.WindowData.HistoryStatus.Location;
            this.Left = this.LastSnappedPoint?.X ?? 0;
            this.Top = this.LastSnappedPoint?.Y ?? 0;
            this.Show();
            this.Hide();
            if (BasicTeraData.Instance.WindowData.HistoryStatus.Visible) this.ShowWindow();

        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            HideWindow();
        }

        public void Update(ConcurrentDictionary<UploadData, NpcEntity> bossHistory)
        {
            Dispatcher.Invoke(() =>
            {
                TeraDpsHistory.Items.Clear();
                foreach (var boss in bossHistory) { TeraDpsHistory.Items.Add(new HistoryLink(boss.Key, boss.Value)); }
            });
        }

        public override void SaveWindowPos()
        {
            if (double.IsNaN(Left) || double.IsNaN(Top)) return;
            BasicTeraData.Instance.WindowData.HistoryStatus = new WindowStatus(LastSnappedPoint ?? new Point(Left, Top), Visible, Scale);
        }
    }
}