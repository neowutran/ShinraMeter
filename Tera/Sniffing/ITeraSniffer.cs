using System;
using System.Net;
using Tera.Game;

namespace Tera.Sniffing
{
    public interface ITeraSniffer
    {
        bool Enabled { get; set; }
        event Action<Server, IPEndPoint, IPEndPoint> NewConnection;
    }
}