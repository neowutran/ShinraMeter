namespace ShinraMeter.D3D9Render.Overlays
{
    public abstract class Overlay
    {
        private int _id = -1;
        public int Id
        {
            get => _id;
            protected set => _id = value;
        }

        private bool _visible;
        public virtual bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        public virtual void Destroy()
        {
            _id = -1;
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Id}";
        }
    }
}
