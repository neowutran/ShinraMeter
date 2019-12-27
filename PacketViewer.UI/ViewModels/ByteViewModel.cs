using DamageMeter.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PacketViewer.UI
{
    public class ByteViewModel : INotifyPropertyChanged
    {
        private byte[] _value;
        public string Hex => BitConverter.ToString(_value).Replace("-", string.Empty);
        public string Text => BuildString();

        private string BuildString()
        {
            var sb = new StringBuilder();
            foreach (var b in _value)
            {
                sb.Append(b > 0x21 && b < 0x80 ? (char)b : '⋅');
            }
            return sb.ToString();
        }
        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value) return;
                _isHovered = value;
                OnPropertyChanged((nameof(IsHovered)));
            }
        }

        public ByteViewModel(byte[] v)
        {
            _value = v;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(IsHovered));
        }
    }
}