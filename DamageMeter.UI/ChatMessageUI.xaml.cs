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
            if (message.ChatType == Chat.ChatType.Normal)
            {
                Channel.Content = message.Channel;
            }else
            {
                Channel.Content = message.ChatType;
            }

            Channel.Content = "[" + Channel.Content + "]";
            
            Brush foreground = null;
            switch (message.ChatType)
            {
                case Chat.ChatType.Whisper:
                    foreground = Brushes.Pink;
                    break;
                case Chat.ChatType.Normal:
                    switch (message.Channel) {
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.Alliance:                   
                            foreground = Brushes.Green;
                            break;
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.Area:
                            foreground = Brushes.Purple;
                            break;
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.General:
                            foreground = Brushes.Yellow;
                            break;
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.Group:
                            foreground = Brushes.LightBlue;
                            break;
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.Guild:
                            foreground = Brushes.LightGreen;
                            break;
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.Raid:
                            foreground = Brushes.Orange;
                            break;
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.Say:
                            foreground = Brushes.White;
                            break;
                        case Tera.Game.Messages.S_CHAT.ChannelEnum.Trading:
                            foreground = Brushes.Sienna;
                            break;
                    }
                    break;
                case Chat.ChatType.PrivateChannel:
                    foreground = Brushes.Red;
                    break;


            }

            Sender.Foreground = foreground;
            Channel.Foreground = foreground;
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