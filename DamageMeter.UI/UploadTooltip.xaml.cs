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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DamageMeter.TeraDpsApi;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logica di interazione per UploadTooltip.xaml
    /// </summary>
    public partial class UploadTooltip : UserControl
    {
        public UploadTooltip(UploadData data)
        {
            InitializeComponent();

            UpdateStatusTb.Text = data.Success ? "Upload successful" : "Upload failed";
            UpdateStatusTb.Foreground = App.Current.FindResource(data.Success ? "HealText" : "DamageText") as SolidColorBrush;

            TimeTb.Text = data.Time.ToShortTimeString() + " " + data.Time.ToShortDateString();

            MessageTb.Text = data.Message;

            NpcNameTb.Text = data.Npc;

            ServerTb.Text = data.Server;

            ExceptionGrid.Visibility = string.IsNullOrEmpty(data.Exception) ? Visibility.Collapsed : Visibility.Visible;

            ExceptionTb.Text = data.Exception;

        }
    }
}
