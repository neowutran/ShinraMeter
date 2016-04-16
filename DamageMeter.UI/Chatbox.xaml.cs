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
using System.Windows.Shapes;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour Chatbox.xaml
    /// </summary>
    public partial class Chatbox
    {
        public Chatbox()
        {
            InitializeComponent();
        }

        public Chatbox(List<ChatMessage> chatbox)
        {
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
            Update(chatbox);
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Update(List<ChatMessage> chatbox)
        {
            for(int i = 0; i < chatbox.Count; i++)
            {

                if(ChatboxList.Items.Count > i)
                {
                    ((ChatMessageUI)ChatboxList.Items.GetItemAt(i)).Update(chatbox[i]);
                }
                else
                {
                    ChatboxList.Items.Add(new ChatMessageUI(chatbox[i]));
                }

            }
        }

      
    }
}
