using System;
using System.Windows;
using System.Windows.Input;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour ChatMessageUI.xaml
    /// </summary>
    public partial class ChatMessageUi
    {
        public ChatMessageUi(ChatMessage message)
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
            Clipboard.SetDataObject((string) Message.Content);
        }

        private void Sender_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try
            {
                w?.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}