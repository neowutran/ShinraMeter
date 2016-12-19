using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamageMeter.Sniffing;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class C_CHECK_VERSION
    {

        internal C_CHECK_VERSION(Tera.Game.Messages.C_CHECK_VERSION message)
        {
            Debug.WriteLine("VERSION0 = " + message.Versions[0]);
//            Debug.WriteLine("VERSION1 = " + message.Versions[1]);
            var opCodeNamer =
                new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory,
                    $"data/opcodes/{message.Versions[0]}.txt"));
            OpCodeNamer sysMsgNamer=null;
            try //we we can have working opcodes from older KR versions included, but don't have sysmsg codes for them, todo: delete trycatch when we have all files for all upcoming versions.
            {sysMsgNamer =
                    new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory,
                        $"data/opcodes/smt_{message.Versions[0]}.txt"));}
            catch{}
            NetworkController.Instance.MessageFactory = new MessageFactory(opCodeNamer, NetworkController.Instance.Server.Region, false, sysMsgNamer);
            if (TeraSniffer.Instance.ClientProxyOverhead + TeraSniffer.Instance.ServerProxyOverhead > 0x1000)
                BasicTeraData.LogError("Client Proxy overhead: " + TeraSniffer.Instance.ClientProxyOverhead + "\r\nServer Proxy overhead: " + TeraSniffer.Instance.ServerProxyOverhead);
        }

    }
}
