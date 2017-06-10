using System.Drawing;
using System.Runtime.InteropServices;

namespace DamageMeter.D3D9Render.Overlays
{
    public class TextLabel : Overlay
    {
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                if (base.Visible==value) return;
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
                if (_text==value)return;
                DxOverlay.TextSetStringUnicode(Id, value);
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
            Id = DxOverlay.TextCreateUnicode(font, size, type.HasFlag(TypeFace.Bold), type.HasFlag(TypeFace.Italic), position.X, position.Y, (uint)color.ToArgb(), text, shadow, show);
            //_font = font;
            //_size = size;
            //_type = type;
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
            DxOverlay.TextUpdateUnicode(Id, font, size, bold, italic);
        }
    }
}
