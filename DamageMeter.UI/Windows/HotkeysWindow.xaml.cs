using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    /// Логика взаимодействия для HotkeysWindow.xaml
    /// </summary>
    public partial class HotkeysWindow : ClickThrouWindow
    {
        public HotkeysWindow()
        {  
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            BackgroundColor.Opacity = 0.75;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            
        }

        private void ClickThrouWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hk1.Content = $"Modifier: {BasicTeraData.Instance.HotkeysData.Paste.Value.ToString()} Key: {BasicTeraData.Instance.HotkeysData.Paste.Key.ToString()}";
            hk2.Content = $"Modifier: {BasicTeraData.Instance.HotkeysData.Reset.Value.ToString()} Key: {BasicTeraData.Instance.HotkeysData.Reset.Key.ToString()}";
            hk3.Content = $"Modifier: {BasicTeraData.Instance.HotkeysData.ResetCurrent.Value.ToString()} Key: {BasicTeraData.Instance.HotkeysData.ResetCurrent.Key.ToString()}";
            hk4.Content = $"Modifier: {BasicTeraData.Instance.HotkeysData.Topmost.Value.ToString()} Key: {BasicTeraData.Instance.HotkeysData.Topmost.Key.ToString()}";
            hk5.Content = $"Modifier: {BasicTeraData.Instance.HotkeysData.ExcelSave.Value.ToString()} Key: {BasicTeraData.Instance.HotkeysData.ExcelSave.Key.ToString()}";

        }
        private void Close_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
