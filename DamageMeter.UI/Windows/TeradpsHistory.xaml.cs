using System.Collections.Concurrent;
using System.Windows;
using DamageMeter.TeraDpsApi;
using Data;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour TeradpsHistory.xaml
    /// </summary>
    public partial class TeradpsHistory
    {
        public TeradpsHistory(ConcurrentDictionary<UploadData, NpcEntity> bossHistory)
        {
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            Update(bossHistory);
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            HideWindow();
        }

        public void Update(ConcurrentDictionary<UploadData, NpcEntity> bossHistory)
        {
            //TeraDpsHistory.Items.Clear();
            foreach (var boss in bossHistory) { TeraDpsHistory.Items.Add(new HistoryLink(boss.Key, boss.Value)); }
        }

        protected override bool Empty => !TeraDpsHistory.HasItems;
    }
}