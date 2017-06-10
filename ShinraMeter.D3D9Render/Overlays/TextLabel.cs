using System.Drawing;

namespace ShinraMeter.D3D9Render.Overlays
{
    public class TextLabel : Overlay
    {
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                DxOverlay.TextSetShown(Id, value);
                base.Visible = value;
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                DxOverlay.TextSetString(Id, value);
                _text = value;
            }
        }

        private bool _shadow;
        public bool Shadow
        {
            get => _shadow;
            set
            {
                DxOverlay.TextSetShadow(Id, value);
                _shadow = value;
            }
        }

        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                DxOverlay.TextSetColor(Id, (uint)value.ToArgb());
                _color = value;
            }
        }

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                DxOverlay.TextSetPos(Id, value.X, value.Y);
                _position = value;
            }
        }

        public TextLabel(string font, int size, TypeFace type, Point position, Color color, string text, bool shadow, bool show)
        {
            Id = DxOverlay.TextCreate(font, size, type.HasFlag(TypeFace.Bold), type.HasFlag(TypeFace.Italic), position.X, position.Y, (uint)color.ToArgb(), text, shadow, show);
            _text = text;
            _shadow = shadow;
            base.Visible = show;
            _color = color;
            _position = position;
        }

        public override void Destroy()
        {
            DxOverlay.TextDestroy(Id);
            base.Destroy();
        }

        public void TextUpdate(string font, int size, bool bold, bool italic)
        {
            DxOverlay.TextUpdate(Id, font, size, bold, italic);
        }
    }
}
