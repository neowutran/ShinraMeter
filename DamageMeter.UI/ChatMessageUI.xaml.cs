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
            if (message.ChatType == Chat.ChatType.Normal) { Channel.Content = message.Channel; }
            else { Channel.Content = message.ChatType; }

            Time.Content = message.Time;

            Channel.Content = "[" + Channel.Content + "]";

            Brush foreground = message.ChatType switch
            {
                Chat.ChatType.Whisper => new SolidColorBrush(BasicTeraData.Instance.WindowData.WhisperColor),
                Chat.ChatType.Normal => message.Channel switch
                {
                    S_CHAT.ChannelEnum.Alliance => new SolidColorBrush(BasicTeraData.Instance.WindowData.AllianceColor),
                    S_CHAT.ChannelEnum.Area => new SolidColorBrush(BasicTeraData.Instance.WindowData.AreaColor),
                    S_CHAT.ChannelEnum.General => new SolidColorBrush(BasicTeraData.Instance.WindowData.GeneralColor),
                    S_CHAT.ChannelEnum.Group => new SolidColorBrush(BasicTeraData.Instance.WindowData.GroupColor),
                    S_CHAT.ChannelEnum.Guild => new SolidColorBrush(BasicTeraData.Instance.WindowData.GuildColor),
                    S_CHAT.ChannelEnum.Raid => new SolidColorBrush(BasicTeraData.Instance.WindowData.RaidColor),
                    S_CHAT.ChannelEnum.Say => new SolidColorBrush(BasicTeraData.Instance.WindowData.SayColor),
                    S_CHAT.ChannelEnum.Trading => new SolidColorBrush(BasicTeraData.Instance.WindowData.TradingColor),
                    S_CHAT.ChannelEnum.Emotes => new SolidColorBrush(BasicTeraData.Instance.WindowData.EmotesColor),
                    _ => Brushes.White
                },
                Chat.ChatType.PrivateChannel => new SolidColorBrush(BasicTeraData.Instance.WindowData.PrivateChannelColor),
                _ => null
            };

            Sender.Foreground = foreground;
            Channel.Foreground = foreground;
            Message.Foreground = foreground;
        }

        private void Copy_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject(Message.Text);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }

        private void Sender_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject(Sender.Content.ToString());
        }
    }
}