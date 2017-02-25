using Data;

namespace DamageMeter.Processing
{
    internal class S_CREST_INFO
    {
        internal S_CREST_INFO(Tera.Game.Messages.S_CREST_INFO message) 
        {
            NetworkController.Instance.Glyphs.playerServer = BasicTeraData.Instance.Servers.GetServerName(NetworkController.Instance.EntityTracker.MeterUser.ServerId);
            NetworkController.Instance.Glyphs.playerName = NetworkController.Instance.EntityTracker.MeterUser.Name;
            NetworkController.Instance.Glyphs.playerClass = NetworkController.Instance.EntityTracker.MeterUser.RaceGenderClass.Class.ToString();
            NetworkController.Instance.Glyphs.glyphs = message.Glyphs;
        }
    }
}
