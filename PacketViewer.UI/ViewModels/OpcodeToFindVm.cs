using System.ComponentModel;
using System.Runtime.CompilerServices;
using DamageMeter.Properties;

namespace PacketViewer.UI
{
    public class OpcodeToFindVm : INotifyPropertyChanged
    {
        public string OpcodeName { get; }

        public OpcodeStatus Status => Mismatching != 0 ? OpcodeStatus.Mismatching : Confirmed ? OpcodeStatus.Confirmed : OpcodeStatus.None ;

        private bool _confirmed;
        public bool Confirmed
        {
            get => _confirmed;
            set
            {
                if (_confirmed == value) return;
                _confirmed = value;
                OnPropertyChanged(nameof(Confirmed));
                OnPropertyChanged(nameof(Status));
            }
        }
        private uint _mismatching;
        public uint Mismatching
        {
            get => _mismatching;
            set
            {
                if (_mismatching == value) return;
                _mismatching = value;
                OnPropertyChanged(nameof(Mismatching));
                OnPropertyChanged(nameof(Status));

            }
        }

        private uint _opcode;
        public uint Opcode
        {
            get => _opcode;
            set
            {
                if (_opcode == value) return;
                _opcode = value;
                OnPropertyChanged(nameof(Opcode));
                OnPropertyChanged(nameof(Status));

            }
        }

        public OpcodeToFindVm(string s, uint i)
        {
            Opcode = i;
            OpcodeName = s;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}