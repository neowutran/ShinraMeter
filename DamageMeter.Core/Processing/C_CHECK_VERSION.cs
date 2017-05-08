using System.Diagnostics;
using System.IO;
using DamageMeter.Sniffing;
using Data;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class C_CHECK_VERSION
    {
        internal C_CHECK_VERSION(Tera.Game.Messages.C_CHECK_VERSION message)
        {
            Debug.WriteLine("VERSION0 = " + message.Versions[0]);
//            Debug.WriteLine("VERSION1 = " + message.Versions[1]);
            var opCodeNamer = new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory, $"data/opcodes/{message.Versions[0]}.txt"));
            var sysMsgNamer = new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory, $"data/opcodes/smt_{message.Versions[0]}.txt"));
            TeraSniffer.Instance.Connected = true;
            NetworkController.Instance.MessageFactory = new MessageFactory(opCodeNamer, NetworkController.Instance.Server.Region, message.Versions[0], false,
                sysMsgNamer);
            if (TeraSniffer.Instance.ClientProxyOverhead + TeraSniffer.Instance.ServerProxyOverhead > 0x1000)
            {
                BasicTeraData.LogError("Client Proxy overhead: " + TeraSniffer.Instance.ClientProxyOverhead + "\r\nServer Proxy overhead: " +
                                       TeraSniffer.Instance.ServerProxyOverhead);
            }
        }
    }
}