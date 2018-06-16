using System.Collections.Generic;

namespace Data
{
    public class Guard
    {
        public Dictionary<uint, Section> Sections { get; set; }
        public uint Id { get; }
        public uint NameId { get; }
        public uint ContinentId { get; set; }
        public string MapId { get; }
        public double Top { get; }
        public double Left { get; }
        public double Width { get; }
        public double Height { get; }

        public Guard(uint gId, uint gNameId, string mapId, double left, double top, double width, double height )
        {
            Sections = new Dictionary<uint, Section>();
            Id = gId;
            NameId = gNameId;
            MapId = mapId;
            Top = top;
            Left = left;
            Width = width;
            Height = height;

        }

        public bool ContainsPoint(float x, float y)
        {
            var matchesY = y > Left && y < Width + Left;
            var matchesX = x < Top && x > Top - Height;
            return matchesX && matchesY;
        }
    }
}
