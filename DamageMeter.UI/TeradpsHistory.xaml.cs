using System.Collections.Concurrent;
using System.Windows;
using Data;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour TeradpsHistory.xaml
    /// </summary>
    public partial class TeradpsHistory
    {
        public TeradpsHistory(ConcurrentDictionary<string, NpcEntity> bossHistory)
        {
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            Update(bossHistory);
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Update(ConcurrentDictionary<string, NpcEntity> bossHistory)
        {
            TeraDpsHistory.Items.Clear();
            foreach (var boss in bossHistory)
            {
                TeraDpsHistory.Items.Add(new HistoryLink(boss.Key, boss.Value));
            }
        }
    }
}