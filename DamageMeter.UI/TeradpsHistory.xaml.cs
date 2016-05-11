using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System;
using System.Collections.Concurrent;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour TeradpsHistory.xaml
    /// </summary>
    public partial class TeradpsHistory
    {
        public TeradpsHistory(ConcurrentDictionary<string, Entity> bossHistory)
        {
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            Update(bossHistory);
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Update(ConcurrentDictionary<string, Entity> bossHistory)
        {
            TeraDpsHistory.Items.Clear();
            foreach(var boss in bossHistory)
            {
                TeraDpsHistory.Items.Add(new HistoryLink(boss.Key, boss.Value));
            }
        }
    
    }
}
