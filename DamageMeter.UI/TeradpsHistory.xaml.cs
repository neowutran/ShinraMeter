using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour TeradpsHistory.xaml
    /// </summary>
    public partial class TeradpsHistory
    {
        public TeradpsHistory(Dictionary<string, Entity> bossHistory)
        {
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            Update(bossHistory);
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Update(Dictionary<string, Entity> bossHistory)
        {
            TeraDpsHistory.Items.Clear();
            foreach(var boss in bossHistory)
            {
                var button = new Button();
                button.Style = FindResource("ShinraButtonStyle") as Style;
                button.Background = System.Windows.Media.Brushes.Transparent;
                button.Foreground = System.Windows.Media.Brushes.White;
                button.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                button.VerticalAlignment = VerticalAlignment.Top;
                button.Content = boss.Value.Name;
                if (boss.Key.StartsWith("!"))
                {
                    button.Foreground = System.Windows.Media.Brushes.Red;
                    button.ToolTip = boss.Key;
                    
                }
                button.Tag = boss.Key;
                button.Click += Button_Click;
                TeraDpsHistory.Items.Add(button);
            }
        }
    
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!((Button)sender).Tag.ToString().StartsWith("!"))
                Process.Start("explorer.exe", "http://teradps.io/party/rank/"+((Button)sender).Tag);
        }
    }
}
