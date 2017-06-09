using System.Drawing;

namespace DamageMeter.D3D9Render.Overlays
{
    public class Line : Overlay
    {
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                DxOverlay.LineSetShown(Id, value);
                base.Visible = value;
            }
        }

        private Color _color;

        public Color Color
        {
            get => _color;
            set
            {
                DxOverlay.LineSetColor(Id, (uint) value.ToArgb());
                _color = value;
            }
        }

        private int _width;

        public int Width
        {
            get => _width;
            set
            {
                DxOverlay.LineSetWidth(Id, value);
                _width = value;
            }
        }

        private Point _startPosition;

        public Point StartPosition
        {
            get => _startPosition;
            set
            {
                DxOverlay.LineSetPos(Id, value.X, value.Y, _endPosition.X, _endPosition.Y);
                _startPosition = value;
            }
        }

        private Point _endPosition;

        public Point EndPosition
        {
            get => _endPosition;
            set
            {
                DxOverlay.LineSetPos(Id, _startPosition.X, _startPosition.Y, value.X, value.Y);
                _endPosition = value;
            }
        }

        public Line(Point startPosition, Point endPosition, int width, Color color, bool show)
        {
            Id = DxOverlay.LineCreate(startPosition.X, startPosition.Y, endPosition.X, endPosition.Y, width, (uint) color.ToArgb(), show);
            _startPosition = startPosition;
            _endPosition = endPosition;
            _width = width;
            _color = color;
            base.Visible = show;
        }

        public override void Destroy()
        {
            DxOverlay.LineDestroy(Id);
            base.Destroy();
        }
    }
}