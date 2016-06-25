using System.Collections.Generic;
using System.Windows;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Chatbox.xaml
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
            for (var i = 0; i < chatbox.Count; i++)
            {
                if (ChatboxList.Items.Count > i)
                {
                    ((ChatMessageUi) ChatboxList.Items.GetItemAt(i)).Update(chatbox[i]);
                }
                else
                {
                    ChatboxList.Items.Add(new ChatMessageUi(chatbox[i]));
                }
            }
        }
    }
}