using System;

namespace Data
{
    public class Section
    {
        public uint Id { get; }
        public uint NameId { get; }
        public string MapId { get; }
        public double Top { get; }
        public double Left { get; }
        public double Width { get; }
        public double Height { get; }
        public bool IsDungeon { get; }

       
        public Section(uint sId, uint sNameId, string mapId, double top, double left, double width, double height, bool dg)
        {
            Id = sId;
            NameId = sNameId;
            MapId = mapId;
            Top = top;
            Left = left;
            Width = width;
            Height = height;
            IsDungeon = dg;
        }

        public bool ContainsPoint(float x, float y)
        {
            var matchesY = y > Left &&  y < Width + Left;
            var matchesX = x < Top && x > Top - Height;

            return matchesX && matchesY;
        }
    }
}
