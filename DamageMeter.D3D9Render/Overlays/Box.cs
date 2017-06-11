using System.Drawing;

namespace DamageMeter.D3D9Render.Overlays
{
    public class Box : Overlay
    {
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                DxOverlay.BoxSetShown(Id, value);
                base.Visible = value;
            }
        }

        private Color _color;

        public Color Color
        {
            get => _color;
            set
            {
                DxOverlay.BoxSetColor(Id, (uint) value.ToArgb());
                _color = value;
            }
        }

        private Rectangle _rectangle;

        public Rectangle Rectangle
        {
            get => _rectangle;
            set
            {
                DxOverlay.BoxSetPos(Id, value.X, value.Y);
                DxOverlay.BoxSetWidth(Id, value.Width);
                DxOverlay.BoxSetHeight(Id, value.Height);
                _rectangle = value;
            }
        }

        public int Height
        {
            get => _rectangle.Height;
            set
            {
                DxOverlay.BoxSetHeight(Id, value);
                _rectangle.Height = value;
            }
        }


        private bool _borderShown;

        public bool BorderShown
        {
            get => _borderShown;
            set
            {
                DxOverlay.BoxSetBorder(Id, _borderHeight, value);
                _borderShown = value;
            }
        }

        private int _borderHeight;

        public int BorderHeight
        {
            get => _borderHeight;
            set
            {
                DxOverlay.BoxSetBorder(Id, value, _borderShown);
                _borderHeight = value;
            }
        }

        private Color _borderColor;

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                DxOverlay.BoxSetBorderColor(Id, (uint) value.ToArgb());
                _borderColor = value;
            }
        }

        public Box(Rectangle rectangle, Color color, bool show, bool borderShown = false, int borderHeight = 0, Color borderColor = default(Color))
        {
            Id = DxOverlay.BoxCreate(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, (uint) color.ToArgb(), show);
            if (borderHeight != 0)
            {
                DxOverlay.BoxSetBorder(Id, borderHeight, borderShown);
                _borderHeight = borderHeight;
                _borderShown = borderShown;
            }
            if (borderColor != default(Color))
            {
                DxOverlay.BoxSetBorderColor(Id, (uint) borderColor.ToArgb());
                _borderColor = borderColor;
            }
            _rectangle = rectangle;
            base.Visible = show;
            _color = color;
        }

        public override void Destroy()
        {
            DxOverlay.BoxDestroy(Id);
            base.Destroy();
        }
    }
}
