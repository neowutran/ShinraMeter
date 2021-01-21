namespace DamageMeter.UI
{
    public class BlackListItemVM : TSPropertyChanged
    {
        private int _bossId;
        private int _areaId;

        public int BossId
        {
            get => _bossId;
            set
            {
                if (_bossId == value) return;
                _bossId = value;
                NotifyPropertyChanged();
            }
        }

        public int AreaId
        {
            get => _areaId;
            set
            {
                if (_areaId == value) return;
                _areaId = value;
                NotifyPropertyChanged();
            }
        }

        public BlackListItemVM(int areaId, int bossId)
        {
            _areaId = areaId;
            _bossId = bossId;
        }
    }
}