using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Data;
using Tera.Game.Messages;

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
            }
            else
            {
                Channel.Content = message.ChatType;
            }

            Time.Content = message.Time;

            Channel.Content = "[" + Channel.Content + "]";

            Brush foreground = null;
            switch (message.ChatType)
            {
                case Chat.ChatType.Whisper:
                    foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.WhisperColor);
                    break;
                case Chat.ChatType.Normal:
                    switch (message.Channel)
                    {
                        case S_CHAT.ChannelEnum.Alliance:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.AllianceColor);
                            break;
                        case S_CHAT.ChannelEnum.Area:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.AreaColor);
                            break;
                        case S_CHAT.ChannelEnum.General:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.GeneralColor);
                            break;
                        case S_CHAT.ChannelEnum.Group:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.GroupColor);
                            break;
                        case S_CHAT.ChannelEnum.Guild:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.GuildColor);
                            break;
                        case S_CHAT.ChannelEnum.Raid:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.RaidColor);
                            break;
                        case S_CHAT.ChannelEnum.Say:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.SayColor);
                            break;
                        case S_CHAT.ChannelEnum.Trading:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.TradingColor);
                            break;
                        case S_CHAT.ChannelEnum.Emotes:
                            foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.EmotesColor);
                            break;
                        default:
                            foreground = Brushes.White;
                            break;
                    }
                    break;
                case Chat.ChatType.PrivateChannel:
                    foreground = new SolidColorBrush(BasicTeraData.Instance.WindowData.PrivateChannelColor);
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

        private void DragWindow(object sender, MouseButtonEventArgs e)
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

        private void Sender_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject(Sender.Content.ToString());
        }
    }
}