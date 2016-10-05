using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class C_CHECK_VERSION
    {

        internal C_CHECK_VERSION(Tera.Game.Messages.C_CHECK_VERSION message)
        {
            Debug.WriteLine("VERSION0 = " + message.Versions[0]);
            Debug.WriteLine("VERSION1 = " + message.Versions[1]);
            var opCodeNamer =
                new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory,
                    $"data/opcodes/{message.Versions[0]}.txt"));
            NetworkController.Instance.MessageFactory = new MessageFactory(opCodeNamer, NetworkController.Instance.Server.Region);
        }

    }
}
