using Data;
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

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour ChatMessageUI.xaml
    /// </summary>
    public partial class ChatMessageUI : UserControl
    {
        public ChatMessageUI(ChatMessage message)
        {
            InitializeComponent();
            Copy.Source = BasicTeraData.Instance.ImageDatabase.Copy.Source;
            Update(message);
        }

        public void Update(ChatMessage message)
        {
            Sender.Content = message.Sender;
            Message.Content = message.Text;
        }

        private void Copy_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText((string)Message.Content);
        }
    }
}
