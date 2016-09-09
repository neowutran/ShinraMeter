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
        private bool _updated = false;
     
        public Chatbox()
        {
            InitializeComponent();
            CloseWindow.Source = BasicTeraData.Instance.ImageDatabase.Close.Source;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Update(List<ChatMessage> chatbox)
        {
            if (_updated) return;
            _updated = true;
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