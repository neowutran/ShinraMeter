using System.Drawing;

namespace DamageMeter.D3D9Render.Overlays
{
    public class Image : Overlay
    {
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                DxOverlay.ImageSetShown(Id, value);
                base.Visible = value;
            }
        }

        private int _rotation;
        public int Rotation
        {
            get => _rotation;
            set
            {
                DxOverlay.ImageSetRotation(Id, value);
                _rotation = value;
            }
        }

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                DxOverlay.ImageSetPos(Id, value.X, value.Y);
                _position = value;
            }
        }

        private Align _align;
        public Align Align
        {
            get => _align;
            set
            {
                DxOverlay.ImageSetAlign(Id, (int)value);
                _align = value;
            }
        }

        public Image(string path, Point position, int rotation, Align align, bool show)
        {
            Id = DxOverlay.ImageCreate(path, position.X, position.Y, rotation, (int)align, show);
            _position = position;
            _rotation = rotation;
            _align = align;
            base.Visible = show;
        }

        public override void Destroy()
        {
            DxOverlay.ImageDestroy(Id);
            base.Destroy();
        }
    }
}
