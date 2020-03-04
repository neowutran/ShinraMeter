using System.IO;
using Data;

namespace DamageMeter.Processing
{
    class C_LOGIN_ARBITER
    {
        internal C_LOGIN_ARBITER(Tera.Game.Messages.C_LOGIN_ARBITER message)
        {
            //if (OpcodeDownloader.DownloadSysmsg(PacketProcessor.Instance.MessageFactory.Version, PacketProcessor.Instance.MessageFactory.ReleaseVersion,
            //    Path.Combine(BasicTeraData.Instance.ResourceDirectory, $"data/opcodes/")))
            //{
            //    PacketProcessor.Instance.MessageFactory.ReloadSysMsg();
            //};  //////  Uncomment if start using sysmsg again
            BasicTeraData.Instance.Servers.Language = message.Language;
        }
    }
}
