using System;
using Tera.Game;

namespace Tera.Sniffing
{
    public interface ITeraSniffer
    {
        bool Enabled { get; set; }
        event Action<Message> MessageReceived;
        event Action<Server> NewConnection;
    }
}