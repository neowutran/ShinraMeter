using System.Collections.Generic;

namespace DamageMeter.TeraDpsApi
{
    public class GlyphBuild
    {
        public string playerName;
        public string playerServer;
        public string playerClass;
        public uint playerId;
        public Dictionary<uint, bool> glyphs;
    }
}