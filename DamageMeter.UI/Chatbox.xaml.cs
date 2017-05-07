using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Chatbox.xaml
    /// </summary>
    public partial class Chatbox
    {
        private bool _updated;

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
            if (_updated)
            {
                return;
            }
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

        private void ChatboxList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ChatboxList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox) sender).SelectedItems.Count <= 1)
            {
                return;
            }
            var messages = "";
            foreach (var messageUi in ((ListBox) sender).SelectedItems.Cast<ChatMessageUi>()
                .OrderBy(x => x.Time.Content))
            {
                messages = messages +
                           $"{messageUi.Time.Content} {messageUi.Channel.Content} {messageUi.Sender.Content}: {messageUi.Message.Text}" +
                           Environment.NewLine;
            }
            Clipboard.SetDataObject(messages);
        }
    }
}