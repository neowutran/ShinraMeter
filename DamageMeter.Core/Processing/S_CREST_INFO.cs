using Data;

namespace DamageMeter.Processing
{
    internal class S_CREST_INFO
    {
        internal S_CREST_INFO(Tera.Game.Messages.S_CREST_INFO message)
        {
            PacketProcessor.Instance.Glyphs.playerServer =
                BasicTeraData.Instance.Servers.GetServerName(PacketProcessor.Instance.EntityTracker.MeterUser.ServerId);
            PacketProcessor.Instance.Glyphs.playerName = PacketProcessor.Instance.EntityTracker.MeterUser.Name;
            PacketProcessor.Instance.Glyphs.playerId = PacketProcessor.Instance.EntityTracker.MeterUser.PlayerId;
            PacketProcessor.Instance.Glyphs.playerClass = PacketProcessor.Instance.EntityTracker.MeterUser.RaceGenderClass.Class.ToString();
            PacketProcessor.Instance.Glyphs.glyphs = message.Glyphs;
        }
    }
}