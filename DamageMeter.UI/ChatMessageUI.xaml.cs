using System;
using System.Windows;
using System.Windows.Input;
using Data;
using System.Windows.Media;

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
            Message.Text = message.Text;

            Brush foreground = null;
            switch (message.ChatType)
            {
                case Chat.ChatType.Whisper:
                    foreground = new SolidColorBrush(Color.FromArgb(255, 230, 113, 184));
                    break;
                case Chat.ChatType.Normal:
                    foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    break;

            }

            Sender.Foreground = foreground;
            Message.Foreground = foreground;

        }

        private void Copy_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject(Message.Text);
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