using DamageMeter.Properties;
using PacketViewer.Heuristic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PacketViewer.UI
{
    public class DatabaseVm : INotifyPropertyChanged
    {
        public ulong CurrentPlayerCid => DbUtils.GetPlayercId();
        public ObservableCollection<Npc> SpawnedNpcs => new ObservableCollection<Npc>(DbUtils.GetNpcList());
        public ObservableCollection<ulong> SpawnedUsers => new ObservableCollection<ulong>(DbUtils.GetUserList());
        public List<PartyMember> PartyMembers => DbUtils.GetPartyMembersList();

        public void RefreshDatabase()
        {
            OnPropertyChanged(nameof(CurrentPlayerCid));
            OnPropertyChanged(nameof(SpawnedUsers));
            OnPropertyChanged(nameof(SpawnedNpcs));
            OnPropertyChanged(nameof(PartyMembers));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
